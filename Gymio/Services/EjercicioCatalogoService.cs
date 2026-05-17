using Gymio.Interfaces;
using Gymio.Models;
using MongoDB.Driver;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Gymio.Services
{
    public class EjercicioCatalogoService : IEjercicioCatalogoService
    {
        private const int EjerciciosMinimosConGif = 120;
        private static readonly SemaphoreSlim CatalogoLock = new(1, 1);
        private static bool sincronizacionExternaIntentada;

        private readonly IMongoCollection<EjercicioCatalogo> _ejercicios;
        private readonly IHttpClientFactory _httpClientFactory;

        public EjercicioCatalogoService(IMongoDatabase database, IHttpClientFactory httpClientFactory)
        {
            _ejercicios = database.GetCollection<EjercicioCatalogo>("EjerciciosCatalogo");
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<EjercicioCatalogo>> BuscarEjerciciosAsync(string? termino, string? grupoMuscular, string? equipo, int limite = 80)
        {
            await AsegurarCatalogoBaseAsync();

            var filtros = new List<FilterDefinition<EjercicioCatalogo>>
            {
                Builders<EjercicioCatalogo>.Filter.Eq(e => e.Activo, true)
            };

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(termino.Trim(), "i");
                filtros.Add(Builders<EjercicioCatalogo>.Filter.Or(
                    Builders<EjercicioCatalogo>.Filter.Regex(e => e.Nombre, regex),
                    Builders<EjercicioCatalogo>.Filter.Regex(e => e.GrupoMuscularPrincipal, regex),
                    Builders<EjercicioCatalogo>.Filter.Regex(e => e.Equipo, regex),
                    Builders<EjercicioCatalogo>.Filter.Regex(e => e.PatronMovimiento, regex)
                ));
            }

            if (!string.IsNullOrWhiteSpace(grupoMuscular))
            {
                filtros.Add(Builders<EjercicioCatalogo>.Filter.Eq(e => e.GrupoMuscularPrincipal, grupoMuscular));
            }

            if (!string.IsNullOrWhiteSpace(equipo))
            {
                filtros.Add(Builders<EjercicioCatalogo>.Filter.Eq(e => e.Equipo, equipo));
            }

            var filtroFinal = Builders<EjercicioCatalogo>.Filter.And(filtros);

            return await _ejercicios
                .Find(filtroFinal)
                .SortBy(e => e.GrupoMuscularPrincipal)
                .ThenBy(e => e.Nombre)
                .Limit(limite)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerGruposMuscularesAsync()
        {
            await AsegurarCatalogoBaseAsync();

            return await _ejercicios
                .Distinct(e => e.GrupoMuscularPrincipal, Builders<EjercicioCatalogo>.Filter.Eq(e => e.Activo, true))
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerEquiposAsync()
        {
            await AsegurarCatalogoBaseAsync();

            return await _ejercicios
                .Distinct(e => e.Equipo, Builders<EjercicioCatalogo>.Filter.Eq(e => e.Activo, true))
                .ToListAsync();
        }

        public async Task<EjercicioCatalogo?> ObtenerPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            await AsegurarCatalogoBaseAsync();
            return await _ejercicios.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<EjercicioCatalogo?> ResolverVisualPorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            await AsegurarCatalogoBaseAsync();

            var nombreNormalizado = Normalizar(nombre);
            var exacto = await _ejercicios
                .Find(e => e.Activo && e.NombreNormalizado == nombreNormalizado)
                .FirstOrDefaultAsync();

            if (TieneGif(exacto))
            {
                return exacto;
            }

            var terminoBusqueda = TraducirTerminoBusqueda(nombre);
            var candidatos = await BuscarEjerciciosAsync(terminoBusqueda, null, null, 10);
            var candidatoConGif = candidatos.FirstOrDefault(TieneGif);
            if (candidatoConGif != null)
            {
                return candidatoConGif;
            }

            var gifSugerido = GifFallbackPorNombre(nombre);
            if (string.IsNullOrWhiteSpace(gifSugerido))
            {
                return exacto;
            }

            return new EjercicioCatalogo
            {
                Nombre = nombre,
                NombreNormalizado = nombreNormalizado,
                GrupoMuscularPrincipal = exacto?.GrupoMuscularPrincipal ?? "General",
                Equipo = exacto?.Equipo ?? "Varios",
                Nivel = exacto?.Nivel ?? "General",
                Instrucciones = exacto?.Instrucciones ?? "",
                ErroresComunes = exacto?.ErroresComunes ?? "",
                GifUrl = gifSugerido,
                Fuente = "Gymio fallback"
            };
        }

        public async Task<EjercicioCatalogo> CrearEjercicioAsync(EjercicioCatalogo ejercicio)
        {
            PrepararEjercicio(ejercicio, string.IsNullOrWhiteSpace(ejercicio.Fuente) ? "Manual" : ejercicio.Fuente);

            await _ejercicios.InsertOneAsync(ejercicio);
            return ejercicio;
        }

        private async Task AsegurarCatalogoBaseAsync()
        {
            await CatalogoLock.WaitAsync();
            try
            {
                await UpsertCatalogoLocalAsync();

                if (sincronizacionExternaIntentada)
                {
                    return;
                }

                var totalConGif = await _ejercicios.CountDocumentsAsync(e => e.Activo && e.GifUrl != "");
                if (totalConGif >= EjerciciosMinimosConGif)
                {
                    return;
                }

                sincronizacionExternaIntentada = true;
                await ImportarExerciseDbAsync();
            }
            finally
            {
                CatalogoLock.Release();
            }
        }

        private async Task UpsertCatalogoLocalAsync()
        {
            foreach (var ejercicio in CatalogoBase())
            {
                PrepararEjercicio(ejercicio, "Gymio");

                var existente = await _ejercicios
                    .Find(e => e.Nombre == ejercicio.Nombre || e.NombreNormalizado == ejercicio.NombreNormalizado)
                    .FirstOrDefaultAsync();

                if (existente == null)
                {
                    await _ejercicios.InsertOneAsync(ejercicio);
                    continue;
                }

                var update = Builders<EjercicioCatalogo>.Update
                    .Set(e => e.NombreNormalizado, ejercicio.NombreNormalizado)
                    .Set(e => e.Activo, true)
                    .Set(e => e.FechaActualizacion, DateTime.Now);

                if (string.IsNullOrWhiteSpace(existente.GifUrl) && !string.IsNullOrWhiteSpace(ejercicio.GifUrl))
                {
                    update = update.Set(e => e.GifUrl, ejercicio.GifUrl);
                }

                if (string.IsNullOrWhiteSpace(existente.Instrucciones) && !string.IsNullOrWhiteSpace(ejercicio.Instrucciones))
                {
                    update = update.Set(e => e.Instrucciones, ejercicio.Instrucciones);
                }

                if (string.IsNullOrWhiteSpace(existente.ErroresComunes) && !string.IsNullOrWhiteSpace(ejercicio.ErroresComunes))
                {
                    update = update.Set(e => e.ErroresComunes, ejercicio.ErroresComunes);
                }

                if (string.IsNullOrWhiteSpace(existente.Fuente))
                {
                    update = update.Set(e => e.Fuente, ejercicio.Fuente);
                }

                await _ejercicios.UpdateOneAsync(e => e.Id == existente.Id, update);
            }
        }

        private async Task ImportarExerciseDbAsync()
        {
            var http = _httpClientFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(20);

            foreach (var bodyPart in BodyPartsExerciseDb())
            {
                try
                {
                    var url = $"https://oss.exercisedb.dev/api/v1/exercises?limit=25&bodyParts={WebUtility.UrlEncode(bodyPart)}";
                    var respuesta = await http.GetFromJsonAsync<ExerciseDbResponse>(url);
                    if (respuesta?.Data == null || respuesta.Data.Count == 0)
                    {
                        continue;
                    }

                    var escrituras = new List<WriteModel<EjercicioCatalogo>>();

                    foreach (var item in respuesta.Data)
                    {
                        var ejercicio = DesdeExerciseDb(item);
                        if (string.IsNullOrWhiteSpace(ejercicio.ExternalId) || string.IsNullOrWhiteSpace(ejercicio.GifUrl))
                        {
                            continue;
                        }

                        var filtro = Builders<EjercicioCatalogo>.Filter.And(
                            Builders<EjercicioCatalogo>.Filter.Eq(e => e.Fuente, "ExerciseDB"),
                            Builders<EjercicioCatalogo>.Filter.Eq(e => e.ExternalId, ejercicio.ExternalId)
                        );

                        var update = Builders<EjercicioCatalogo>.Update
                            .Set(e => e.Nombre, ejercicio.Nombre)
                            .Set(e => e.NombreNormalizado, ejercicio.NombreNormalizado)
                            .Set(e => e.GrupoMuscularPrincipal, ejercicio.GrupoMuscularPrincipal)
                            .Set(e => e.MusculosSecundarios, ejercicio.MusculosSecundarios)
                            .Set(e => e.Equipo, ejercicio.Equipo)
                            .Set(e => e.Nivel, ejercicio.Nivel)
                            .Set(e => e.PatronMovimiento, ejercicio.PatronMovimiento)
                            .Set(e => e.Instrucciones, ejercicio.Instrucciones)
                            .Set(e => e.ErroresComunes, ejercicio.ErroresComunes)
                            .Set(e => e.GifUrl, ejercicio.GifUrl)
                            .Set(e => e.VideoUrl, ejercicio.VideoUrl)
                            .Set(e => e.Activo, true)
                            .Set(e => e.Fuente, ejercicio.Fuente)
                            .Set(e => e.ExternalId, ejercicio.ExternalId)
                            .Set(e => e.FechaActualizacion, DateTime.Now)
                            .SetOnInsert(e => e.FechaCreacion, DateTime.Now);

                        escrituras.Add(new UpdateOneModel<EjercicioCatalogo>(filtro, update) { IsUpsert = true });
                    }

                    if (escrituras.Count > 0)
                    {
                        await _ejercicios.BulkWriteAsync(escrituras, new BulkWriteOptions { IsOrdered = false });
                    }

                    await Task.Delay(250);
                }
                catch
                {
                    // Si el API externo falla o limita la tasa, la app sigue funcionando con el catalogo local.
                }
            }
        }

        private static EjercicioCatalogo DesdeExerciseDb(ExerciseDbItem item)
        {
            var nombre = FormatearNombreExerciseDb(item.Name);
            var ejercicio = new EjercicioCatalogo
            {
                Nombre = nombre,
                GrupoMuscularPrincipal = MapearGrupoMuscular(item),
                MusculosSecundarios = item.SecondaryMuscles.Select(MapearMusculo).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList(),
                Equipo = MapearEquipo(item.Equipments.FirstOrDefault() ?? ""),
                Nivel = "General",
                PatronMovimiento = MapearPatron(item),
                Instrucciones = item.Instructions.Count == 0
                    ? "Revisa el GIF, controla el movimiento y usa un rango comodo antes de aumentar carga."
                    : string.Join(" ", item.Instructions),
                ErroresComunes = "Usar impulso, perder postura o recortar el recorrido por levantar demasiado peso.",
                GifUrl = item.GifUrl,
                Fuente = "ExerciseDB",
                ExternalId = item.ExerciseId
            };

            PrepararEjercicio(ejercicio, "ExerciseDB");
            return ejercicio;
        }

        private static void PrepararEjercicio(EjercicioCatalogo ejercicio, string fuente)
        {
            ejercicio.Nombre = ejercicio.Nombre.Trim();
            ejercicio.GrupoMuscularPrincipal = string.IsNullOrWhiteSpace(ejercicio.GrupoMuscularPrincipal)
                ? "General"
                : ejercicio.GrupoMuscularPrincipal.Trim();
            ejercicio.Equipo = string.IsNullOrWhiteSpace(ejercicio.Equipo) ? "Varios" : ejercicio.Equipo.Trim();
            ejercicio.Nivel = string.IsNullOrWhiteSpace(ejercicio.Nivel) ? "General" : ejercicio.Nivel.Trim();
            ejercicio.Fuente = fuente;
            ejercicio.NombreNormalizado = Normalizar(ejercicio.Nombre);
            ejercicio.FechaActualizacion = DateTime.Now;
            ejercicio.FechaCreacion = ejercicio.FechaCreacion == default ? DateTime.Now : ejercicio.FechaCreacion;
            ejercicio.Activo = true;

            if (string.IsNullOrWhiteSpace(ejercicio.GifUrl))
            {
                ejercicio.GifUrl = GifFallbackPorNombre(ejercicio.Nombre);
            }
        }

        private static bool TieneGif(EjercicioCatalogo? ejercicio)
        {
            return ejercicio != null && !string.IsNullOrWhiteSpace(ejercicio.GifUrl);
        }

        private static string Normalizar(string texto)
        {
            var formD = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(formD.Length);

            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
                }
            }

            return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string TraducirTerminoBusqueda(string nombre)
        {
            var normalizado = Normalizar(nombre);

            if (normalizado.Contains("sentadilla")) return "squat";
            if (normalizado.Contains("banca") || normalizado.Contains("pectoral")) return "bench press";
            if (normalizado.Contains("peso muerto")) return "deadlift";
            if (normalizado.Contains("jalon")) return "pulldown";
            if (normalizado.Contains("remo")) return "row";
            if (normalizado.Contains("militar") || normalizado.Contains("press hombro")) return "shoulder press";
            if (normalizado.Contains("lateral")) return "lateral raise";
            if (normalizado.Contains("curl")) return "curl";
            if (normalizado.Contains("triceps") || normalizado.Contains("pushdown")) return "pushdown";
            if (normalizado.Contains("hip thrust") || normalizado.Contains("puente")) return "hip raise";
            if (normalizado.Contains("prensa")) return "leg press";
            if (normalizado.Contains("plancha")) return "plank";
            if (normalizado.Contains("dominada")) return "pull up";
            if (normalizado.Contains("zancada") || normalizado.Contains("lunge")) return "lunge";
            if (normalizado.Contains("pantorrilla")) return "calf raise";

            return nombre;
        }

        private static string GifFallbackPorNombre(string nombre)
        {
            var normalizado = Normalizar(nombre);

            if (normalizado.Contains("sentadilla")) return "https://static.exercisedb.dev/media/qXTaZnJ.gif";
            if (normalizado.Contains("banca") || normalizado.Contains("press pecho")) return "https://static.exercisedb.dev/media/EIeI8Vf.gif";
            if (normalizado.Contains("peso muerto")) return "https://static.exercisedb.dev/media/ila4NZS.gif";
            if (normalizado.Contains("jalon")) return "https://static.exercisedb.dev/media/RVwzP10.gif";
            if (normalizado.Contains("remo")) return "https://static.exercisedb.dev/media/fUBheHs.gif";
            if (normalizado.Contains("militar") || normalizado.Contains("press hombro")) return "https://static.exercisedb.dev/media/Xy4jlWA.gif";
            if (normalizado.Contains("elevacion lateral") || normalizado.Contains("lateral")) return "https://static.exercisedb.dev/media/DsgkuIt.gif";
            if (normalizado.Contains("curl")) return "https://static.exercisedb.dev/media/6TG6x2w.gif";
            if (normalizado.Contains("triceps") || normalizado.Contains("pushdown")) return "https://static.exercisedb.dev/media/3ZflifB.gif";
            if (normalizado.Contains("hip thrust") || normalizado.Contains("puente")) return "https://static.exercisedb.dev/media/196HJGw.gif";
            if (normalizado.Contains("prensa")) return "https://static.exercisedb.dev/media/V07qpXy.gif";
            if (normalizado.Contains("plancha")) return "https://static.exercisedb.dev/media/hCjGsRQ.gif";

            return string.Empty;
        }

        private static string FormatearNombreExerciseDb(string nombre)
        {
            var texto = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(nombre.Replace("-", " ").Trim());
            return texto
                .Replace("Barbell", "Barra")
                .Replace("Dumbbell", "Mancuerna")
                .Replace("Cable", "Polea")
                .Replace("Lever", "Maquina")
                .Replace("Bodyweight", "Peso Corporal")
                .Replace("Bench Press", "Press Banca")
                .Replace("Squat", "Sentadilla")
                .Replace("Deadlift", "Peso Muerto")
                .Replace("Pulldown", "Jalon")
                .Replace("Pull Down", "Jalon")
                .Replace("Row", "Remo")
                .Replace("Shoulder Press", "Press Hombro")
                .Replace("Lateral Raise", "Elevacion Lateral")
                .Replace("Leg Press", "Prensa de Piernas")
                .Replace("Calf Raise", "Elevacion de Pantorrilla")
                .Replace("Pull Up", "Dominada")
                .Replace("Push Up", "Flexion")
                .Replace("Plank", "Plancha");
        }

        private static string MapearGrupoMuscular(ExerciseDbItem item)
        {
            var tokens = item.TargetMuscles.Concat(item.BodyParts).Select(t => t.ToLowerInvariant()).ToList();

            if (tokens.Any(t => t.Contains("pectorals") || t.Contains("chest"))) return "Pecho";
            if (tokens.Any(t => t.Contains("lats") || t.Contains("upper back") || t.Contains("traps") || t.Contains("back"))) return "Espalda";
            if (tokens.Any(t => t.Contains("delts") || t.Contains("shoulders"))) return "Hombro";
            if (tokens.Any(t => t.Contains("biceps"))) return "Biceps";
            if (tokens.Any(t => t.Contains("triceps"))) return "Triceps";
            if (tokens.Any(t => t.Contains("forearms") || t.Contains("lower arms"))) return "Antebrazo";
            if (tokens.Any(t => t.Contains("abs") || t.Contains("obliques") || t.Contains("waist"))) return "Core";
            if (tokens.Any(t => t.Contains("glutes"))) return "Gluteos";
            if (tokens.Any(t => t.Contains("hamstrings"))) return "Femoral";
            if (tokens.Any(t => t.Contains("quads"))) return "Cuadriceps";
            if (tokens.Any(t => t.Contains("calves") || t.Contains("lower legs"))) return "Pantorrilla";
            if (tokens.Any(t => t.Contains("cardio"))) return "Cardio";
            if (tokens.Any(t => t.Contains("upper legs"))) return "Pierna";

            return "General";
        }

        private static string MapearMusculo(string musculo)
        {
            var t = musculo.ToLowerInvariant();

            if (t.Contains("pectorals")) return "Pecho";
            if (t.Contains("lats") || t.Contains("back") || t.Contains("traps")) return "Espalda";
            if (t.Contains("delts")) return "Hombro";
            if (t.Contains("biceps")) return "Biceps";
            if (t.Contains("triceps")) return "Triceps";
            if (t.Contains("forearms")) return "Antebrazo";
            if (t.Contains("abs") || t.Contains("obliques")) return "Core";
            if (t.Contains("glutes")) return "Gluteos";
            if (t.Contains("hamstrings")) return "Femoral";
            if (t.Contains("quads")) return "Cuadriceps";
            if (t.Contains("calves")) return "Pantorrilla";

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(musculo);
        }

        private static string MapearEquipo(string equipo)
        {
            return equipo.ToLowerInvariant() switch
            {
                "body weight" => "Peso corporal",
                "dumbbell" => "Mancuernas",
                "barbell" => "Barra",
                "ez barbell" => "Barra Z",
                "cable" => "Polea",
                "leverage machine" or "lever" => "Maquina",
                "smith machine" => "Smith",
                "resistance band" or "band" => "Banda",
                "kettlebell" => "Kettlebell",
                "medicine ball" => "Balon medicinal",
                "stability ball" => "Pelota suiza",
                "sled machine" => "Maquina",
                "stationary bike" => "Bicicleta",
                "elliptical machine" => "Eliptica",
                "" => "Varios",
                _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(equipo)
            };
        }

        private static string MapearPatron(ExerciseDbItem item)
        {
            var nombre = item.Name.ToLowerInvariant();

            if (nombre.Contains("squat") || nombre.Contains("lunge") || nombre.Contains("leg press")) return "Dominante de rodilla";
            if (nombre.Contains("deadlift") || nombre.Contains("hip") || nombre.Contains("glute")) return "Bisagra de cadera";
            if (nombre.Contains("press") || nombre.Contains("push")) return "Empuje";
            if (nombre.Contains("row")) return "Jalon horizontal";
            if (nombre.Contains("pulldown") || nombre.Contains("pull-up") || nombre.Contains("chin-up")) return "Jalon vertical";
            if (nombre.Contains("curl")) return "Flexion de codo";
            if (nombre.Contains("extension") || nombre.Contains("pushdown")) return "Extension de codo";
            if (nombre.Contains("plank") || nombre.Contains("crunch")) return "Core";
            if (nombre.Contains("raise")) return "Elevacion";

            return "General";
        }

        private static IReadOnlyList<string> BodyPartsExerciseDb()
        {
            return new[]
            {
                "chest",
                "back",
                "shoulders",
                "upper arms",
                "lower arms",
                "upper legs",
                "lower legs",
                "waist",
                "cardio"
            };
        }

        private static List<EjercicioCatalogo> CatalogoBase()
        {
            return new List<EjercicioCatalogo>
            {
                Ejercicio("Press banca con barra", "Pecho", "Barra", "Intermedio", "Empuje horizontal", "Acuestate con los ojos debajo de la barra. Retrae escapulas, apoya firme los pies y baja la barra al pecho con control. Empuja manteniendo hombros estables.", "Rebotar la barra, despegar cadera del banco, abrir demasiado los codos.", "Triceps", "Deltoide anterior"),
                Ejercicio("Press inclinado con mancuernas", "Pecho", "Mancuernas", "Intermedio", "Empuje inclinado", "Ajusta el banco inclinado, baja las mancuernas a los lados del pecho y empuja sin perder control escapular.", "Chocar mancuernas arriba, arquear lumbar o bajar sin control.", "Triceps", "Deltoide anterior"),
                Ejercicio("Aperturas en polea", "Pecho", "Polea", "Principiante", "Aduccion horizontal", "Da un paso al frente, deja codos suaves y junta las manos frente al pecho manteniendo tension continua.", "Doblar demasiado los codos o convertirlo en press.", "Deltoide anterior"),
                Ejercicio("Fondos para pecho", "Pecho", "Peso corporal", "Intermedio", "Empuje vertical", "Inclina ligeramente el torso, baja hasta rango comodo y empuja separando el piso con las manos.", "Bajar con dolor, encoger hombros o balancearse.", "Triceps", "Hombro"),

                Ejercicio("Sentadilla libre", "Cuadriceps", "Barra", "Intermedio", "Dominante de rodilla", "Coloca la barra estable sobre la espalda alta, respira profundo, baja con rodillas siguiendo la punta del pie y sube empujando el piso.", "Colapsar rodillas hacia adentro, perder neutralidad lumbar, levantar talones.", "Gluteos", "Core"),
                Ejercicio("Prensa de piernas", "Cuadriceps", "Maquina", "Principiante", "Dominante de rodilla", "Coloca pies firmes, baja la plataforma hasta un rango comodo y empuja sin bloquear agresivo las rodillas.", "Despegar cadera del respaldo, juntar rodillas, recortar demasiado el recorrido.", "Gluteos", "Femoral"),
                Ejercicio("Zancadas caminando", "Cuadriceps", "Mancuernas", "Principiante", "Unilateral", "Da pasos largos y controlados, baja la rodilla trasera y empuja con la pierna frontal.", "Inclinarse demasiado o perder estabilidad de rodilla.", "Gluteos", "Core"),
                Ejercicio("Extension de piernas", "Cuadriceps", "Maquina", "Principiante", "Extension de rodilla", "Ajusta el rodillo sobre el empeine, extiende rodillas y baja controlado manteniendo cadera pegada.", "Usar impulso o despegar la cadera del asiento."),

                Ejercicio("Peso muerto rumano", "Femoral", "Barra", "Intermedio", "Bisagra de cadera", "Con rodillas semi flexionadas, lleva la cadera hacia atras y baja la barra pegada al cuerpo hasta sentir estiramiento en femorales. Sube extendiendo cadera.", "Convertirlo en sentadilla, redondear espalda, separar demasiado la barra.", "Gluteos", "Espalda baja"),
                Ejercicio("Curl femoral acostado", "Femoral", "Maquina", "Principiante", "Flexion de rodilla", "Ajusta el rodillo sobre los tobillos, flexiona rodillas y baja lento sin despegar cadera.", "Arquear lumbar o soltar el peso rapido."),
                Ejercicio("Hip thrust con barra", "Gluteos", "Barra", "Intermedio", "Extension de cadera", "Apoya la espalda alta en el banco, menton ligeramente abajo y sube la cadera hasta alinear torso y muslos. Pausa arriba.", "Hiperextender lumbar, empujar desde puntas, no completar extension.", "Femoral", "Core"),
                Ejercicio("Patada de gluteo en polea", "Gluteos", "Polea", "Principiante", "Extension de cadera", "Fija el torso, empuja el talon hacia atras y arriba sin girar la cadera.", "Balancear la espalda o usar demasiado peso.", "Femoral"),

                Ejercicio("Jalon al pecho", "Espalda", "Polea", "Principiante", "Jalon vertical", "Sujeta la barra con agarre comodo, baja los hombros y lleva los codos hacia las costillas. Controla la subida sin perder tension.", "Tirar con brazos solamente, balancear el torso, llevar la barra detras de la nuca.", "Biceps", "Deltoide posterior"),
                Ejercicio("Remo sentado en polea", "Espalda", "Polea", "Principiante", "Jalon horizontal", "Mantente erguido, inicia con escapulas y lleva el agarre hacia el abdomen. Pausa un segundo y regresa controlado.", "Jalar con impulso, encoger hombros, redondear espalda.", "Biceps", "Trapecio medio"),
                Ejercicio("Dominadas", "Espalda", "Peso corporal", "Intermedio", "Jalon vertical", "Cuelga con hombros activos, lleva el pecho hacia la barra y baja controlado.", "Balancearse, acortar recorrido o encoger hombros.", "Biceps", "Core"),
                Ejercicio("Remo con mancuerna", "Espalda", "Mancuernas", "Principiante", "Jalon horizontal", "Apoya una mano en banco, lleva el codo hacia la cadera y baja con control.", "Rotar demasiado el torso o jalar con biceps.", "Biceps"),

                Ejercicio("Press militar con mancuernas", "Hombro", "Mancuernas", "Intermedio", "Empuje vertical", "Lleva las mancuernas a la altura de hombros, aprieta abdomen y empuja arriba sin chocar las pesas. Baja con control.", "Arquear demasiado la espalda, perder control al bajar, bloquear con dolor.", "Triceps", "Core"),
                Ejercicio("Elevaciones laterales", "Hombro", "Mancuernas", "Principiante", "Abduccion de hombro", "Con codos suaves, eleva las mancuernas hacia los lados hasta la linea del hombro. Baja lento y evita impulso.", "Subir con trapecio, balancear el cuerpo, usar demasiado peso.", "Trapecio superior"),
                Ejercicio("Face pull", "Hombro", "Polea", "Principiante", "Traccion escapular", "Tira la cuerda hacia el rostro separando manos y manteniendo codos altos.", "Arquear lumbar o convertirlo en remo pesado.", "Deltoide posterior", "Trapecio"),
                Ejercicio("Pajaros con mancuernas", "Hombro", "Mancuernas", "Principiante", "Abduccion posterior", "Inclina el torso y abre los brazos hasta sentir el deltoide posterior.", "Usar impulso o cerrar demasiado los codos.", "Espalda alta"),

                Ejercicio("Curl de biceps con barra Z", "Biceps", "Barra Z", "Principiante", "Flexion de codo", "Mantén codos cerca del torso, sube la barra sin balancear y aprieta arriba. Baja completo con control.", "Mover hombros, acortar recorrido, usar impulso de cadera.", "Antebrazo"),
                Ejercicio("Curl inclinado con mancuernas", "Biceps", "Mancuernas", "Intermedio", "Flexion de codo", "Recuestate en banco inclinado, deja el brazo atras y flexiona sin mover el hombro.", "Balancear el brazo o cortar el estiramiento.", "Antebrazo"),
                Ejercicio("Curl martillo", "Biceps", "Mancuernas", "Principiante", "Flexion neutra", "Sube las mancuernas con agarre neutro y codos fijos al costado.", "Girar la muñeca o usar impulso.", "Antebrazo"),
                Ejercicio("Curl predicador", "Biceps", "Maquina", "Principiante", "Flexion de codo", "Apoya brazos en el banco, extiende sin bloquear agresivo y sube con control.", "Despegar codos del apoyo o soltar la bajada.", "Antebrazo"),

                Ejercicio("Pushdown de triceps", "Triceps", "Polea", "Principiante", "Extension de codo", "Fija los codos al costado y extiende hasta bloquear suave. Regresa hasta sentir estiramiento sin mover hombros.", "Abrir codos, inclinarse demasiado, cortar la fase negativa.", "Antebrazo"),
                Ejercicio("Extension de triceps sobre cabeza", "Triceps", "Polea", "Principiante", "Extension de codo", "Coloca codos altos, deja estirar el triceps y extiende sin mover hombros.", "Abrir demasiado codos o arquear lumbar.", "Core"),
                Ejercicio("Press frances", "Triceps", "Barra Z", "Intermedio", "Extension de codo", "Desde banco, baja la barra hacia la frente con codos estables y extiende controlado.", "Mover hombros o cargar demasiado.", "Antebrazo"),
                Ejercicio("Fondos en banco", "Triceps", "Peso corporal", "Principiante", "Empuje", "Apoya manos en banco, baja recto y empuja manteniendo hombros controlados.", "Alejar cadera demasiado o bajar con molestia de hombro.", "Pecho"),

                Ejercicio("Plancha frontal", "Core", "Peso corporal", "Principiante", "Anti extension", "Apoya antebrazos y puntas, aprieta abdomen y gluteos. Mantén linea recta sin hundir cadera.", "Elevar demasiado cadera, perder abdomen, aguantar la respiracion.", "Gluteos", "Hombro"),
                Ejercicio("Crunch en polea", "Core", "Polea", "Principiante", "Flexion de tronco", "Arrodillado frente a la polea, flexiona la columna llevando costillas hacia pelvis.", "Jalar con brazos o sentarse hacia atras.", "Oblicuos"),
                Ejercicio("Elevacion de piernas", "Core", "Peso corporal", "Intermedio", "Flexion de cadera", "Cuelga o apoya antebrazos, eleva piernas con pelvis controlada y baja lento.", "Balancearse o arquear lumbar.", "Flexores de cadera"),
                Ejercicio("Pallof press", "Core", "Polea", "Principiante", "Anti rotacion", "De lado a la polea, empuja el agarre al frente resistiendo que el torso rote.", "Girar cadera o perder postura.", "Oblicuos"),

                Ejercicio("Elevacion de pantorrilla de pie", "Pantorrilla", "Maquina", "Principiante", "Flexion plantar", "Sube sobre la punta de los pies, pausa arriba y baja hasta sentir estiramiento.", "Rebotar o doblar demasiado rodillas."),
                Ejercicio("Elevacion de pantorrilla sentado", "Pantorrilla", "Maquina", "Principiante", "Flexion plantar", "Mantén rodillas flexionadas, sube talones y baja lento.", "Usar rebote o recorrido corto.")
            };
        }

        private static EjercicioCatalogo Ejercicio(
            string nombre,
            string grupo,
            string equipo,
            string nivel,
            string patron,
            string instrucciones,
            string errores,
            params string[] secundarios)
        {
            return new EjercicioCatalogo
            {
                Nombre = nombre,
                GrupoMuscularPrincipal = grupo,
                MusculosSecundarios = secundarios.ToList(),
                Equipo = equipo,
                Nivel = nivel,
                PatronMovimiento = patron,
                Instrucciones = instrucciones,
                ErroresComunes = errores
            };
        }

        private sealed class ExerciseDbResponse
        {
            [JsonPropertyName("data")]
            public List<ExerciseDbItem> Data { get; set; } = new();
        }

        private sealed class ExerciseDbItem
        {
            [JsonPropertyName("exerciseId")]
            public string ExerciseId { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("gifUrl")]
            public string GifUrl { get; set; } = string.Empty;

            [JsonPropertyName("targetMuscles")]
            public List<string> TargetMuscles { get; set; } = new();

            [JsonPropertyName("bodyParts")]
            public List<string> BodyParts { get; set; } = new();

            [JsonPropertyName("equipments")]
            public List<string> Equipments { get; set; } = new();

            [JsonPropertyName("secondaryMuscles")]
            public List<string> SecondaryMuscles { get; set; } = new();

            [JsonPropertyName("instructions")]
            public List<string> Instructions { get; set; } = new();
        }
    }
}
