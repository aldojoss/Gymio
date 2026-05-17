using Microsoft.AspNetCore.Components.Forms;

namespace Gymio.Interfaces
{
    public interface IProfileImageService
    {
        Task<string?> CrearVistaPreviaAsync(IBrowserFile? archivo);
        Task<string?> GuardarImagenPerfilAsync(IBrowserFile? archivo, string carpeta, string? rutaActual = null);
    }
}
