using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic.Models
{
    public class Producto : ViewModelBase
    {
        private string _codigo;
        public string Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private int _cantidad = 1;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (SetProperty(ref _cantidad, value))
                {
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        private decimal _precioUnitario = 0m;
        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                if (SetProperty(ref _precioUnitario, value))
                {
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Total => Cantidad * PrecioUnitario;
    }
}
