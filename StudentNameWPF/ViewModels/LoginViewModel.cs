using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;
using System.Windows;
using System.Windows.Input;

namespace StudentNameWPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public LoginViewModel()
        {
            _authService = new AuthenticationService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand LoginCommand { get; }

        private bool CanExecuteLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Get password from PasswordBox in the view
                var loginWindow = Application.Current.MainWindow as Views.LoginWindow;
                string password = "";
                if (loginWindow != null)
                {
                    var passwordBox = loginWindow.FindName("PasswordBox") as System.Windows.Controls.PasswordBox;
                    if (passwordBox != null)
                    {
                        password = passwordBox.Password;
                    }
                }
                
                var result = _authService.Authenticate(Email, password);
                
                if (result.IsSuccess && result.Customer != null)
                {
                    // Store current user
                    Application.Current.Properties["CurrentUser"] = result.Customer;
                    
                    // Navigate to appropriate window
                    if (result.Customer.IsAdmin)
                    {
                        var adminWindow = new Views.AdminMainWindow();
                        adminWindow.Show();
                    }
                    else
                    {
                        var customerWindow = new Views.CustomerMainWindow();
                        customerWindow.Show();
                    }
                    
                    // Close login window
                    Application.Current.MainWindow?.Close();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
