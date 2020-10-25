using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    public class MinesweeperGame
    {
        public readonly int SizeX = 13;
        public readonly int SizeY = 13;

        public bool IsRunning { get; private set; } = true;
        public GameResult GameResult { get; private set; } = GameResult.None;

        public readonly int[][] MinesMap;
        public readonly int[][] NumbersMap;
        public readonly int[][] VisibleMap;

        public Action<GameResult> OneGameEnd = delegate { };

        public MinesweeperGame()
        {
            MinesMap = new int[SizeY][];
            NumbersMap = new int[SizeY][];
            VisibleMap = new int[SizeY][];
        }

        public void MakeGuess(int y, int x)
        {
            VisibleMap[y][x] = 1;

            if (MinesMap[y][x] == 1)
            {
                GameResult = GameResult.Loose;
            }
            else if (isWin())
            {
                GameResult = GameResult.Win;
            }

            if (GameResult != GameResult.None)
            {
                IsRunning = false;
                OneGameEnd(GameResult);
                return;
            }

            var closestSquares = new List<(int y, int x)>();

            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    if (y + di >= 0 && y + di < SizeY && x + dj >= 0 && x + dj < SizeX
                        && MinesMap[y + di][x + dj] == 0 && NumbersMap[y + di][x + dj] == 0)
                    {
                        closestSquares.Add((y + di, x + dj));
                    }
                }
            }

            foreach (var square in closestSquares)
            {
                var closedEmptySquares = GetClosedEmptySquares(square.y, square.x);

                foreach (var closedEmptySquare in closedEmptySquares)
                {
                    VisibleMap[closedEmptySquare.y][closedEmptySquare.x] = 1;
                }

                VisibleMap[square.y][square.x] = 1;
            }
        }

        private List<(int y, int x)> GetClosedEmptySquares(int y, int x, List<(int y, int x)> visited = null)
        {
            visited = visited ?? new List<(int, int)>();
            var output = new List<(int, int)>();

            if (MinesMap[y][x] == 1 || NumbersMap[y][x] != 0)
            {
                return output;
            }

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (y + dy >= 0 && y + dy < SizeY && x + dx >= 0 && x + dx < SizeX
                        && !visited.Any(v => v.y == y + dy && v.x == x + dx))
                    {
                        visited.Add((y + dy, x + dx));
                        output.Add((y + dy, x + dx));
                        output.AddRange(GetClosedEmptySquares(y + dy, x + dx, visited));
                    }
                }
            }

            return output;
        }

        public bool isWin()
        {
            for (var i = 0; i < SizeY; i++)
            {
                for (var j = 0; j < SizeX; j++)
                {
                    if (VisibleMap[i][j] == 0 ^ MinesMap[i][j] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Initialize()
        {
            IsRunning = true;
            GameResult = GameResult.None;

            // reset visibility
            for (var i = 0; i < SizeY; i++)
            {
                VisibleMap[i] = new int[SizeX];
            }

            // generate mines
            for (var i = 0; i < SizeY; i++)
            {
                MinesMap[i] = new int[SizeX];

                for (var j = 0; j < SizeX; j++)
                {
                    if (new Random().NextDouble() > 0.85)
                    {
                        MinesMap[i][j] = 1;
                    }
                }
            }

            // generate numbers
            for (var i = 0; i < SizeY; i++)
            {
                NumbersMap[i] = new int[SizeX];

                for (var j = 0; j < SizeX; j++)
                {
                    if (MinesMap[i][j] == 1)
                    {
                        continue;
                    }

                    var minesNumber = 0;

                    for (int di = -1; di <= 1; di++)
                    {
                        for (int dj = -1; dj <= 1; dj++)
                        {
                            if (i + di >= 0 && i + di < SizeY && j + dj >= 0 && j + dj < SizeX && MinesMap[i + di][j + dj] == 1)
                            {
                                minesNumber++;
                            }
                        }

                        NumbersMap[i][j] = minesNumber;
                    }
                }
            }
        }

    }
}
