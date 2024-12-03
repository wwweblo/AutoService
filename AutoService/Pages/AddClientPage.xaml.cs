using AutoService.Data;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoService.Pages
{
    public partial class AddClientPage : Page
    {
        private readonly Connect _context;
        private string _photoPath; 

        public AddClientPage(Connect context)
        {
            InitializeComponent();
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null");
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string patronymic = PatronymicTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            DateTime? birthday = BirthdayPicker.SelectedDate;
            string genderCode = MaleRadioButton.IsChecked == true ? "м" : FemaleRadioButton.IsChecked == true ? "ж" : null;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                ShowStatus("Имя и фамилия обязательны.", Colors.Red);
                return;
            }

            if (!Regex.IsMatch(firstName + lastName + patronymic, @"^[А-Яа-яЁёA-Za-z\s-]+$"))
            {
                ShowStatus("Имя, фамилия и отчество могут содержать только буквы, пробелы и дефисы.", Colors.Red);
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowStatus("Неверный формат email.", Colors.Red);
                return;
            }

            if (!IsValidPhone(phone))
            {
                ShowStatus("Неверный формат телефона.", Colors.Red);
                return;
            }

            if (genderCode == null)
            {
                ShowStatus("Выберите пол клиента.", Colors.Red);
                return;
            }

            if (birthday == null)
            {
                ShowStatus("Укажите дату рождения.", Colors.Red);
                return;
            }

            if (_photoPath != null && new FileInfo(_photoPath).Length > 2 * 1024 * 1024)
            {
                ShowStatus("Размер фотографии не должен превышать 2 МБ.", Colors.Red);
                return;
            }

            var newClient = new Client
            {
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                Email = email,
                Phone = phone,
                Birthday = birthday,
                GenderCode = genderCode,
                RegistrationDate = DateTime.Now,
                PhotoPath = _photoPath
            };

            _context.Client.Add(newClient);
            _context.SaveChanges();

            ShowStatus("Клиент успешно добавлен!", Colors.Green);
            ClearFields();
        }

        private void ClearFields()
        {
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            PatronymicTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;
            MaleRadioButton.IsChecked = false;
            FemaleRadioButton.IsChecked = false;
            BirthdayPicker.SelectedDate = null;
            ClientImage.Visibility = Visibility.Collapsed;
            _photoPath = null;
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]+$");
        }

        private void ShowStatus(string message, Color color)
        {
            MessageBox.Show(message);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClientPage());
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _photoPath = openFileDialog.FileName;

                ClientImage.Source = new BitmapImage(new Uri(_photoPath, UriKind.Absolute));
                ClientImage.Visibility = Visibility.Visible;
            }
        }
    }
}
