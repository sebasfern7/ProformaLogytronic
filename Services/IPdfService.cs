using ProformaLogytronic.Models;

namespace ProformaLogytronic.Services
{
    public interface IPdfService
    {
        string Generar(Proforma proforma, string directorioBase);
    }
}
