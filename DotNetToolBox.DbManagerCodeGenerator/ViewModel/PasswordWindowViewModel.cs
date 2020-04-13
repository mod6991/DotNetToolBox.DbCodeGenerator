using DotNetToolBox.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DotNetToolBox.DbManagerCodeGenerator.ViewModel
{
    public class PasswordWindowViewModel : ViewModelBase<Window>
    {
        private string _username;
        private string _password;
        private PasswordBox _passwordBox;

        #region Constructor

        public PasswordWindowViewModel(Window visualObj)
            : base(visualObj)
        {
            ValidateCommand = new RelayCommand((param) => Validate(), (param) => ReturnTrue());

            _passwordBox = (PasswordBox)VisualObject.FindName("Password");
        }

        #endregion

        #region Properties

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        #endregion

        #region Commands

        public ICommand ValidateCommand { get; }

        #endregion

        #region Methods

        private bool ReturnTrue()
        {
            return true;
        }

        private void Validate()
        {
            try
            {
                _password = _passwordBox.Password;
                VisualObject.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
