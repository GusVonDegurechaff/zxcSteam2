using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; // Для работы с OpenFileDialog

namespace zxcSteam2.Pages
{
    public partial class StorePage : Page
    {
        private DBforISGameContext dbContext;
        private readonly User currentUser;

        public StorePage(DBforISGameContext context, User user)
        {
            InitializeComponent();
            dbContext = context;
            currentUser = user;
            LoadGames();
        }

        private void LoadGames()
        {
            var games = dbContext.Games.ToList();

            foreach (var game in games)
            {
                if (!string.IsNullOrEmpty(game.CoverImage))
                {
                    // Формируем полный путь к файлу
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), game.CoverImage);

                    // Проверяем существует ли файл
                    if (File.Exists(imagePath))
                    {
                        // Если файл существует, используем его
                        game.CoverImage = game.CoverImage;  // Уже содержит относительный путь
                    }
                    else
                    {
                        // Если файл не найден, используем изображение-заглушку
                        game.CoverImage = "GameProfImg\\m1000x1000.png";
                    }
                }
                else
                {
                    // Если в базе данных нет пути, используем изображение-заглушку
                    game.CoverImage = "GameProfImg\\m1000x1000.png";
                }
            }

            // Обновляем источник данных для ListBox
            GamesList.ItemsSource = games;
        }


        // Обработчик выбора игры
        private void GamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesList.SelectedItem != null)
            {
                var selectedGame = (Game)GamesList.SelectedItem;
                // Логика для обработки выбора игры
            }
        }

        // Обработчик добавления изображения игры
        private void AddGameImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string sourcePath = openFileDialog.FileName;

                // Папка хранения изображений
                string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GameProfImg");

                // Создаем папку, если она не существует
                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                }

                // Генерируем уникальное имя файла (или сохраняем оригинальное)
                string fileName = Path.GetFileName(sourcePath);
                string targetPath = Path.Combine(imagesDirectory, fileName);

                try
                {
                    // Копируем файл в папку GameProfImg
                    File.Copy(sourcePath, targetPath, overwrite: true);

                    // Сохраняем относительный путь в базе данных
                    var button = sender as Button;
                    if (button?.Tag is int gameId)
                    {
                        var game = dbContext.Games.FirstOrDefault(g => g.GameId == gameId);
                        if (game != null)
                        {
                            game.CoverImage = $"GameProfImg\\{fileName}"; // Относительный путь
                            dbContext.SaveChanges();

                            MessageBox.Show("Изображение успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadGames(); // Обновляем список игр
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик добавления игры на аккаунт
        private void AddGameToAccount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var userId = currentUser.UserId;
            if (button != null)
            {
                int gameId = (int)button.Tag; // Получаем GameId из Tag кнопки

                var game = dbContext.Games.FirstOrDefault(g => g.GameId == gameId);
                if (game != null)
                {
                    var existingUserGame = dbContext.UserGames
                        .FirstOrDefault(ug => ug.GameId == gameId && ug.UserId == currentUser.UserId);

                    if (existingUserGame != null)
                    {
                        // Если игра уже добавлена на аккаунт
                        MessageBox.Show("Эта игра уже добавлена на ваш аккаунт.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Добавляем игру на аккаунт
                        var userGame = new UserGame
                        {
                            GameId = gameId,
                            UserId = userId
                        };

                        dbContext.UserGames.Add(userGame);
                        dbContext.SaveChanges();

                        MessageBox.Show("Игра добавлена на ваш аккаунт!");
                    }
                }
            }
        }

        private string GetGameImageFolderPath()
        {
            // Путь к папке внутри проекта
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imageFolderPath = System.IO.Path.Combine(projectDirectory, "GameProfImg");

            // Создаем папку, если она еще не существует
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            return imageFolderPath;
        }

    }
}



