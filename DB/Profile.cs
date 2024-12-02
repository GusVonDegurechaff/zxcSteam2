using System;
using System.Collections.Generic;

namespace zxcSteam2
{
    public partial class Profile
    {
        public Profile()
        {
            Statistics = new HashSet<Statistic>();
        }

        public int ProfileId { get; set; }
        public int GameId { get; set; }
        public int StatId { get; set; }
        public string ProfileName { get; set; } = null!;
        public int UserId { get; set; }

        public virtual Game Game { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Statistic> Statistics { get; set; }
    }
}
