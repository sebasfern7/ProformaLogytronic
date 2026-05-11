using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using ProformaLogytronic.Models;
using ProformaLogytronic.Repositories;
using ProformaLogytronic.Services;

namespace ProformaLogytronic.ViewModels
{
    public class ProformaViewModel : ViewModelBase
    {
        private readonly IProformaRepository _repository;
        private readonly IPdfService _pdfService;
        private readonly IDialogService _dialogService;

        public class MesOpcion
        {
            public string Nombre { get; set; } = string.Empty;
            public int MesNumero { get; set; } // -1 = Todos
        }

        private string _clienteNombre = string.Empty;
        public string ClienteNombre
        {
            get => _clienteNombre;
            set => SetProperty(ref _clienteNombre, value);
        }

        private string _clienteDocumento = string.Empty;
        public string ClienteDocumento
        {
            get => _clienteDocumento;
            set => SetProperty(ref _clienteDocumento, value);
        }

        private string _clienteTelefono = string.Empty;
        public string ClienteTelefono
        {
            get => _clienteTelefono;
            set => SetProperty(ref _clienteTelefono, value);
        }

        private string _clienteCorreo = string.Empty;
        public string ClienteCorreo
        {
            get => _clienteCorreo;
            set => SetProperty(ref _clienteCorreo, value);
        }

        private string _clienteDireccion = string.Empty;
        public string ClienteDireccion
        {
            get => _clienteDireccion;
            set => SetProperty(ref _clienteDireccion, value);
        }

        private DateTime _fechaCreacion = DateTime.Now;
        public DateTime FechaCreacion
        {
            get => _fechaCreacion;
            set => SetProperty(ref _fechaCreacion, value);
        }

        private int _numeroSecuencia;
        public int NumeroSecuencia
        {
            get => _numeroSecuencia;
            set => SetProperty(ref _numeroSecuencia, value);
        }

        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set => SetProperty(ref _subtotal, value);
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public ObservableCollection<ProformaItem> Items { get; } = new();

        private ProformaItem? _selectedItem;
        public ProformaItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand AgregarItemCommand { get; }
        public ICommand EliminarItemCommand { get; }
        public ICommand LimpiarCommand { get; }
        public ICommand GenerarYGuardarCommand { get; }
        public ICommand AbrirConfiguracionCommand { get; }

        public ICommand BuscarHistorialCommand { get; }
        public ICommand LimpiarFiltrosHistorialCommand { get; }
        public ICommand AbrirProformaHistorialCommand { get; }
        public ICommand AbrirPdfHistorialCommand { get; }

        private int _mainSelectedTabIndex;
        public int MainSelectedTabIndex
        {
            get => _mainSelectedTabIndex;
            set
            {
                if (SetProperty(ref _mainSelectedTabIndex, value))
                {
                    OnPropertyChanged(nameof(IsProformaTabSelected));
                }
            }
        }

        public bool IsProformaTabSelected => MainSelectedTabIndex == 0;

        public System.Collections.ObjectModel.ObservableCollection<MesOpcion> MesesHistorial { get; } = new()
        {
            new MesOpcion { Nombre = "Todos", MesNumero = -1 },
            new MesOpcion { Nombre = "Enero", MesNumero = 1 },
            new MesOpcion { Nombre = "Febrero", MesNumero = 2 },
            new MesOpcion { Nombre = "Marzo", MesNumero = 3 },
            new MesOpcion { Nombre = "Abril", MesNumero = 4 },
            new MesOpcion { Nombre = "Mayo", MesNumero = 5 },
            new MesOpcion { Nombre = "Junio", MesNumero = 6 },
            new MesOpcion { Nombre = "Julio", MesNumero = 7 },
            new MesOpcion { Nombre = "Agosto", MesNumero = 8 },
            new MesOpcion { Nombre = "Septiembre", MesNumero = 9 },
            new MesOpcion { Nombre = "Octubre", MesNumero = 10 },
            new MesOpcion { Nombre = "Noviembre", MesNumero = 11 },
            new MesOpcion { Nombre = "Diciembre", MesNumero = 12 }
        };

        private MesOpcion? _mesSeleccionadoHistorial;
        public MesOpcion? MesSeleccionadoHistorial
        {
            get => _mesSeleccionadoHistorial;
            set
            {
                if (SetProperty(ref _mesSeleccionadoHistorial, value))
                {
                    // No filtrar en cada cambio de texto/mes automáticamente; se filtra con Buscar
                }
            }
        }

        private string _filtroClienteHistorial = string.Empty;
        public string FiltroClienteHistorial
        {
            get => _filtroClienteHistorial;
            set => SetProperty(ref _filtroClienteHistorial, value);
        }

        private string _filtroCodigoHistorial = string.Empty;
        public string FiltroCodigoHistorial
        {
            get => _filtroCodigoHistorial;
            set => SetProperty(ref _filtroCodigoHistorial, value);
        }

        private string _filtroDescripcionHistorial = string.Empty;
        public string FiltroDescripcionHistorial
        {
            get => _filtroDescripcionHistorial;
            set => SetProperty(ref _filtroDescripcionHistorial, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<Proforma> HistorialFiltrado { get; } = new();

        private Proforma? _selectedHistoryProforma;
        public Proforma? SelectedHistoryProforma
        {
            get => _selectedHistoryProforma;
            set => SetProperty(ref _selectedHistoryProforma, value);
        }

        private System.Collections.Generic.List<Proforma> _historialOriginal = new();

        private static bool ContainsIgnoreCase(string? source, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;
            return source.IndexOf(value.Trim(), System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public ProformaViewModel(IProformaRepository repository, IPdfService pdfService, IDialogService dialogService)
        {
            _repository = repository;
            _pdfService = pdfService;
            _dialogService = dialogService;

            AgregarItemCommand = new RelayCommand(_ => AgregarItem());
            EliminarItemCommand = new RelayCommand(_ => EliminarItem(), _ => SelectedItem != null);
            LimpiarCommand = new RelayCommand(_ => Limpiar());
            GenerarYGuardarCommand = new RelayCommand(_ => GenerarYGuardar(), _ => CanGenerar());
            AbrirConfiguracionCommand = new RelayCommand(_ => AbrirConfiguracion());

            BuscarHistorialCommand = new RelayCommand(_ => BuscarHistorial());
            LimpiarFiltrosHistorialCommand = new RelayCommand(_ => LimpiarFiltrosHistorial());
            AbrirProformaHistorialCommand = new RelayCommand(_ => AbrirProformaHistorial(), _ => SelectedHistoryProforma != null);
            AbrirPdfHistorialCommand = new RelayCommand(_ => AbrirPdfHistorial(), _ => SelectedHistoryProforma != null);

            Items.CollectionChanged += Items_CollectionChanged;
            
            // Pestaña principal por defecto
            MainSelectedTabIndex = 0;
            MesSeleccionadoHistorial = MesesHistorial.FirstOrDefault(m => m.MesNumero == -1);

            // Cargar historial desde el JSON local
            CargarHistorial();

            Limpiar(); // Inicializar estado
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProformaItem item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (ProformaItem item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }

            CalcularTotales();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProformaItem.Cantidad) || e.PropertyName == nameof(ProformaItem.PrecioUnitario))
            {
                CalcularTotales();
                // Notificar cambio en Subtotal del item para que la UI actualice la columna Total
                OnPropertyChanged(nameof(Items)); 
            }
        }

        private void CalcularTotales()
        {
            Subtotal = Items.Sum(i => i.Subtotal);
            Total = Subtotal; // En Fase 1 no hay impuestos/descuentos adicionales complejos
        }

        private void AgregarItem()
        {
            Items.Add(new ProformaItem { Cantidad = 1, PrecioUnitario = 0 });
        }

        private void EliminarItem()
        {
            if (SelectedItem != null)
            {
                Items.Remove(SelectedItem);
            }
        }

        private void Limpiar()
        {
            ClienteNombre = string.Empty;
            ClienteDocumento = string.Empty;
            ClienteTelefono = string.Empty;
            ClienteCorreo = string.Empty;
            ClienteDireccion = string.Empty;
            FechaCreacion = DateTime.Now;
            NumeroSecuencia = _repository.GenerarSiguienteNumero();
            Items.Clear();
            AgregarItem(); // Siempre empezar con una fila
        }

        private void AbrirConfiguracion()
        {
            _dialogService.ShowSettings();
        }

        private void CargarHistorial()
        {
            _historialOriginal = _repository.ObtenerTodas()
                .OrderByDescending(p => p.FechaCreacion)
                .ToList();

            FiltrarHistorial();
        }

        private void BuscarHistorial()
        {
            FiltrarHistorial();
        }

        private void LimpiarFiltrosHistorial()
        {
            MesSeleccionadoHistorial = MesesHistorial.FirstOrDefault(m => m.MesNumero == -1);
            FiltroClienteHistorial = string.Empty;
            FiltroCodigoHistorial = string.Empty;
            FiltroDescripcionHistorial = string.Empty;

            FiltrarHistorial();
        }

        private void FiltrarHistorial()
        {
            IEnumerable<Proforma> query = _historialOriginal;

            if (MesSeleccionadoHistorial != null && MesSeleccionadoHistorial.MesNumero != -1)
            {
                query = query.Where(p => p.FechaCreacion.Month == MesSeleccionadoHistorial.MesNumero);
            }

            if (!string.IsNullOrWhiteSpace(FiltroClienteHistorial))
            {
                query = query.Where(p => ContainsIgnoreCase(p.ClienteNombre, FiltroClienteHistorial));
            }

            if (!string.IsNullOrWhiteSpace(FiltroCodigoHistorial))
            {
                query = query.Where(p => p.Items.Any(i => ContainsIgnoreCase(i.CodigoProducto, FiltroCodigoHistorial)));
            }

            if (!string.IsNullOrWhiteSpace(FiltroDescripcionHistorial))
            {
                query = query.Where(p => p.Items.Any(i => ContainsIgnoreCase(i.Descripcion, FiltroDescripcionHistorial)));
            }

            var result = query.OrderByDescending(p => p.FechaCreacion).ToList();

            HistorialFiltrado.Clear();
            foreach (var p in result)
                HistorialFiltrado.Add(p);
        }

        private void AbrirProformaHistorial()
        {
            if (SelectedHistoryProforma == null)
                return;

            var p = SelectedHistoryProforma;

            // Cargar en el editor actual
            ClienteNombre = p.ClienteNombre;
            ClienteDocumento = p.ClienteDocumento;
            ClienteTelefono = p.ClienteTelefono;
            ClienteCorreo = p.ClienteCorreo;
            ClienteDireccion = p.ClienteDireccion;
            FechaCreacion = p.FechaCreacion;
            NumeroSecuencia = p.NumeroSecuencia;

            Items.Clear();
            foreach (var item in p.Items)
            {
                Items.Add(new ProformaItem
                {
                    CodigoProducto = item.CodigoProducto,
                    Descripcion = item.Descripcion,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }

            // Volver a la pestaña de proforma
            MainSelectedTabIndex = 0;
        }

        private void AbrirPdfHistorial()
        {
            if (SelectedHistoryProforma == null)
                return;

            var path = SelectedHistoryProforma.RutaPdf;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                _dialogService.ShowError("No se encontró el PDF para la proforma seleccionada (RutaPdf vacía o archivo inexistente).");
                return;
            }

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        private bool CanGenerar()
        {
            return !string.IsNullOrWhiteSpace(ClienteNombre) &&
                   Items.Count > 0 &&
                   Items.All(i => !string.IsNullOrWhiteSpace(i.Descripcion) && i.Cantidad > 0 && i.PrecioUnitario > 0);
        }

        private void GenerarYGuardar()
        {
            try
            {
                var proforma = new Proforma
                {
                    NumeroSecuencia = this.NumeroSecuencia,
                    FechaCreacion = this.FechaCreacion,
                    ClienteNombre = this.ClienteNombre,
                    ClienteDocumento = this.ClienteDocumento,
                    ClienteTelefono = this.ClienteTelefono,
                    ClienteCorreo = this.ClienteCorreo,
                    ClienteDireccion = this.ClienteDireccion,
                    Subtotal = this.Subtotal,
                    Total = this.Total,
                    Items = this.Items.ToList()
                };

                // 1. Guardar en JSON (Base de datos temporal)
                _repository.Guardar(proforma);

                // 2. Generar PDF
                string pdfPath = _pdfService.Generar(proforma);

                // 3. Actualizar ruta en JSON
                proforma.RutaPdf = pdfPath;
                _repository.Guardar(proforma);

                // 4. Abrir automáticamente
                Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });

                _dialogService.ShowInfo("¡Proforma generada y guardada exitosamente!");
                
                Limpiar();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error crítico: {ex.Message}");
            }
        }
    }
}
