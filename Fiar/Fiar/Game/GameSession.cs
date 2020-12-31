using System;

namespace Fiar
{
    /// <summary>
    /// Running game session context
    /// </summary>
    public class GameSession
    {
        #region Constants

        public const int StartingNumberOfRows = 15;
        public const int MaxNumberOfRows = 75;
        public const int StartingNumberOfColumns = 15;
        public const int MaxNumberOfColumns = 75;
        public const int ChainLengthToWin = 5;

        #endregion

        #region Public Properties

        /// <summary>
        /// <see cref="GameDataModel.Id"/>
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// <see cref="ApplicationUser.Id"/> of player 1
        /// It indicates who is the player 1
        /// </summary>
        public string PlayerOneUserId { get; private set; }

        /// <summary>
        /// <see cref="ApplicationUser.Id"/> of player 2
        /// It indicates who is the player 2
        /// </summary>
        public string PlayerTwoUserId { get; private set; }

        /// <summary>
        /// Defines player 1 instance joined into the game
        /// </summary>
        public Player PlayerOne { get; set; }

        /// <summary>
        /// Defines player 2 instance joined into the game
        /// </summary>
        public Player PlayerTwo { get; set; }

        /// <summary>
        /// Points to the current player object on turn
        /// </summary>
        /// <remarks>
        ///     Should not be null
        /// </remarks>
        public Player CurrentPlayer { get; private set; }

        /// <summary>
        /// Indicates if the game is in progress (true) or not (false)
        /// </summary>
        /// <remarks>
        ///     It indicates the game already started (true).
        /// </remarks>
        public bool InProgress { get; set; }

        /// <summary>
        /// The board representation
        /// </summary>
        public GameBoardCellType[][] Board { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameSession(string playerOneUserId, string playerTwoUserId)
        {
            PlayerOneUserId = playerOneUserId;
            PlayerTwoUserId = playerTwoUserId;

            PopulateBoard();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get player based on connection ID (SignalR)
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <returns>The player object or null on failure</returns>
        public Player GetPlayer(string connectionId)
        {
            if (PlayerOne?.ConnectionId.Equals(connectionId) == true)
                return PlayerOne;
            if (PlayerTwo?.ConnectionId.Equals(connectionId) == true)
                return PlayerTwo;
            return null;
        }

        /// <summary>
        /// Recognize player in a game based on connection ID (SignalR)
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <returns>Player type or null on failure</returns>
        private PlayerType? RecognizePlayerType(string connectionId)
        {
            if (PlayerOne?.ConnectionId.Equals(connectionId) == true)
                return PlayerType.PlayerOne;
            if (PlayerTwo?.ConnectionId.Equals(connectionId) == true)
                return PlayerType.PlayerTwo;
            return null;
        }

        /// <summary>
        /// Indicates if the player is part of this game
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <returns>TRUE, player is in the game. FALSE, otherwise.</returns>
        public bool HasPlayer(string connectionId)
        {
            if (PlayerOne?.ConnectionId.Equals(connectionId) == true)
                return true;
            if (PlayerTwo?.ConnectionId.Equals(connectionId) == true)
                return true;
            return false;
        }

        /// <summary>
        /// If the position is in range of expansion, the board will expand the size
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        public bool TryExpandBoard(int row, int column)
        {
            bool result = false;

            // Expand to top
            if (Board.Length < MaxNumberOfRows && row - ChainLengthToWin < 0)
            {
                System.Diagnostics.Debug.WriteLine("top");
                // Get the new size to expand to
                var additionalSize = 0;
                for (int i = 0; i < Math.Abs(row - ChainLengthToWin); ++i)
                {
                    if (Board.Length + additionalSize + 1 > MaxNumberOfRows)
                        break;
                    ++additionalSize;
                }

                // Create a new expanded board
                GameBoardCellType[][] newBoard = new GameBoardCellType[Board.Length + additionalSize][];
                for (int y = 0; y < Board.Length + additionalSize; ++y)
                {
                    newBoard[y] = new GameBoardCellType[Board[0].Length];
                    for (int x = 0; x < Board[0].Length; ++x)
                    {
                        if (y >= additionalSize)
                            newBoard[y][x] = Board[y - additionalSize][x];
                        else
                            newBoard[y][x] = GameBoardCellType.Empty;
                    }
                }
                // Replace the old board with the new one
                Board = newBoard;
                result = true;
            }
            // Expand to left
            if (Board[0].Length < MaxNumberOfColumns && column - ChainLengthToWin < 0)
            {
                System.Diagnostics.Debug.WriteLine("left");
                // Get the new size to expand to
                var additionalSize = 0;
                for (int i = 0; i < Math.Abs(column - ChainLengthToWin); ++i)
                {
                    if (Board[0].Length + additionalSize + 1 > MaxNumberOfColumns)
                        break;
                    ++additionalSize;
                }
                
                // Create a new expanded board
                GameBoardCellType[][] newBoard = new GameBoardCellType[Board.Length][];
                for (int y = 0; y < Board.Length; ++y)
                {
                    newBoard[y] = new GameBoardCellType[Board[0].Length + additionalSize];
                    for (int x = 0; x < Board[0].Length + additionalSize; ++x)
                    {
                        if (x >= additionalSize)
                            newBoard[y][x] = Board[y][x - additionalSize];
                        else
                            newBoard[y][x] = GameBoardCellType.Empty;
                    }
                }
                // Replace the old board with the new one
                Board = newBoard;
                result = true;
            }
            // Expand to bottom
            if (Board.Length < MaxNumberOfRows && row + ChainLengthToWin >= Board.Length)
            {
                // Get the new size to expand to
                var additionalSize = 0;
                for (int i = 0; i < row + ChainLengthToWin + 1 - Board.Length; ++i)
                {
                    if (Board.Length + additionalSize + 1 > MaxNumberOfRows)
                        break;
                    ++additionalSize;
                }
                
                // Create a new expanded board
                GameBoardCellType[][] newBoard = new GameBoardCellType[Board.Length + additionalSize][];
                for (int y = 0; y < Board.Length + additionalSize; ++y)
                {
                    newBoard[y] = new GameBoardCellType[Board[0].Length];
                    for (int x = 0; x < Board[0].Length; ++x)
                    {
                        if (y < Board.Length)
                            newBoard[y][x] = Board[y][x];
                        else
                            newBoard[y][x] = GameBoardCellType.Empty;
                    }
                }
                // Replace the old board with the new one
                Board = newBoard;
                result = true;
            }
            // Expand to right
            if (Board[0].Length < MaxNumberOfColumns && column + ChainLengthToWin >= Board[0].Length)
            {
                // Get the new size to expand to
                var additionalSize = 0;
                for (int i = 0; i < column + ChainLengthToWin + 1 - Board[0].Length; ++i)
                {
                    if (Board[0].Length + additionalSize + 1 > MaxNumberOfColumns)
                        break;
                    ++additionalSize;
                }

                // Create a new expanded board
                GameBoardCellType[][] newBoard = new GameBoardCellType[Board.Length][];
                for (int y = 0; y < Board.Length; ++y)
                {
                    newBoard[y] = new GameBoardCellType[Board[0].Length + additionalSize];
                    for (int x = 0; x < Board[0].Length + additionalSize; ++x)
                    {
                        if (x < Board[0].Length)
                            newBoard[y][x] = Board[y][x];
                        else
                            newBoard[y][x] = GameBoardCellType.Empty;
                    }
                }
                // Replace the old board with the new one
                Board = newBoard;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Check the victory conditions after player's turn
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        /// <returns>TRUE win conditions are matched, FALSE otherwise</returns>
        public bool CheckVictory(int row, int column)
        {
            var playerCellType = Board[row][column];

            if (CheckVictoryHorizontally(row, column, playerCellType))
                return true;

            if (CheckVictoryVertically(row, column, playerCellType))
                return true;

            if (CheckVictoryDiagonally(row, column, playerCellType))
                return true;

            return false;
        }

        /// <summary>
        /// Try assign player to a cell on a position
        /// </summary>
        /// <param name="row">The row index</param>
        /// <param name="column">The column index</param>
        /// <returns>TRUE, successfully assigned. FALSE, otherwise.</returns>
        public bool TryAssignPlayerToCell(int row, int column)
        {
            if (Board[row][column] == GameBoardCellType.Empty)
            {
                Board[row][column] = RecognizePlayerCellType(CurrentPlayer);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move the pointer to the next player on turn
        /// </summary>
        public void MoveTurnPointerToNextPlayer()
        {
            if (CurrentPlayer != null)
            {

                if (CurrentPlayer.Id.Equals(PlayerOne.Id))
                    CurrentPlayer = PlayerTwo;
                else
                    CurrentPlayer = PlayerOne;
            }
            else
            {
                CurrentPlayer = PlayerTwo;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Prepare the board for the play
        /// </summary>
        private void PopulateBoard()
        {
            Board = new GameBoardCellType[StartingNumberOfRows][];

            for (int y = 0; y < StartingNumberOfRows; ++y)
            {
                Board[y] = new GameBoardCellType[StartingNumberOfColumns];
                for (int x = 0; x < StartingNumberOfColumns; ++x)
                {
                    Board[y][x] = GameBoardCellType.Empty;
                }
            }
        }

        /// <summary>
        /// Check victory conditions horizontally
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        /// <param name="player">The player's cell type</param>
        /// <returns>TRUE win conditions are matched, FALSE otherwise</returns>
        private bool CheckVictoryHorizontally(int row, int column, GameBoardCellType player)
        {
            var startColumn = column;
            var endColumn = column;

            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var columnToCheck = column - k;

                if (columnToCheck < 0)
                    break;
                if (Board[row][columnToCheck] != player)
                    break;

                startColumn = columnToCheck;
            }
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var columnToCheck = column + k;

                if (columnToCheck >= Board[0].Length)
                    break;
                if (Board[row][columnToCheck] != player)
                    break;

                endColumn = columnToCheck;
            }

            if (endColumn - startColumn >= ChainLengthToWin - 1)
            {
                for (var k = 0; k < ChainLengthToWin; ++k)
                    Board[row][startColumn + k] = (GameBoardCellType)((int)player + 2); // 2 players

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check victory conditions vertically
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        /// <param name="player">The player's cell type</param>
        /// <returns>TRUE win conditions are matched, FALSE otherwise</returns>
        private bool CheckVictoryVertically(int row, int column, GameBoardCellType player)
        {
            var startRow = row;
            var endRow = row;

            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row - k;

                if (rowToCheck < 0)
                    break;
                if (Board[rowToCheck][column] != player)
                    break;

                startRow = rowToCheck;
            }
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row + k;

                if (rowToCheck >= Board.Length)
                    break;
                if (Board[rowToCheck][column] != player)
                    break;

                endRow = rowToCheck;
            }

            if (endRow - startRow >= ChainLengthToWin - 1)
            {
                for (var k = 0; k < ChainLengthToWin; ++k)
                    Board[startRow + k][column] = (GameBoardCellType)((int)player + 2); // 2 players

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check victory conditions diagonally (all)
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        /// <param name="player">The player's cell type</param>
        /// <returns>TRUE win conditions are matched, FALSE otherwise</returns>
        private bool CheckVictoryDiagonally(int row, int column, GameBoardCellType player)
        {
            var startRow = row;
            var startColumn = column;
            var endColumn = column;

            // Check diagonal top-left -> bottom-right        
            // Count to top-left
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row - k;
                var columnToCheck = column - k;

                if (rowToCheck < 0 || columnToCheck < 0)
                    break;
                if (Board[rowToCheck][columnToCheck] != player)
                    break;

                startRow = rowToCheck;
                startColumn = columnToCheck;
            }

            // Count to bottom-right
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row + k;
                var columnToCheck = column + k;

                if (rowToCheck >= Board.Length || columnToCheck >= Board[0].Length)
                    break;
                if (Board[rowToCheck][columnToCheck] != player)
                    break;

                endColumn = columnToCheck;
            }

            if (endColumn - startColumn >= ChainLengthToWin - 1)
            {
                for (var k = 0; k < ChainLengthToWin; ++k)
                    Board[startRow + k][startColumn + k] = (GameBoardCellType)((int)player + 2); // 2 players

                return true;
            }

            startRow = row;
            startColumn = column;
            endColumn = column;

            // Count to bottom-left
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row + k;
                var columnToCheck = column - k;

                if (rowToCheck >= Board.Length || columnToCheck < 0)
                    break;
                if (Board[rowToCheck][columnToCheck] != player)
                    break;

                startRow = rowToCheck;
                startColumn = columnToCheck;
            }

            // Count to top-right
            for (var k = 1; k < ChainLengthToWin; ++k)
            {
                var rowToCheck = row - k;
                var columnToCheck = column + k;

                if (rowToCheck < 0 || columnToCheck >= Board[0].Length)
                    break;
                if (Board[rowToCheck][columnToCheck] != player)
                    break;

                endColumn = columnToCheck;
            }

            if (endColumn - startColumn >= ChainLengthToWin - 1)
            {
                for (var k = 0; k < ChainLengthToWin; ++k)
                    Board[startRow - k][startColumn + k] = (GameBoardCellType)((int)player + 2); // 2 players

                return true;
            }

            return false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Recognize the player cell type based on the player object
        /// </summary>
        /// <param name="player">The player object</param>
        /// <returns>The cell type of the player</returns>
        private GameBoardCellType RecognizePlayerCellType(Player player)
        {
            if (player.Type == PlayerType.PlayerTwo)
                return GameBoardCellType.PlayerTwo;
            return GameBoardCellType.PlayerOne;
        }

        #endregion
    }
}
