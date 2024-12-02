using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace zxcSteam2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBforISGameContext dbContext;
        public MainWindow()
        {
            InitializeComponent();
            dbContext = new DBforISGameContext();
            ClearUserData();
        }

        private async void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            // Проверка, есть ли логин и пароль
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста введите логин и пароль");
                return;
            }

            // Проверка в базе данных через контекст данных
            try
            {
                // Получаем пользователя из базы данных
                var user = dbContext.Users.FirstOrDefault(u => (u.Email == login || u.Username == login) && u.Password == password);

                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    await dbContext.SaveChangesAsync(); // await используется корректно
                    // ... (Успешная авторизация) ...
                    WindowProfile mainWindow2 = new WindowProfile(user, dbContext); // Передача объекта User и контекста
                    mainWindow2.Show();
                    this.Close();
                }

                else
                {
                    MessageBox.Show("Неправильный логин или пароль");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка");
            }

        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            WindowReg mainWindow2 = new WindowReg();
            mainWindow2.Show();
            this.Close();
        }

        private void ClearUserData()
        {
            txtLogin.Text = "";
            txtPassword.Password = "";
        }

        private void btnSignIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Нажата клавиша Enter
                btnSignIn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Симулируем нажатие кнопки "Войти"
            }
            else if (e.Key == Key.Tab)
            {
                // Нажата клавиша Tab
                btnReg.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Симулируем нажатие кнопки "Регистрация"
            }
        }
    }
}