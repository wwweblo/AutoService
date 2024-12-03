using AutoService.Data;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AutoService.Pages
{
    public partial class UpdateClientPage : Page
    {
        private Client _client;
        private Connect _context;
        private string _photoPath;

        public UpdateClientPage(Client client, Connect context)
        {
            InitializeComponent();
            _context = context;
            _client = client;

            if (_client != null)
            {
                LoadClientData();
            }
            else
            {
                IdPanel.Visibility = Visibility.Collapsed; // Скрываем ID для нового клиента
                _client = new Client { RegistrationDate = DateTime.Now };
            }
        }

        private void LoadClientData()
        {
            IdPanel.Visibility = Visibility.Visible;
            IdTextBox.Text = _client.ID.ToString();
            FirstNameTextBox.Text = _client.FirstName;
            LastNameTextBox.Text = _client.LastName;
            PatronymicTextBox.Text = _client.Patronymic;
            EmailTextBox.Text = _client.Email;
            PhoneTextBox.Text = _client.Phone;
            BirthdayDatePicker.SelectedDate = _client.Birthday;

            if (_client.GenderCode == "м")
                MaleRadioButton.IsChecked = true;
            else if (_client.GenderCode == "ж")
                FemaleRadioButton.IsChecked = true;

            // Загружаем изображение клиента
            LoadClientImage();

            // Загрузка тегов клиента
            TagsListBox.ItemsSource = _client.Tag.ToList(); // Предполагаем, что _client.Tags - это коллекция тегов клиента
        }

        private void LoadClientImage()
        {
            try
            {
                // Получаем путь к корневой папке приложения (где исполнимая сборка)
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Путь к папке "Клиенты" относительно корня проекта
                string imageFolderPath = System.IO.Path.Combine(baseDirectory, "Клиенты");

                // Путь к изображению клиента на основе сохранённого пути в базе данных
                string imagePath = System.IO.Path.Combine(imageFolderPath, _client.PhotoPath);

                // Проверяем существует ли изображение по этому пути
                if (!string.IsNullOrEmpty(_client.PhotoPath) && System.IO.File.Exists(imagePath))
                {
                    // Загружаем изображение клиента
                    ClientImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
                else
                {
                    // Если изображение не найдено, показываем изображение по умолчанию
                    string defaultImagePath = System.IO.Path.Combine(imageFolderPath, "default-image.png");
                    if (System.IO.File.Exists(defaultImagePath))
                    {
                        ClientImage.Source = new BitmapImage(new Uri(defaultImagePath, UriKind.Absolute));
                    }
                    else
                    {
                        ClientImage.Source = null; // Если даже изображение по умолчанию отсутствует
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ClientImage.Source = null;
            }
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show("Размер фотографии не должен превышать 2 МБ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _photoPath = openFileDialog.FileName;
                ClientImage.Source = new BitmapImage(new Uri(_photoPath, UriKind.Absolute));
            }
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            string tagTitle = NewTagTextBox.Text.Trim();
            string tagColor = NewTagColorTextBox.Text.Trim();

            if (string.IsNullOrEmpty(tagTitle))
            {
                MessageBox.Show("Введите название тега.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(tagColor))
            {
                MessageBox.Show("Введите цвет для тега.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingTag = _context.Tag.FirstOrDefault(t => t.Title == tagTitle);
            if (existingTag == null)
            {
                // Если тег не найден в базе данных, добавляем новый тег
                var newTag = new Tag { Title = tagTitle, Color = tagColor };

                // Добавляем новый тег в таблицу Tags
                _context.Tag.Add(newTag);
                _context.SaveChanges(); // Сохраняем в базу данных

                // Добавляем новый тег в список тегов клиента
                _client.Tag.Add(newTag);
            }
            else
            {
                // Если тег уже существует, добавляем его в список тегов клиента
                if (!_client.Tag.Contains(existingTag))
                {
                    _client.Tag.Add(existingTag);
                }
                else
                {
                    MessageBox.Show("Этот тег уже привязан к клиенту.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            // Обновляем список тегов
            TagsListBox.ItemsSource = _client.Tag.ToList();
            NewTagTextBox.Clear(); // Очищаем поле ввода названия
            NewTagColorTextBox.Clear(); // Очищаем поле ввода цвета
        }


        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.Tag as Tag;

            if (tag != null)
            {
                // Удаляем тег из коллекции клиента
                _client.Tag.Remove(tag);

                // Обновляем список тегов
                TagsListBox.ItemsSource = _client.Tag.ToList();
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            _client.FirstName = FirstNameTextBox.Text.Trim();
            _client.LastName = LastNameTextBox.Text.Trim();
            _client.Patronymic = PatronymicTextBox.Text.Trim();
            _client.Email = EmailTextBox.Text.Trim();
            _client.Phone = PhoneTextBox.Text.Trim();
            _client.Birthday = BirthdayDatePicker.SelectedDate;
            _client.GenderCode = MaleRadioButton.IsChecked == true ? "м" : "ж";

            if (!string.IsNullOrEmpty(_photoPath))
            {
                _client.PhotoPath = _photoPath;
            }

            // Добавляем клиента, если это новый
            if (_client.ID == 0)
            {
                _context.Client.Add(_client);
            }

            _context.SaveChanges();
            MessageBox.Show("Клиент успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private bool ValidateForm()
        {
            // Проверка ФИО
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                FirstNameTextBox.Text.Length > 50 ||
                LastNameTextBox.Text.Length > 50 ||
                !Regex.IsMatch(FirstNameTextBox.Text, @"^[а-яА-ЯёЁa-zA-Z\s\-]+$") ||
                !Regex.IsMatch(LastNameTextBox.Text, @"^[а-яА-ЯёЁa-zA-Z\s\-]+$"))
            {
                MessageBox.Show("ФИО должно содержать только буквы, пробелы и дефисы и не превышать 50 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка email
            if (!Regex.IsMatch(EmailTextBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный Email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(PhoneTextBox.Text, @"^[\d\s\-\(\)\+]+$"))
            {
                MessageBox.Show("Телефон может содержать только цифры, пробелы, круглые скобки, дефисы и плюс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
