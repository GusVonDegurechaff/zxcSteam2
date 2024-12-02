using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace zxcSteam2
{
    /// <summary>
    /// Логика взаимодействия для WindowReg.xaml
    /// </summary>
    public partial class WindowReg : Window

    {
        private DBforISGameContext dbContext;
        public WindowReg()
        {
            InitializeComponent();
            dbContext = new DBforISGameContext();
        }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = usernamereg.Text;
            string email = emailreg.Text;
            string password = passwordreg.Password;
            string checkPassword = checkpasswordreg.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(checkPassword))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            // Проверка на совпадение паролей
            if (password != checkPassword)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            try
            {
                // Добавление пользователя в базу данных (НЕБЕЗОПАСНО!)
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    Password = password, // !!! НЕБЕЗОПАСНО !!!  Хранение паролей в открытом виде.
                    RegistrationDate = DateTime.Now
                };

                dbContext.Users.Add(newUser);
                dbContext.SaveChanges(); // или _dbContext.SaveChangesAsync() для асинхронной операции

                MessageBox.Show("Регистрация успешна!");
                MainWindow mainWindow1 = new MainWindow();
                mainWindow1.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void RegisterBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow1 = new MainWindow();
            mainWindow1.Show();
            this.Close();
        }
    }
}

