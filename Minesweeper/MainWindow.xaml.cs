using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int SizeX = 13;
        public const int SizeY = 13;

        public int XUnit;
        public int YUnit;

        public bool IsRunning = true;

        public int[][] MinesMap = new int[SizeY][];
        public int[][] NumbersMap = new int[SizeY][];
        public int[][] VisibleMap = new int[SizeY][];

        public MainWindow()
        {
            InitializeComponent();
            XUnit = (int)CanvasElement.Width / SizeX;
            YUnit = (int)CanvasElement.Height / SizeY;

        GenerateMap();
            DrawMap();
        }

        public void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsRunning)
            {
                return;
            }

            var clickedPoint = Mouse.GetPosition(CanvasElement);

            var y = (int)(clickedPoint.Y * SizeY / CanvasElement.Height);
            var x = (int)(clickedPoint.X * SizeX / CanvasElement.Width);
            VisibleMap[y][x] = 1;

            if (MinesMap[y][x] == 1)
            {
                IsRunning = false;
                DrawMap();
                DrawMines();
                MessageBox.Show("Lost!");
                return;
            }

            if (isWin())
            {
                IsRunning = false;
                DrawMap();
                DrawMines();
                MessageBox.Show("Win!");
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

            DrawMap();
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

        public void Restart_Click(object sender, RoutedEventArgs e)
        {
            IsRunning = true;
            GenerateMap();
            DrawMap();
        }

        public void DrawMap()
        {
            CanvasElement.Children.Clear();

            DrawMines();

            // draw invisible map's parts
            for (var i = 0; i < SizeY; i++)
            {
                for (var j = 0; j < SizeX; j++)
                {
                    if (VisibleMap[i][j] != 1)
                    {
                        var invisibleBlock = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Gray),
                            Width = XUnit,
                            Height = YUnit
                        };

                        Canvas.SetTop(invisibleBlock, i * YUnit);
                        Canvas.SetLeft(invisibleBlock, j * XUnit);

                        CanvasElement.Children.Add(invisibleBlock);
                    }
                }
            }

            // draw vertical lines
            for (var i = 0; i <= SizeY; i++)
            {
                var line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Black),
                    X1 = i * XUnit,
                    X2 = i * XUnit,
                    Y1 = 0,
                    Y2 = YUnit * SizeY,
                };

                CanvasElement.Children.Add(line);
            }

            // draw horizontal lines
            for (var i = 0; i <= SizeX; i++)
            {
                var line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Black),
                    X1 = 0,
                    X2 = XUnit * SizeX,
                    Y1 = i * YUnit,
                    Y2 = i * YUnit,
                };

                CanvasElement.Children.Add(line);
            }
        }

        public void DrawMines()
        {
            // draw mines/numbers
            var mineMargin = 12;
            for (var i = 0; i < SizeY; i++)
            {
                for (var j = 0; j < SizeX; j++)
                {
                    if (MinesMap[i][j] == 1)
                    {
                        var mine = new Rectangle()
                        {
                            Fill = new SolidColorBrush(Colors.Red),
                            Width = XUnit - mineMargin,
                            Height = YUnit - mineMargin
                        };

                        Canvas.SetTop(mine, i * YUnit + mineMargin / 2);
                        Canvas.SetLeft(mine, j * XUnit + mineMargin / 2);

                        CanvasElement.Children.Add(mine);
                    }
                    else if (NumbersMap[i][j] != 0)
                    {
                        var number = new TextBlock()
                        {
                            Text = NumbersMap[i][j].ToString(),
                            Foreground = new SolidColorBrush(Colors.Blue),
                            FontSize = 20
                        };

                        Canvas.SetTop(number, i * YUnit);
                        Canvas.SetLeft(number, j * XUnit + mineMargin);
                        CanvasElement.Children.Add(number);
                    }
                }
            }
        }

        public void GenerateMap()
        {
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
