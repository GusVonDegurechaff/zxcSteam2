using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using zxcSteam2.Pages;

namespace zxcSteam2
{
    /// <summary>
    /// Логика взаимодействия для WindowProfile.xaml
    /// </summary>
    public partial class WindowProfile : Window
    {
        private DBforISGameContext dbContext;

        public WindowProfile(User user, DBforISGameContext context)
        {

            InitializeComponent();
            this.dbContext = context;
            this.DataContext = user; // Важно установить DataContext здесь, перед вызовом LoadProfilePicture
            LoadProfilePicture();

        }
        private async void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName; // Путь к выбранному файлу
                string relativePath = ""; // Относительный путь для сохранения в базе данных

                try
                {
                    // Папка для хранения изображений профиля
                    string profileImagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "ProfileImage");
                    Directory.CreateDirectory(profileImagePath); // Создаем папку, если её нет

                    // Формируем путь для копирования файла
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string destinationPath = System.IO.Path.Combine(profileImagePath, fileName);

                    // Копируем файл с перезаписью в случае существования
                    File.Copy(filePath, destinationPath, true);

                    // Сохраняем относительный путь к файлу
                    relativePath = System.IO.Path.Combine("ProfileImage", fileName);

                    // Обновляем путь к изображению в модели пользователя
                    ((User)this.DataContext).ProfilePicture = relativePath;
                    await dbContext.SaveChangesAsync();

                    // Обновляем изображение в UI
                    LoadProfilePicture();
                }
                catch (Exception ex)
                {
                    // Обработка ошибок
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadProfilePicture()
        {
            if (DataContext is User user && !string.IsNullOrEmpty(user.ProfilePicture))
            {
                string fullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), user.ProfilePicture);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        BitmapImage image = new BitmapImage(new Uri(fullPath));
                        ProfileImage.Source = image;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                    }
                }
                else
                {
                    // Обработка случая, когда изображение не найдено
                    // Например, установка изображения по умолчанию:
                    ProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/default_profile.png")); // Замените на ваш путь
                }
            }
        }
        private void Menu_Friends_Click(object sender, RoutedEventArgs e) => FrmMain.Navigate(new FriendsPage(dbContext, (User)DataContext));
        private void Menu_Store_Click(object sender, RoutedEventArgs e) => FrmMain.Navigate(new StorePage(dbContext, (User)DataContext));
        private void Menu_Library_Click(object sender, RoutedEventArgs e) => FrmMain.Navigate(new LibraryPage(dbContext, (User)DataContext));
        private void Menu_Stats_Click(object sender, RoutedEventArgs e) => FrmMain.Navigate(new StatsPage(dbContext, (User)DataContext));

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

    }
}

