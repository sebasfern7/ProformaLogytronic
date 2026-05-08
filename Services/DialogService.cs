using System.Windows;

namespace ProformaLogytronic.Services
{
    public interface IDialogService
    {
        void ShowInfo(string message);
        void ShowError(string message);
        bool Confirm(string message);
    }

    public class DialogService : IDialogService
    {
        public void ShowInfo(string message) => MessageBox.Show(message, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        public void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public bool Confirm(string message) => MessageBox.Show(message, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
