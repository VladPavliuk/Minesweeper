using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;

namespace Minesweeper
{
    public class MinesweeperNetwork
    {
        public NeuralNetwork NeuralNetwork { get; set; }
        public MinesweeperState MinesweeperState { get; set; }

        public List<(int, int)> MadeMoves { get; set; } = new List<(int, int)>();

        public int WinCount = 1;
        public int LooseCount = 1;
        public double[] Training;
        public MinesweeperNetwork(MinesweeperState minesweeperState)
        {
            MinesweeperState = minesweeperState;
            NeuralNetwork = new NeuralNetwork(
                MinesweeperState.SizeX * MinesweeperState.SizeY * 9, // 8 numbers + 1 visibility
                                                                     //MinesweeperState.SizeX * MinesweeperState.SizeY * 2,
                MinesweeperState.SizeX * MinesweeperState.SizeY,
                2,
                13,
                x => 1 / (1 + Math.Exp(-x)));
        }

        public (double[], double[]) Learn()
        {
            var learningRate = .5d;
            var error = 0d;

            var training = new double[MinesweeperState.SizeX * MinesweeperState.SizeY];
            var outputs = NeuralNetwork.Outputs[NeuralNetwork.Outputs.Length - 1];

            var backpropagationReapeatCount = 1;

            var marginSquares = new List<int>();
            for (int i = 0; i < MinesweeperState.SizeX * MinesweeperState.SizeY; i++)
            {
                var y = i / MinesweeperState.SizeY;
                var x = i % MinesweeperState.SizeX;

                if (MinesweeperState.VisibleMap[y][x] == 1)
                {
                    training[i] = 0;
                }
                else
                {
                    var isCloseToVisible = false;

                    for (int dy = -1; dy <= 1 && !isCloseToVisible; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (y + dy >= 0 && y + dy < MinesweeperState.SizeY && x + dx >= 0 && x + dx < MinesweeperState.SizeX
                                && MinesweeperState.VisibleMap[y + dy][x + dx] == 1)
                            {
                                isCloseToVisible = true;
                                break;
                            }
                        }
                    }

                    training[i] = 0;

                    if (isCloseToVisible)
                    {
                        marginSquares.Add(i);
                    }
                }

                error += Math.Pow(outputs[i] - training[i], 2);
            }

            var marginSquaresAmount = marginSquares.Count;
            foreach (var marginSquareIndex in marginSquares)
            {
                training[marginSquareIndex] = 1.0d / marginSquaresAmount;
            }

            if (!MadeMoves.Any(m => m == MinesweeperState.LastMove))
            {
                //backpropagationReapeatCount = 5;
                training[MinesweeperState.SizeY * MinesweeperState.LastMove.y + MinesweeperState.LastMove.x] = 1;
                MadeMoves.Add(MinesweeperState.LastMove);
            }

            if (MinesweeperState.GameResult == GameResult.Loose)
            {
                //backpropagationReapeatCount = 5;

                var safeMarginSquares = marginSquares.Where(s => MinesweeperState.MinesMap[s / MinesweeperState.SizeY][s % MinesweeperState.SizeX] == 0);

                training[MinesweeperState.SizeY * MinesweeperState.LastMove.y + MinesweeperState.LastMove.x] = 0;
                var moreZero = safeMarginSquares.Count() != 0 ? 1.0d / safeMarginSquares.Count() : 0;

                for (int i = 0; i < marginSquares.Count; i++)
                {
                    if (MinesweeperState.MinesMap[marginSquares[i] / MinesweeperState.SizeY][marginSquares[i] % MinesweeperState.SizeX] == 0)
                    {
                        training[marginSquares[i]] = moreZero;
                    }
                    else
                    {
                        training[marginSquares[i]] = 0;
                    }
                }
                //learningRate = .2d;
                LooseCount++;
            }
            else if (MinesweeperState.GameResult == GameResult.Win)
            {
                WinCount++;
            }

            if (!MinesweeperState.IsRunning)
            {
                //if (LooseCount > 1000)
                //{
                //    WinCount = 1;
                //    LooseCount = 1;
                //}
                MadeMoves = new List<(int, int)>();
                Debug.WriteLine("win/loose: " + WinCount + " - " + LooseCount + ", rate: " + (double)WinCount / LooseCount + " s: " + outputs.Sum());
            }

            for (int r = 0; r < backpropagationReapeatCount; r++)
            {
                //Debug.WriteLine("error: " + error);
                NeuralNetwork.ApplyBackpropagation(training, learningRate);
                //learningRate = .02d;
            }

            return (training, outputs);
        }

        public (int y, int x) Predict()
        {
            var inputs = GetInputsFromState(MinesweeperState);
            var outputs = NeuralNetwork.ForwardPass(inputs);

            var maxIndex = 0;
            double maxValue = 0;

            for (int i = 0; i < outputs.Length; i++)
            {
                var y = i / MinesweeperState.SizeY;
                var x = i % MinesweeperState.SizeX;

                var isCloseToVisible = false;

                for (int dy = -1; dy <= 1 && !isCloseToVisible; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (y + dy >= 0 && y + dy < MinesweeperState.SizeY && x + dx >= 0 && x + dx < MinesweeperState.SizeX
                            && MinesweeperState.VisibleMap[y + dy][x + dx] == 1)
                        {
                            isCloseToVisible = true;
                            break;
                        }
                    }
                }

                if (isCloseToVisible && MinesweeperState.VisibleMap[y][x] == 0 && outputs[i] > maxValue)
                {
                    maxIndex = i;
                    maxValue = outputs[i];
                }
            }

            return (maxIndex / MinesweeperState.SizeY, maxIndex % MinesweeperState.SizeX);
        }

        double[] GetInputsFromState(MinesweeperState minesweeperState)
        {
            var size = minesweeperState.SizeX * minesweeperState.SizeY;
            var inputSize = size * 9; // 8 numbers + 1 visibility;
            //var inputSize = size * 2;

            var inputs = new double[inputSize];
 
            for (int i = 0; i < size; i++)
            {
                inputs[i] = minesweeperState.VisibleMap[i / minesweeperState.SizeY][i % minesweeperState.SizeX];
            }

            for (int n = 1; n <= 8; n++)
            {
                for (int i = 0; i < size; i++)
                {
                    var y = i / minesweeperState.SizeY;
                    var x = i % minesweeperState.SizeX;

                    if (minesweeperState.VisibleMap[y][x] == 1 && minesweeperState.NumbersMap[y][x] == n)
                    {
                        inputs[i + size * n] = 1;
                    }
                }
            }

            //for (int i = 0; i < size; i++)
            //{
            //    var y = i / minesweeperState.SizeY;
            //    var x = i % minesweeperState.SizeX;
            //    if (minesweeperState.VisibleMap[y][x] == 1)
            //    {
            //        inputs[i] = minesweeperState.NumbersMap[y][x];
            //    }
            //}

            return inputs;
        }
    }
}
