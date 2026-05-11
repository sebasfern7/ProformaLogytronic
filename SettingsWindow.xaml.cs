using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using ProformaLogytronic.Models;
using ProformaLogytronic.Services;

namespace ProformaLogytronic
{
    public partial class SettingsWindow : Window
    {
        private readonly IApplicationSettingsStore _settingsStore;

        public SettingsWindow(IApplicationSettingsStore settingsStore)
        {
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
            InitializeComponent();
            Loaded += (_, _) => CargarDesdeAlmacen();
        }

        private void CargarDesdeAlmacen()
        {
            var s = _settingsStore.Load();
            PdfFolderPathTextBox.Text = string.IsNullOrWhiteSpace(s.PdfOutputDirectory)
                ? string.Empty
                : s.PdfOutputDirectory;
        }

        private void Examinar_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Elegí la carpeta donde guardar los PDF de proformas.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            var actual = PdfFolderPathTextBox.Text?.Trim();
            if (!string.IsNullOrEmpty(actual) && Directory.Exists(actual))
                dlg.SelectedPath = actual;
            else
                dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                PdfFolderPathTextBox.Text = dlg.SelectedPath;
        }

        private void AplicarCambios_Click(object sender, RoutedEventArgs e)
        {
            var raw = PdfFolderPathTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(raw))
            {
                _settingsStore.Save(new AppSettings { PdfOutputDirectory = null });
                Close();
                return;
            }

            if (!Path.IsPathRooted(raw))
            {
                System.Windows.MessageBox.Show(
                    "La ruta debe ser absoluta (por ejemplo C:\\MisDocumentos\\Proformas). Podés usar «Examinar…» para elegirla.",
                    "Ruta no válida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var full = Path.GetFullPath(raw);
                Directory.CreateDirectory(full);
                _settingsStore.Save(new AppSettings { PdfOutputDirectory = full });
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"No se pudo crear o acceder a la carpeta:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
