using ProformaLogytronic.Models;

namespace ProformaLogytronic.Services
{
    public interface IApplicationSettingsStore
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}
