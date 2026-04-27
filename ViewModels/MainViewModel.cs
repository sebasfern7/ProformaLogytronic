using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ProformaLogytronic.Models;
using ProformaLogytronic.Services;

namespace ProformaLogytronic.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public Cliente Cliente { get; } = new Cliente();
        public Proforma Proforma { get; } = new Proforma();

        public ObservableCollection<Producto> Productos { get; } = new ObservableCollection<Producto>();

        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            private set
            {
                if (SetProperty(ref _subtotal, value))
                {
                    OnPropertyChanged(nameof(TotalFinal));
                }
            }
        }

        public decimal TotalFinal => Subtotal;

        public ICommand AgregarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand LimpiarFormularioCommand { get; }
        public ICommand GenerarPdfCommand { get; }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        public MainViewModel()
        {
            Productos.CollectionChanged += Productos_CollectionChanged;

            AgregarProductoCommand = new RelayCommand(AgregarProducto);
            EliminarProductoCommand = new RelayCommand(EliminarProducto, CanEliminarProducto);
            LimpiarFormularioCommand = new RelayCommand(LimpiarFormulario);
            GenerarPdfCommand = new RelayCommand(GenerarPdf, CanGenerarPdf);
            
            // Iniciar con una fila vacía
            AgregarProducto(null);
        }

        private void Productos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Producto item in e.NewItems)
                {
                    item.PropertyChanged -= Producto_PropertyChanged;
                    item.PropertyChanged += Producto_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Producto item in e.OldItems)
                {
                    item.PropertyChanged -= Producto_PropertyChanged;
                }
            }
            CalcularTotales();
        }

        private void Producto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Producto.Total))
            {
                CalcularTotales();
            }
        }

        private void CalcularTotales()
        {
            Subtotal = Productos.Sum(p => p.Total);
        }

        private void AgregarProducto(object parameter)
        {
            Productos.Add(new Producto());
        }

        private bool CanEliminarProducto(object parameter)
        {
            return ProductoSeleccionado != null;
        }

        private void EliminarProducto(object parameter)
        {
            if (ProductoSeleccionado != null)
            {
                Productos.Remove(ProductoSeleccionado);
            }
        }

        private void LimpiarFormulario(object parameter)
        {
            Cliente.Nombre = string.Empty;
            Cliente.NitCi = string.Empty;
            Cliente.Telefono = string.Empty;
            Cliente.Correo = string.Empty;
            Cliente.Direccion = string.Empty;

            Proforma.Fecha = DateTime.Now;
            Proforma.NumeroProforma = string.Empty;

            Productos.Clear();
            AgregarProducto(null);
        }

        private bool CanGenerarPdf(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Cliente.Nombre)) return false;
            if (Productos.Count == 0) return false;
            
            // Validar que al menos un producto tenga descripción y cantidades válidas
            if (Productos.Any(p => string.IsNullOrWhiteSpace(p.Descripcion) || p.Cantidad <= 0 || p.PrecioUnitario <= 0))
            {
                return false;
            }

            return true;
        }

        private void GenerarPdf(object parameter)
        {
            try
            {
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeName = string.Join("_", Cliente.Nombre.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"Proforma_{safeName}_{dateStr}.pdf".Replace(" ", "_");
                string fullPath = Path.Combine(docsPath, fileName);

                PdfGeneratorService.Generate(this, fullPath);

                DialogService.ShowSuccess($"¡Proforma generada exitosamente!\nSe ha guardado en:\n{fullPath}");
            }
            catch (Exception ex)
            {
                DialogService.ShowError($"Ocurrió un error al generar el PDF:\n{ex.Message}");
            }
        }
    }
}
