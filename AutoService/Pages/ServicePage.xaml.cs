using AutoService.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AutoService.Pages
{
    public partial class ServicePage : Page
    {
        private Connect _context;
        private Service _selectedService;
        public ObservableCollection<Service> Services { get; set; }
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _searchText = string.Empty;
        private string _sortBy = "ID";

        public ServicePage(Connect context)
        {
            InitializeComponent();
            _context = context;
            Services = new ObservableCollection<Service>();

            LoadServices();
        }

        private void LoadServices()
        {
            var servicesQuery = _context.Service.AsQueryable();

            if (!string.IsNullOrEmpty(_searchText))
            {
                servicesQuery = servicesQuery.Where(s =>
                    (s.Title != null && s.Title.ToLower().Contains(_searchText)) ||
                    (s.Description != null && s.Description.ToLower().Contains(_searchText))
                );
            }

            switch (_sortBy)
            {
                case "Title":
                    servicesQuery = servicesQuery.OrderBy(s => s.Title);
                    break;
                case "Cost":
                    servicesQuery = servicesQuery.OrderBy(s => s.Cost);
                    break;
                case "Duration":
                    servicesQuery = servicesQuery.OrderBy(s => s.DurationInSeconds);
                    break;
                case "Discount":
                    servicesQuery = servicesQuery.OrderBy(s => s.Discount);
                    break;
                case "ID":
                    servicesQuery = servicesQuery.OrderBy(s => s.ID);
                    break;
            }

            int totalServices = servicesQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalServices / _pageSize);

            var services = servicesQuery
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            ServiceDataGrid.ItemsSource = services;
            PageNumberTextBlock.Text = $"Page {_currentPage} of {totalPages}";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text.ToLower();
            _currentPage = 1;
            LoadServices();
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            var addServicePage = new AddServicePage(_context);
            NavigationService.Navigate(addServicePage);
        }

        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedService != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить сервис {_selectedService.Title}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Service.Remove(_selectedService);
                        _context.SaveChanges();

                        LoadServices();

                        MessageBox.Show("Сервис успешно удален.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении сервиса: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateService_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedService != null)
            {
                var updateServicePage = new UpdateServicePage(_selectedService, _context);
                NavigationService.Navigate(updateServicePage);
            }
            else
            {
                MessageBox.Show("Выберите сервис для обновления.");
            }
        }

        private void BackToClients_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClientPage());
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _sortBy = selectedItem.Tag.ToString();
                LoadServices();
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadServices();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            LoadServices();
        }

        private void ServiceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedService = (Service)ServiceDataGrid.SelectedItem;
        }
    }
}
