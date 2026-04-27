using System.Windows;
using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}