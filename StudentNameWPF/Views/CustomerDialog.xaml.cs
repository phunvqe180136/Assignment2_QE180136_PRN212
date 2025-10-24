using System.Windows;
using FUMiniHotelSystem.DataAccess.Models;
using System.Windows.Controls;

namespace StudentNameWPF.Views
{
    public partial class CustomerDialog : Window
    {
        public Customer Customer { get; private set; }
        private bool _isEditMode;

        public CustomerDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            Customer = new Customer
            {
                CustomerStatus = 1
            };
            LoadData();
        }

        public CustomerDialog(Customer customer)
        {
            InitializeComponent();
            _isEditMode = true;
            Customer = customer;
            LoadData();
        }

        private void LoadData()
        {
            FullNameTextBox.Text = Customer.CustomerFullName;
            EmailTextBox.Text = Customer.EmailAddress;
            TelephoneTextBox.Text = Customer.Telephone;
            BirthdayDatePicker.SelectedDate = Customer.CustomerBirthday;
            StatusComboBox.SelectedIndex = Customer.CustomerStatus - 1;
            
            if (_isEditMode)
            {
                this.Title = "Edit Customer";
                PasswordBox.Visibility = Visibility.Collapsed;
                // Hide password label too
                var passwordLabel = this.FindName("PasswordLabel") as System.Windows.Controls.Label;
                if (passwordLabel != null)
                    passwordLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Customer.CustomerFullName = FullNameTextBox.Text.Trim();
                Customer.EmailAddress = EmailTextBox.Text.Trim();
                Customer.Telephone = TelephoneTextBox.Text.Trim();
                Customer.CustomerBirthday = BirthdayDatePicker.SelectedDate ?? DateTime.Today;
                Customer.CustomerStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Tag.ToString() == "1" ? 1 : 2;
                
                if (!_isEditMode)
                {
                    Customer.Password = PasswordBox.Password;
                }

                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            ErrorMessageText.Text = "";

            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                ShowError("Full name is required.");
                return false;
            }

            if (FullNameTextBox.Text.Length > 50)
            {
                ShowError("Full name cannot exceed 50 characters.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Email address is required.");
                return false;
            }

            if (EmailTextBox.Text.Length > 50)
            {
                ShowError("Email address cannot exceed 50 characters.");
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowError("Invalid email format.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelephoneTextBox.Text))
            {
                ShowError("Telephone is required.");
                return false;
            }

            if (TelephoneTextBox.Text.Length > 12)
            {
                ShowError("Telephone cannot exceed 12 characters.");
                return false;
            }

            if (BirthdayDatePicker.SelectedDate == null)
            {
                ShowError("Birthday is required.");
                return false;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Password is required.");
                return false;
            }

            if (!_isEditMode && PasswordBox.Password.Length > 50)
            {
                ShowError("Password cannot exceed 50 characters.");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
