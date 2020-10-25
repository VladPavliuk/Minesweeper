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
        public int XUnit;
        public int YUnit;

        public MinesweeperGame ActiveGame { get; set; }

        public MainWindow()
        {
            ActiveGame = new MinesweeperGame()
            {
                OneGameEnd = gameResult =>
                {
                    DrawMap();
                    DrawMines();
                    if (gameResult == GameResult.Win)
                    {
                        MessageBox.Show("Win!");
                    }
                    else if (gameResult == GameResult.Loose)
                    {
                        MessageBox.Show("Loose!");
                    }
                }
            };

            InitializeComponent();
            XUnit = (int)CanvasElement.Width / ActiveGame.SizeX;
            YUnit = (int)CanvasElement.Height / ActiveGame.SizeY;

            ActiveGame.Initialize();
            DrawMap();
        }

        public void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ActiveGame.IsRunning)
            {
                return;
            }

            var clickedPoint = Mouse.GetPosition(CanvasElement);

            var y = (int)(clickedPoint.Y * ActiveGame.SizeY / CanvasElement.Height);
            var x = (int)(clickedPoint.X * ActiveGame.SizeX / CanvasElement.Width);

            ActiveGame.MakeGuess(y, x);

            if (ActiveGame.IsRunning)
            {
                DrawMap();
            }
        }

        public void Restart_Click(object sender, RoutedEventArgs e)
        {
            ActiveGame.Initialize();
            DrawMap();
        }

        public void DrawMap()
        {
            CanvasElement.Children.Clear();

            DrawMines();

            // draw invisible map's parts
            for (var i = 0; i < ActiveGame.SizeY; i++)
            {
                for (var j = 0; j < ActiveGame.SizeX; j++)
                {
                    if (ActiveGame.VisibleMap[i][j] != 1)
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
            for (var i = 0; i <= ActiveGame.SizeY; i++)
            {
                var line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Black),
                    X1 = i * XUnit,
                    X2 = i * XUnit,
                    Y1 = 0,
                    Y2 = YUnit * ActiveGame.SizeY,
                };

                CanvasElement.Children.Add(line);
            }

            // draw horizontal lines
            for (var i = 0; i <= ActiveGame.SizeX; i++)
            {
                var line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Black),
                    X1 = 0,
                    X2 = XUnit * ActiveGame.SizeX,
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
            for (var i = 0; i < ActiveGame.SizeY; i++)
            {
                for (var j = 0; j < ActiveGame.SizeX; j++)
                {
                    if (ActiveGame.MinesMap[i][j] == 1)
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
                    else if (ActiveGame.NumbersMap[i][j] != 0)
                    {
                        var number = new TextBlock()
                        {
                            Text = ActiveGame.NumbersMap[i][j].ToString(),
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
    }
}
