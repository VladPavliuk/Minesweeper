using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        public MinesweeperState ActiveGame { get; set; }

        public MainWindow()
        {
            ActiveGame = new MinesweeperState()
            {
                //OneGameEnd = gameResult =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        DrawMap();
                //        DrawMines();
                //    });
                //    if (gameResult == GameResult.Win)
                //    {
                //        //Debug.WriteLine("Win!");
                //        MessageBox.Show("Win!");
                //    }
                //    else if (gameResult == GameResult.Loose)
                //    {
                //        //Debug.WriteLine("Loose!");
                //        MessageBox.Show("Loose!");
                //    }

                //    ActiveGame.Initialize();
                //}
            };

            InitializeComponent();
            XUnit = (int)CanvasElement.Width / ActiveGame.SizeX;
            YUnit = (int)CanvasElement.Height / ActiveGame.SizeY;

            ActiveGame.Initialize();
            DrawMap();

            var updateFileIterator = 10;
            var neuralNetworkDataFilePath = "../../../NeuralNetworkData.txt";
            var network = new MinesweeperNetwork(ActiveGame);

            try
            {
                var neuralNetworkDataText = File.ReadAllText(neuralNetworkDataFilePath);

                var neuralNetworkData = JsonConvert.DeserializeObject<NeuralNetworkData>(neuralNetworkDataText);

                // Keep in mind these arrays are copied by the reference.
                network.NeuralNetwork.Weights = neuralNetworkData.Weights;
                network.NeuralNetwork.Bias = neuralNetworkData.Bias;
            }
            catch (FileNotFoundException)
            {
                File.CreateText(neuralNetworkDataFilePath);
            }

            Task.Factory.StartNew(() =>
            {
                var alreadyOpenSqaureClick = 0;

                while (ActiveGame.IsRunning)
                {
                    var (y, x) = network.Predict();

                    if (ActiveGame.VisibleMap[y][x] == 1)
                    {
                        alreadyOpenSqaureClick++;

                        if (alreadyOpenSqaureClick > 5)
                        {
                            alreadyOpenSqaureClick = 0;
                            ActiveGame.Initialize();
                            continue;
                        }
                    }

                    ActiveGame.MakeGuess(y, x);
                    var (training, outputs) = network.Learn();

                    this.Dispatcher.Invoke(() =>
                    {
                        DrawMap();
                    });

                    //> draw training data
                    for (int i = 0; i < training.Length; i++)
                    {
                        var yT = i / ActiveGame.SizeY;
                        var xT = i % ActiveGame.SizeX;
                        this.Dispatcher.Invoke(() =>
                        {
                            var number = new TextBlock()
                            {
                                Text = Math.Round(training[i] * 1000.0d).ToString(),
                                Foreground = new SolidColorBrush(Colors.Green),
                                FontSize = 12
                            };

                            Canvas.SetTop(number, yT * YUnit + 4);
                            Canvas.SetLeft(number, xT * XUnit + 5);
                            CanvasElement.Children.Add(number);

                            var number2 = new TextBlock()
                            {
                                Text = Math.Round(outputs[i] * 1000.0d).ToString(),
                                Foreground = new SolidColorBrush(Colors.Purple),
                                FontSize = 12
                            };

                            Canvas.SetTop(number2, yT * YUnit + 15);
                            Canvas.SetLeft(number2, xT * XUnit + 5);
                            CanvasElement.Children.Add(number2);
                        });
                    }
                    //<
                    Thread.Sleep(500);

                    if (!ActiveGame.IsRunning)
                    {
                        updateFileIterator--;
                        this.Dispatcher.Invoke(() =>
                        {
                            //DrawMap();
                            //DrawMines();
                        });

                        if (updateFileIterator <= 0)
                        {
                            File.WriteAllText(neuralNetworkDataFilePath, JsonConvert.SerializeObject(
                                new NeuralNetworkData()
                                {
                                    Weights = network.NeuralNetwork.Weights,
                                    Bias = network.NeuralNetwork.Bias,
                                }
                            ));
                            updateFileIterator = 10;
                        }

                        ActiveGame.Initialize();
                    }
                }
            });
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
