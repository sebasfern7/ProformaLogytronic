using ProformaLogytronic;

namespace ProformaLogytronic.Services
{
    public interface IDialogService
    {
        void ShowInfo(string message);
        void ShowError(string message);
        bool Confirm(string message);
        void ShowSettings();
    }

    public class DialogService : IDialogService
    {
        private readonly IApplicationSettingsStore _settingsStore;

        public DialogService(IApplicationSettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public void ShowInfo(string message) =>
            System.Windows.MessageBox.Show(message, "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        public void ShowError(string message) =>
            System.Windows.MessageBox.Show(message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

        public bool Confirm(string message) =>
            System.Windows.MessageBox.Show(message, "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;

        public void ShowSettings()
        {
            var window = new SettingsWindow(_settingsStore);
            var app = System.Windows.Application.Current;
            if (app?.MainWindow != null)
                window.Owner = app.MainWindow;

            window.ShowDialog();
        }
    }
}
