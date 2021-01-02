using System.Collections.Generic;

namespace Fiar.ViewModels
{
    public class MyGamesViewModel
    {
        public List<MyGameDataViewModel> MyGames { get; set; }

        public int TotalWins { get; set; }

        public int TotalLosses { get; set; }
    }
}
