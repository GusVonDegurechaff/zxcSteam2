using System;
using System.Collections.Generic;

namespace zxcSteam2
{
    public partial class Statistic
    {
        public int StatId { get; set; }
        public int ProfileId { get; set; }
        public string HoursInGame { get; set; } = null!;
        public string Rank { get; set; } = null!;
        public string Lvl { get; set; } = null!;

        public virtual Profile Profile { get; set; } = null!;
    }
}
