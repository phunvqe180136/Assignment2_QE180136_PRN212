using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StudentNameWPF.ViewModels
{
    public class CustomerManagementViewModel : BaseViewModel
    {
        private readonly CustomerService _customerService;
        private ObservableCollection<Customer> _customers;
        private Customer? _selectedCustomer;
        private string _searchTerm = string.Empty;
        private string _statusMessage = string.Empty;

        public CustomerManagementViewModel()
        {
            _customerService = new CustomerService();
            _customers = new ObservableCollection<Customer>();
            
            LoadCustomers();
            
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, () => SelectedCustomer != null);
            DeleteCommand = new RelayCommand(ExecuteDelete, () => SelectedCustomer != null);
            SearchCommand = new RelayCommand(ExecuteSearch);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand RefreshCommand { get; }

        private void LoadCustomers()
        {
            try
            {
                var customers = _customerService.GetAllCustomers();
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
                StatusMessage = $"Loaded {customers.Count} customers";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
        }

        private void ExecuteAdd()
        {
            try
            {
                var dialog = new Views.CustomerDialog();
                if (dialog.ShowDialog() == true)
                {
                    var customer = dialog.Customer;
                    var result = _customerService.AddCustomer(customer);
                    
                    if (result.IsSuccess)
                    {
                        Customers.Add(customer);
                        StatusMessage = result.Message;
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit()
        {
            if (SelectedCustomer == null) return;

            try
            {
                var dialog = new Views.CustomerDialog(SelectedCustomer);
                if (dialog.ShowDialog() == true)
                {
                    var updatedCustomer = dialog.Customer;
                    var result = _customerService.UpdateCustomer(updatedCustomer);
                    
                    if (result.IsSuccess)
                    {
                        var index = Customers.IndexOf(SelectedCustomer);
                        Customers[index] = updatedCustomer;
                        StatusMessage = result.Message;
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete customer '{SelectedCustomer.CustomerFullName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var deleteResult = _customerService.DeleteCustomer(SelectedCustomer.CustomerID);
                    
                    if (deleteResult.IsSuccess)
                    {
                        Customers.Remove(SelectedCustomer);
                        StatusMessage = deleteResult.Message;
                    }
                    else
                    {
                        MessageBox.Show(deleteResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteSearch()
        {
            try
            {
                var customers = _customerService.SearchCustomers(SearchTerm);
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
                StatusMessage = $"Found {customers.Count} customers";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching customers: {ex.Message}";
            }
        }

        private void ExecuteRefresh()
        {
            LoadCustomers();
        }
    }
}
