using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ProformaLogytronic.Models
{
    public class ProformaItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; set; }
        public int ProformaId { get; set; }

        private string _codigoProducto = string.Empty;
        public string CodigoProducto
        {
            get => _codigoProducto;
            set => SetProperty(ref _codigoProducto, value);
        }

        private string _descripcion = string.Empty;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set 
            {
                if (SetProperty(ref _cantidad, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        private decimal _precioUnitario;
        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set 
            {
                if (SetProperty(ref _precioUnitario, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        [JsonIgnore] 
        public decimal Subtotal => Cantidad * PrecioUnitario;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
