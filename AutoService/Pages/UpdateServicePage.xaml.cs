using AutoService.Data; 
using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoService.Pages
{
    public partial class UpdateServicePage : Page
    {
        private Service _service;
        private Connect _context;

        public UpdateServicePage(Service service, Connect context)
        {
            InitializeComponent();
            _service = service;
            _context = context;

            TitleTextBox.Text = _service.Title;
            CostTextBox.Text = _service.Cost.ToString();
            DurationTextBox.Text = _service.DurationInSeconds.ToString();
            DescriptionTextBox.Text = _service.Description;
            DiscountTextBox.Text = _service.Discount?.ToString() ?? string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _service.Title = TitleTextBox.Text;
            if (decimal.TryParse(CostTextBox.Text, out decimal cost))
                _service.Cost = cost;
            if (int.TryParse(DurationTextBox.Text, out int duration))
                _service.DurationInSeconds = duration;
            _service.Description = DescriptionTextBox.Text;
            if (double.TryParse(DiscountTextBox.Text, out double discount))
                _service.Discount = discount;

            try
            {
                _context.SaveChanges(); 
                MessageBox.Show("Service updated successfully.");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating service: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
