namespace Gymio.Interfaces
{
    public interface IPlanillaService
    {
        public Task<DateTime?> ObtenerUltimaFechaPagoAsync(int entrenadorId);
        public Task<bool> RegistrarPagoAsync(int entrenadorId, int cajeroId, decimal monto, DateTime inicio, DateTime fin);

    }
}
