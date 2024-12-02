using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для StatsPage.xaml
    /// </summary>
    public partial class StatsPage : Page
    {
        private readonly DBforISGameContext dbContext;
        private readonly User currentUser;

        public StatsPage(DBforISGameContext context, User user)
        {
            InitializeComponent();
            dbContext = context;
            currentUser = user;
            LoadStats();
        }

        private void LoadStats()
        {
            var stats = dbContext.Statistics
                .Join(dbContext.Profiles, s => s.ProfileId, p => p.ProfileId, (s, p) => new { s, p })
                .Where(sp => sp.p.UserId == currentUser.UserId)
                .Select(sp => new { sp.p.ProfileName, sp.s.HoursInGame, sp.s.Rank, sp.s.Lvl })
                .ToList();

            StatsGrid.ItemsSource = stats;
        }
    }
}
