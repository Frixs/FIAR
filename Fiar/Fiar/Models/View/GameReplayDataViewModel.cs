using System.Collections.Generic;

namespace Fiar.ViewModels
{
    public class GameReplayDataViewModel
    {
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public List<GameBoardCellType[][]> BoardHistory { get; set; }
    }
}
