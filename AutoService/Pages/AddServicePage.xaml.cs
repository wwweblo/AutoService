using AutoService.Data;
using System;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;

namespace AutoService.Pages
{
    public partial class AddServicePage : Page
    {
        private readonly Connect _context;

        public AddServicePage(Connect context)
        {
            InitializeComponent();
            _context = context;
        }
        private void ClearFields()
        {
            TitleTextBox.Text = string.Empty;
            TitleTextBox.Text = string.Empty;
            DurationTextBox.Text = string.Empty;
            DiscountTextBox.Text = string.Empty;
       
        }

        private void BackToService_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ServicePage(_context));
        }
        private  void SaveService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||  
      !decimal.TryParse(CostTextBox.Text, out decimal cost) || cost < 0 ||
      !int.TryParse(DurationTextBox.Text, out int duration) || duration < 0 ||
      !double.TryParse(DiscountTextBox.Text, out double discount) || discount < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректные данные. Значения не могут быть отрицательными");
                return;
            }

            var newService = new Service
            {
                Title = TitleTextBox.Text,
                Cost = cost,
                DurationInSeconds = duration,
                Description = DescriptionTextBox.Text,
                Discount = discount / 100
            };

            try
            {
                _context.Service.Add(newService);
                 _context.SaveChanges();
                MessageBox.Show("Сервис успешно добавлен.");
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сервиса: {ex.Message}");
            }
        }
    }
}
