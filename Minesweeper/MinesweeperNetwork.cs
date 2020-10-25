using System;
using System.Diagnostics;

namespace Minesweeper
{
    public class MinesweeperNetwork
    {
        public NeuralNetwork NeuralNetwork { get; set; }
        public MinesweeperState MinesweeperState { get; set; }

        public int WinCount = 1;
        public int LooseCount = 1;

        public MinesweeperNetwork(MinesweeperState minesweeperState)
        {
            MinesweeperState = minesweeperState;
            NeuralNetwork = new NeuralNetwork(
                MinesweeperState.SizeX * MinesweeperState.SizeY * 9, // 8 numbers + 1 visibility
                MinesweeperState.SizeX * MinesweeperState.SizeY,
                4,
                10,
                x => 1 / (1 + Math.Exp(-x)));
        }

        public void Learn()
        {
            var learningRate = .02d;
            //var stopTraining = false;
            var error = 0d;

            var training = new double[MinesweeperState.SizeX * MinesweeperState.SizeY];
            var outputs = NeuralNetwork.Outputs[NeuralNetwork.Outputs.Length - 1];

            for (int r = 0; r < 1; r++)
            {
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

                        if (isCloseToVisible)
                        {
                            training[i] = outputs[i];
                        }
                        else
                        {
                            training[i] = 0;
                        }
                    }

                    error += Math.Pow(outputs[i] - training[i], 2);
                }

                if (MinesweeperState.GameResult == GameResult.Loose)
                {
                    training[MinesweeperState.SizeY * MinesweeperState.LastMove.y + MinesweeperState.LastMove.x] = 0;
                    learningRate = .2d;
                    LooseCount++;
                }
                else if (MinesweeperState.GameResult == GameResult.Win)
                {
                    WinCount++;
                }

                if (!MinesweeperState.IsRunning)
                {
                    if (LooseCount > 1000)
                    {
                        WinCount = 1;
                        LooseCount = 1;
                    }

                    Debug.WriteLine("win/loose: " + WinCount + " - " + LooseCount + ", rate: " + (double)WinCount / LooseCount);
                }

                //Debug.WriteLine("error: " + error);
                NeuralNetwork.ApplyBackpropagation(training, learningRate);
                learningRate = .02d;
            }
        }

        public (int y, int x) Predict()
        {
            var inputs = GetInputsFromState(MinesweeperState);
            var outputs = NeuralNetwork.ForwardPass(inputs);

            var maxIndex = 0;
            double maxValue = 0;

            for (int i = 0; i < outputs.Length; i++)
            {
                if (outputs[i] > maxValue)
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
                        inputs[i + size * n] = minesweeperState.NumbersMap[y][x];
                    }
                }
            }

            return inputs;
        }
    }
}
