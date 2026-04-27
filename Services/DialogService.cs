using System.Windows;

namespace ProformaLogytronic.Services
{
    public static class DialogService
    {
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
