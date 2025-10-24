using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using StudentNameWPF.ViewModels;
using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;

namespace StudentNameWPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
            
            // Add event handlers for real-time validation
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            
            // Add Enter key support
            EmailTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += PasswordBox_KeyDown;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Validate input
                var validationResult = ValidateLoginInput(email, password);
                if (!validationResult.IsValid)
                {
                    ShowError(validationResult.ErrorMessage, validationResult.ErrorType);
                    return;
                }

                // Show loading
                SetLoadingState(true);

                var result = _authService.Authenticate(email, password);
                
                if (result.IsSuccess && result.Customer != null)
                {
                    // Store current user
                    Application.Current.Properties["CurrentUser"] = result.Customer;
                    
                    // Navigate to appropriate window
                    if (result.Customer.IsAdmin)
                    {
                        var adminWindow = new AdminMainWindow();
                        adminWindow.Show();
                    }
                    else
                    {
                        var customerWindow = new CustomerMainWindow();
                        customerWindow.Show();
                    }
                    
                    // Close login window
                    this.Close();
                }
                else
                {
                    ShowError(result.Message, ErrorType.Error);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Đã xảy ra lỗi hệ thống: {ex.Message}", ErrorType.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void ShowError(string message, ErrorType errorType = ErrorType.Error)
        {
            ErrorMessageText.Text = message;
            
            // Set appropriate styling and icon based on error type
            switch (errorType)
            {
                case ErrorType.Error:
                    ErrorMessageBorder.Background = new SolidColorBrush(Color.FromRgb(254, 215, 215)); // Light red
                    ErrorMessageText.Foreground = new SolidColorBrush(Color.FromRgb(197, 48, 48)); // Dark red
                    ErrorIcon.Text = "❌";
                    break;
                case ErrorType.Warning:
                    ErrorMessageBorder.Background = new SolidColorBrush(Color.FromRgb(254, 235, 200)); // Light orange
                    ErrorMessageText.Foreground = new SolidColorBrush(Color.FromRgb(197, 78, 0)); // Dark orange
                    ErrorIcon.Text = "⚠️";
                    break;
                case ErrorType.Info:
                    ErrorMessageBorder.Background = new SolidColorBrush(Color.FromRgb(219, 234, 254)); // Light blue
                    ErrorMessageText.Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175)); // Dark blue
                    ErrorIcon.Text = "ℹ️";
                    break;
            }

            // Show with animation
            ErrorMessageBorder.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            ErrorMessageBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            
            // Highlight problematic fields
            HighlightErrorFields(message);
        }

        private void HighlightErrorFields(string errorMessage)
        {
            // Reset all borders first
            EmailBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
            PasswordBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
            
            // Highlight based on error type
            if (errorMessage.Contains("email") || errorMessage.Contains("Email"))
            {
                EmailBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(197, 48, 48));
            }
            if (errorMessage.Contains("mật khẩu") || errorMessage.Contains("password") || errorMessage.Contains("Password"))
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(197, 48, 48));
            }
        }

        private void HideError()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => ErrorMessageBorder.Visibility = Visibility.Collapsed;
            ErrorMessageBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private ValidationResult ValidateLoginInput(string email, string password)
        {
            // Check for empty fields
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Vui lòng nhập địa chỉ email.",
                    ErrorType = ErrorType.Warning
                };
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Vui lòng nhập mật khẩu.",
                    ErrorType = ErrorType.Warning
                };
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Địa chỉ email không hợp lệ. Vui lòng kiểm tra lại.",
                    ErrorType = ErrorType.Warning
                };
            }

            // Validate password length
            if (password.Length < 6)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.",
                    ErrorType = ErrorType.Warning
                };
            }

            return new ValidationResult { IsValid = true };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }


        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            LoginButton.Content = isLoading ? "Đang đăng nhập..." : "Đăng nhập";
            LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            
            if (isLoading)
            {
                HideError();
            }
        }

        // Event handlers for real-time validation
        private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ErrorMessageBorder.Visibility == Visibility.Visible)
            {
                HideError();
            }
            
            // Reset border color
            EmailBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorMessageBorder.Visibility == Visibility.Visible)
            {
                HideError();
            }
            
            // Reset border color
            PasswordBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
        }

        // Enter key support
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }
    }

    // Helper classes for validation and error handling
    public enum ErrorType
    {
        Error,
        Warning,
        Info
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ErrorType ErrorType { get; set; } = ErrorType.Error;
    }
}
