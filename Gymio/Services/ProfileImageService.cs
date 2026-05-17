using Gymio.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace Gymio.Services
{
    public class ProfileImageService : IProfileImageService
    {
        private const long MaxOriginalFileSize = 6 * 1024 * 1024;
        private const long MaxProcessedFileSize = 3 * 1024 * 1024;
        private const long MaxPreviewFileSize = 256 * 1024;
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private readonly IWebHostEnvironment _environment;

        public ProfileImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> CrearVistaPreviaAsync(IBrowserFile? archivo)
        {
            if (archivo == null)
            {
                return null;
            }

            ValidarArchivo(archivo);

            try
            {
                var vistaPrevia = await archivo.RequestImageFileAsync("image/jpeg", 160, 160);
                await using var stream = vistaPrevia.OpenReadStream(MaxPreviewFileSize);
                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                return $"data:image/jpeg;base64,{Convert.ToBase64String(memory.ToArray())}";
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GuardarImagenPerfilAsync(IBrowserFile? archivo, string carpeta, string? rutaActual = null)
        {
            if (archivo == null)
            {
                return rutaActual;
            }

            ValidarArchivo(archivo);

            var carpetaSegura = LimpiarSegmento(carpeta);
            var webRoot = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var destino = Path.Combine(webRoot, "uploads", "personas", carpetaSegura);
            Directory.CreateDirectory(destino);

            IBrowserFile imagenParaGuardar = archivo;
            var extension = ExtensionPara(archivo.ContentType);
            var limiteLectura = MaxOriginalFileSize;

            try
            {
                imagenParaGuardar = await archivo.RequestImageFileAsync("image/jpeg", 720, 720);
                extension = ".jpg";
                limiteLectura = MaxProcessedFileSize;
            }
            catch
            {
                imagenParaGuardar = archivo;
            }

            var nombreArchivo = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(destino, nombreArchivo);

            await using (var origen = imagenParaGuardar.OpenReadStream(limiteLectura))
            await using (var salida = File.Create(rutaFisica))
            {
                await origen.CopyToAsync(salida);
            }

            EliminarImagenAnterior(webRoot, rutaActual);

            return $"/uploads/personas/{carpetaSegura}/{nombreArchivo}";
        }

        private static void ValidarArchivo(IBrowserFile archivo)
        {
            if (!AllowedContentTypes.Contains(archivo.ContentType))
            {
                throw new InvalidOperationException("La foto debe ser JPG, PNG o WEBP.");
            }

            if (archivo.Size > MaxOriginalFileSize)
            {
                throw new InvalidOperationException("La foto no debe superar los 6 MB.");
            }
        }

        private static string ExtensionPara(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        private static string LimpiarSegmento(string segmento)
        {
            var caracteres = segmento
                .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
                .ToArray();

            return caracteres.Length == 0 ? "generales" : new string(caracteres).ToLowerInvariant();
        }

        private static void EliminarImagenAnterior(string webRoot, string? rutaActual)
        {
            if (string.IsNullOrWhiteSpace(rutaActual) || !rutaActual.StartsWith("/uploads/personas/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativa = rutaActual.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rutaFisica = Path.GetFullPath(Path.Combine(webRoot, relativa));
            var raizUploads = Path.GetFullPath(Path.Combine(webRoot, "uploads", "personas"));

            if (rutaFisica.StartsWith(raizUploads, StringComparison.OrdinalIgnoreCase) && File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }
        }
    }
}
