using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

        public ProformaViewModel(IProformaRepository repository, IPdfService pdfService, IDialogService dialogService)
        {
            _repository = repository;
            _pdfService = pdfService;
            _dialogService = dialogService;

            AgregarItemCommand = new RelayCommand(_ => AgregarItem());
            EliminarItemCommand = new RelayCommand(_ => EliminarItem(), _ => SelectedItem != null);
            LimpiarCommand = new RelayCommand(_ => Limpiar());
            GenerarYGuardarCommand = new RelayCommand(_ => GenerarYGuardar(), _ => CanGenerar());

            Items.CollectionChanged += Items_CollectionChanged;
            
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
            FechaCreacion = DateTime.Now;
            NumeroSecuencia = _repository.GenerarSiguienteNumero();
            Items.Clear();
            AgregarItem(); // Siempre empezar con una fila
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
                    Subtotal = this.Subtotal,
                    Total = this.Total,
                    Items = this.Items.ToList()
                };

                // 1. Guardar en JSON (Base de datos temporal)
                _repository.Guardar(proforma);

                // 2. Generar PDF
                string baseDir = AppContext.BaseDirectory;
                string pdfPath = _pdfService.Generar(proforma, baseDir);

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
