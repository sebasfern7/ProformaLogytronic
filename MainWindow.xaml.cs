using System.Windows;
using ProformaLogytronic.Repositories;
using ProformaLogytronic.Services;
using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Inyección de dependencias manual para el MVP
            var repository = new JsonProformaRepository();
            var settingsStore = new JsonApplicationSettingsStore();
            var pdfService = new PdfService(settingsStore);
            var dialogService = new DialogService(settingsStore);

            DataContext = new ProformaViewModel(repository, pdfService, dialogService);
        }
    }
}