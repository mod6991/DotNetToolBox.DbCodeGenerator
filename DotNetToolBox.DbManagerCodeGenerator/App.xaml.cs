using System.Windows;

namespace DotNetToolBox.DbManagerCodeGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            ViewModel.MainWindowViewModel vm = new ViewModel.MainWindowViewModel(window);
            window.DataContext = vm;
            window.Show();

            base.OnStartup(e);
        }
    }
}
