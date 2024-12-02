using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace zxcSteam2.Pages
{
    public partial class LibraryPage : Page
    {
        private readonly DBforISGameContext dbContext;
        private readonly User currentUser;

        public LibraryPage(DBforISGameContext context, User user)
        {
            InitializeComponent();
            dbContext = context ?? throw new ArgumentNullException(nameof(context), "dbContext не может быть null");
            currentUser = user ?? throw new ArgumentNullException(nameof(user), "User не может быть null");
            LoadLibrary();
        }

        private void LoadLibrary()
        {
            if (dbContext == null)
            {
                MessageBox.Show("Ошибка подключения к базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userGames = dbContext.UserGames
                .Where(ug => ug.UserId == currentUser.UserId)
                .Join(dbContext.Games, ug => ug.GameId, g => g.GameId, (ug, g) => g)
                .ToList();

            if (userGames == null || !userGames.Any())
            {
                MessageBox.Show("У вас нет игр в библиотеке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LibraryList.ItemsSource = userGames.Select(g => new GameViewModel(g, dbContext, currentUser)).ToList();
        }
    }

    public class GameViewModel
    {
        private readonly DBforISGameContext dbContext;
        private readonly Game game;
        private readonly User currentUser;
        private DateTime gameStartTime; // Время начала игры

        public GameViewModel(Game game, DBforISGameContext context, User user)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game), "Игра не может быть null");
            this.dbContext = context ?? throw new ArgumentNullException(nameof(context), "dbContext не может быть null");
            this.currentUser = user ?? throw new ArgumentNullException(nameof(user), "User не может быть null");

            GameId = game.GameId;
            GameName = game.GameName;
            CoverImage = game.CoverImage;
            InstallCommand = new RelayCommand(install => InstallGame());
            PlayCommand = new RelayCommand(play => LaunchGame());
        }

        public int GameId { get; set; }
        public string GameName { get; set; }
        public string CoverImage { get; set; }
        public ICommand InstallCommand { get; set; }
        public ICommand PlayCommand { get; set; }

        // Логика установки игры
        private void InstallGame()
        {
            MessageBox.Show("Перед установкой,помните что название папки и exe файла должно полностью соответствовать названию игры что указано в Библиотека,так же помните что папка с игрой должна находится по след.пути:C:\\Games\\MyGame\\");
            string gameInstallerFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameInstaller");

            // Проверка существования папки GameInstaller
            if (Directory.Exists(gameInstallerFolder))
            {
                // Открытие папки с инсталляторами или документами
                Process.Start(new ProcessStartInfo("explorer.exe", gameInstallerFolder));
            }
            else
            {
                MessageBox.Show("Папка GameInstaller не найдена. Убедитесь, что она существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Логика запуска игры
        private void LaunchGame()
        {
            string gameFolder = Path.Combine("C:\\Games\\MyGame\\", game.GameName);
            string gameExecutable = Path.Combine(gameFolder, $"{game.GameName}.exe");

            // Проверка на наличие исполнимого файла игры
            if (File.Exists(gameExecutable))
            {
                try
                {
                    // Проверяем, существует ли профиль для текущего пользователя и игры
                    var profile = dbContext.Profiles
                        .FirstOrDefault(p => p.UserId == currentUser.UserId && p.GameId == game.GameId);

                    if (profile == null)
                    {
                        // Если профиль не найден, создаем новый
                        profile = new Profile
                        {
                            GameId = game.GameId,
                            ProfileName = $"{game.GameName}",
                            UserId = currentUser.UserId
                        };

                        dbContext.Profiles.Add(profile);
                        dbContext.SaveChanges(); // Сохраняем профиль в базу

                        // После создания профиля, создаем статистику для этого профиля
                        var newStat = new Statistic
                        {
                            ProfileId = profile.ProfileId,
                            HoursInGame = "0", // Начальное значение
                            Rank = "Beginner", // Начальное значение
                            Lvl = "1" // Начальное значение
                        };

                        dbContext.Statistics.Add(newStat);
                        dbContext.SaveChanges(); // Сохраняем статистику в базу
                    }

                    // Начинаем отсчет времени
                    gameStartTime = DateTime.Now;

                    // Теперь можно запускать игру
                    Process.Start(gameExecutable);  // Запуск игры

                    // Отслеживаем завершение игры
                    var process = Process.GetProcessesByName(game.GameName).FirstOrDefault();
                    if (process != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += (sender, e) => OnGameExit(profile);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось запустить игру: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Не найден исполнимый файл игры! Пожалуйста, установите игру.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработка завершения игры
        private void OnGameExit(Profile profile)
        {
            var profileStat = dbContext.Statistics.FirstOrDefault(s => s.ProfileId == profile.ProfileId);

            if (profileStat != null)
            {
                // Вычисление прошедшего времени
                var elapsedHours = (DateTime.Now - gameStartTime).TotalHours;
                profileStat.HoursInGame = (double.Parse(profileStat.HoursInGame) + elapsedHours).ToString("0.0");

                // Повышаем уровень (пример: увеличиваем уровень за каждый запуск игры)
                profileStat.Lvl = (int.Parse(profileStat.Lvl) + 1).ToString();

                // Сохраняем обновленную статистику
                dbContext.SaveChanges();
            }
        }
    }
}







