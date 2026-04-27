using System;
using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic.Models
{
    public class Proforma : ViewModelBase
    {
        private DateTime _fecha;
        public DateTime Fecha
        {
            get => _fecha;
            set => SetProperty(ref _fecha, value);
        }

        private string _numeroProforma;
        public string NumeroProforma
        {
            get => _numeroProforma;
            set => SetProperty(ref _numeroProforma, value);
        }

        public Proforma()
        {
            // Inicializar la fecha al crear la proforma, como indica el requerimiento
            Fecha = DateTime.Now;
        }
    }
}
