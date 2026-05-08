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
            var pdfService = new PdfService();
            var dialogService = new DialogService();

            DataContext = new ProformaViewModel(repository, pdfService, dialogService);
        }
    }
}