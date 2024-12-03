using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoService.Data;

namespace AutoService.Pages
{
    public partial class ClientPage : Page
    {
        private readonly Connect _context = new Connect();
        private Client _selectedClient;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _searchText = string.Empty;
        private string _genderFilter = "All";
        private bool _showBirthdays = false;
        private string _sortBy = "ID";

        public ClientPage()
        {
            InitializeComponent();
            Loaded += ClientPage_Loaded;
        }

        private void ClientPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        private void LoadClients()
        {
            var clientsQuery = _context.Client.AsQueryable();

            if (_genderFilter != "All")
            {
                clientsQuery = clientsQuery.Where(c => c.GenderCode == _genderFilter);
            }

            if (_showBirthdays)
            {
                var currentMonth = DateTime.Now.Month;
                clientsQuery = clientsQuery.Where(c => c.Birthday.HasValue && c.Birthday.Value.Month == currentMonth);
            }

            if (!string.IsNullOrEmpty(_searchText))
            {
                clientsQuery = clientsQuery.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(_searchText)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(_searchText)) ||
                    (c.Email != null && c.Email.ToLower().Contains(_searchText)) ||
                    (c.Phone != null && c.Phone.Contains(_searchText))
                );
            }

            var projectedQuery = clientsQuery.Select(c => new
            {
                c.ID,
                c.GenderCode,
                c.FirstName,
                c.LastName,
                c.Patronymic,
                c.Birthday,
                c.Phone,
                c.Email,
                c.RegistrationDate,
                LastVisit = c.ClientService.Max(cs => (DateTime?)cs.StartTime),
                VisitCount = c.ClientService.Count,
            });

            switch (_sortBy)
            {
                case "LastName":
                    projectedQuery = projectedQuery.OrderBy(c => c.LastName);
                    break;
                case "LastVisit":
                    projectedQuery = projectedQuery.OrderByDescending(c => c.LastVisit);
                    break;
                case "VisitCount":
                    projectedQuery = projectedQuery.OrderByDescending(c => c.VisitCount);
                    break;
                default:
                    projectedQuery = projectedQuery.OrderBy(c => c.ID);
                    break;
            }


            int totalClients = projectedQuery.Count();
            int totalPages = _pageSize == 0 ? 1 : (int)Math.Ceiling((double)totalClients / _pageSize);

            var clients = _pageSize == 0
                ? projectedQuery.ToList()
                : projectedQuery.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            if (ClientDataGrid != null)
            {
                ClientDataGrid.ItemsSource = clients;
                PageNumberTextBlock.Text = $"Страница {_currentPage} из {totalPages}";
            }
        }

        private void SortByLastName_Click(object sender, RoutedEventArgs e)
        {
            _sortBy = "LastName";
            LoadClients();
        }

        private void SortByLastVisit_Click(object sender, RoutedEventArgs e)
        {
            _sortBy = "LastVisit";
            LoadClients();
        }

        private void SortByVisitCount_Click(object sender, RoutedEventArgs e)
        {
            _sortBy = "VisitCount";
            LoadClients();
        }

        private void GenderFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenderFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _genderFilter = selectedItem.Tag.ToString();
                LoadClients();
            }
        }

        private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _pageSize = selectedItem.Tag.ToString() == "All" ? 0 : int.Parse(selectedItem.Tag.ToString());
                _currentPage = 1;
                LoadClients();
            }
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortByComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _sortBy = selectedItem.Tag.ToString();
                LoadClients(); // Перезагрузить данные с новой сортировкой
            }
        }


        private void ShowBirthdaysCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _showBirthdays = ShowBirthdaysCheckBox.IsChecked == true;
            LoadClients();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text.ToLower();
            _currentPage = 1;
            LoadClients();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadClients();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalClients = _context.Client.Count();
            int totalPages = (int)Math.Ceiling((double)totalClients / _pageSize);

            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadClients();
            }
        }

        private void AddNewClient_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddClientPage(_context));
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient != null)
            {
                try
                {
                    var clientToDelete = _context.Client.FirstOrDefault(c => c.ID == _selectedClient.ID);
                    if (clientToDelete != null)
                    {
                        if (_context.ClientService.Any(cs => cs.ClientID == clientToDelete.ID))
                        {
                            MessageBox.Show("Нельзя удалить клиента с посещениями.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _context.Client.Remove(clientToDelete);
                        _context.SaveChanges();

                        MessageBox.Show("Клиент успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        _selectedClient = null;
                        LoadClients();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateClient_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient != null)
            {
                NavigationService.Navigate(new UpdateClientPage(_selectedClient, _context));
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewVisits_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient != null)
            {
                var visitListWindow = new VisitListWindow(_selectedClient);
                visitListWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите клиента для просмотра посещений.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientDataGrid.SelectedItem != null)
            {
                var selectedRow = ClientDataGrid.SelectedItem;

                var idProperty = selectedRow.GetType().GetProperty("ID");
                if (idProperty != null)
                {
                    int selectedId = (int)idProperty.GetValue(selectedRow);
                    _selectedClient = _context.Client.FirstOrDefault(c => c.ID == selectedId);
                }
            }
            else
            {
                _selectedClient = null;
            }
        }
    }
}
