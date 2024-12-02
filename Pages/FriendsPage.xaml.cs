using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace zxcSteam2.Pages
{
    /// <summary>
    /// Логика взаимодействия для FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        private readonly DBforISGameContext dbContext;
        private readonly User currentUser;

        public FriendsPage(DBforISGameContext context, User user)
        {
            InitializeComponent();
            dbContext = context;
            currentUser = user;
            LoadFriends();
        }

        private void LoadFriends()
        {
            // Загружаем всех пользователей в память
            var users = dbContext.Users.ToList();

            // Загрузка списка друзей с их ID
            var friends = dbContext.Friends
                .Where(f => f.UserId1 == currentUser.UserId || f.UserId2 == currentUser.UserId)
                .Select(f => new
                {
                    FriendId = f.UserId1 == currentUser.UserId ? f.UserId2 : f.UserId1, // Определяем FriendId
                })
                .ToList();

            // Для каждого друга получаем имя из списка пользователей
            var friendList = friends.Select(f =>
            {
                var username = users.FirstOrDefault(u => u.UserId == f.FriendId)?.Username;
                return new
                {
                    FriendId = f.FriendId,
                    Username = username ?? "Не найден"
                };
            }).ToList();

            // Привязка списка друзей к ListBox
            FriendsList.ItemsSource = friendList;
        }



        private async void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            // Проверка валидности ввода
            if (int.TryParse(FriendIdInput.Text, out int friendId))
            {
                if (friendId == currentUser.UserId)
                {
                    MessageBox.Show("Вы не можете добавить себя в друзья.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка, существует ли пользователь с указанным ID
                var friend = await dbContext.Users.FindAsync(friendId);
                if (friend == null)
                {
                    MessageBox.Show("Пользователь с таким ID не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка, не являются ли они уже друзьями
                bool alreadyFriends = dbContext.Friends.Any(f =>
                    (f.UserId1 == currentUser.UserId && f.UserId2 == friendId) ||
                    (f.UserId1 == friendId && f.UserId2 == currentUser.UserId));
                if (alreadyFriends)
                {
                    MessageBox.Show("Этот пользователь уже в вашем списке друзей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Добавление в друзья
                dbContext.Friends.Add(new Friend
                {
                    UserId1 = currentUser.UserId,
                    UserId2 = friendId
                });

                await dbContext.SaveChangesAsync();

                MessageBox.Show("Друг успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновление списка друзей
                LoadFriends();

                // Очистка поля ввода
                FriendIdInput.Clear();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректный ID.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            // Проверка на null для Tag
            if (button.Tag == null)
            {
                MessageBox.Show("Не указан FriendId.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Преобразование Tag в integer (FriendId)
            if (int.TryParse(button.Tag.ToString(), out int friendId))
            {
                // Ищем запись о дружбе в базе данных по UserId1 и UserId2
                var friendship = dbContext.Friends
                    .FirstOrDefault(f =>
                        (f.UserId1 == currentUser.UserId && f.UserId2 == friendId) ||
                        (f.UserId1 == friendId && f.UserId2 == currentUser.UserId));

                if (friendship != null)
                {
                    // Удаление дружбы из базы данных
                    dbContext.Friends.Remove(friendship);
                    await dbContext.SaveChangesAsync();

                    // Обновление списка друзей
                    LoadFriends();

                    MessageBox.Show("Друг успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Эта дружба не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Ошибка при преобразовании FriendId.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
