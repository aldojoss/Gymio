using Gymio.Models;

namespace Gymio.Interfaces
{
    public interface ITurnoCajaService
    {
        Task<TurnoCaja> AbrirTurnoAsync(int usuarioId, decimal montoInicial);
        Task<TurnoCaja?> ObtenerTurnoActivoAsync(int usuarioId);
        Task<TurnoCaja> CerrarTurnoAsync(int turnoId, decimal montoFisicoContado);
    }
}