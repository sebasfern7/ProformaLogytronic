using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic.Models
{
    public class Cliente : ViewModelBase
    {
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _nitCi;
        public string NitCi
        {
            get => _nitCi;
            set => SetProperty(ref _nitCi, value);
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _correo;
        public string Correo
        {
            get => _correo;
            set => SetProperty(ref _correo, value);
        }

        private string _direccion;
        public string Direccion
        {
            get => _direccion;
            set => SetProperty(ref _direccion, value);
        }
    }
}
