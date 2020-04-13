using System;
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
            try
            {
                MainWindow window = new MainWindow();
                ViewModel.MainWindowViewModel vm = new ViewModel.MainWindowViewModel(window);
                window.DataContext = vm;
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
