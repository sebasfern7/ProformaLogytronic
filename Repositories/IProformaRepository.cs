using System.Collections.Generic;
using ProformaLogytronic.Models;

namespace ProformaLogytronic.Repositories
{
    public interface IProformaRepository
    {
        IEnumerable<Proforma> ObtenerTodas();
        void Guardar(Proforma proforma);
        int GenerarSiguienteNumero();
    }
}
