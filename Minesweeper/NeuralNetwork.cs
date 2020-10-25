using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    public class NeuralNetworkData
    {
        public double[][][] Weights { get; set; }

        public double[][] Bias { get; set; }
    }

    public class NeuralNetwork
    {
        public double[][][] Weights { get; set; }

        public double[][] Bias { get; set; }

        public double[][] Inputs { get; set; }

        public double[][] Outputs { get; set; }

        public Func<double, double>[] Functions { get; set; }

        public NeuralNetwork(
            int inputsAmount,
            int outputsAmount,
            int innerLayersAmount,
            int innerNodesAmount,
            Func<double, double> trasholdFunc)
        {
            Inputs = GetEmptyNodes(inputsAmount, outputsAmount, innerLayersAmount, innerNodesAmount);
            Outputs = GetEmptyNodes(inputsAmount, outputsAmount, innerLayersAmount, innerNodesAmount);
            Weights = GetInitWeights(inputsAmount, outputsAmount, innerLayersAmount, innerNodesAmount);
            Bias = GetInitBias(outputsAmount, innerLayersAmount, innerNodesAmount);
            Functions = GetFunctions(trasholdFunc, innerLayersAmount);
        }

        public double[] ForwardPass(double[] inputs)
        {
            for (int i = 0; i < Inputs[0].Length; i++)
            {
                Inputs[0][i] = inputs[i];
            }

            for (int i = 0; i < Inputs.Length; i++)
            {
                //apply function
                for (int j = 0; j < Inputs[i].Length; j++)
                {
                    Outputs[i][j] = Functions[i](Inputs[i][j]);
                }

                if (i >= Weights.Length) break;

                for (int j = 0; j < Inputs[i + 1].Length; j++)
                {
                    Inputs[i + 1][j] = 0;
                }

                for (int j = 0; j < Inputs[i].Length; j++)
                {
                    for (int k = 0; k < Weights[i][j].Length; k++)
                    {
                        Inputs[i + 1][k] += Outputs[i][j] * Weights[i][j][k];
                    }
                }

                for (int j = 0; j < Inputs[i + 1].Length; j++)
                {
                    Inputs[i + 1][j] += Bias[i][j];
                }
            }

            return Outputs[Outputs.Length - 1];
        }

        public void ApplyBackpropagation(double[] trainnedOutput, double learnRate)
        {
            var nodesDerivs = new double[Weights.Length][];
            nodesDerivs[^1] = new double[Outputs[^1].Length];

            for (int i = 0; i < trainnedOutput.Length; i++)
            {
                nodesDerivs[^1][i] = Outputs[^1][i] - trainnedOutput[i];
            }

            for (int i = Weights.Length - 2; i >= 0; i--)
            {
                nodesDerivs[i] = new double[Weights[i + 1].Length];

                for (int j = 0; j < Weights[i + 1].Length; j++)
                {
                    nodesDerivs[i][j] = 0;

                    for (int k = 0; k < nodesDerivs[i + 1].Length; k++)
                    {
                        var o = Outputs[i + 2][k] * (1 - Outputs[i + 2][k]);
                        var w = Weights[i + 1][j][k];

                        nodesDerivs[i][j] += nodesDerivs[i + 1][k] * o * w;
                    }
                }
            }

            for (int i = Weights.Length - 1; i >= 0; i--)
            {
                // weights update
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    for (int k = 0; k < Weights[i][j].Length; k++)
                    {
                        var test = nodesDerivs[i][k] * Outputs[i + 1][k] * (1 - Outputs[i + 1][k]) * Outputs[i][j];
                        Weights[i][j][k] -= learnRate * test;
                    }
                }


                // bias update
                for (int j = 0; j < Bias[i].Length; j++)
                {
                    Bias[i][j] -= nodesDerivs[i][j] * 1 * Outputs[i + 1][j] * (1 - Outputs[i + 1][j]);
                }
            }
        }

        Func<double, double>[] GetFunctions(Func<double, double> trasholdFunc, int innerLayersAmount)
        {
            var functions = new List<Func<double, double>>() { x => x };
            functions.AddRange(Enumerable.Repeat(trasholdFunc, innerLayersAmount + 1));

            return functions.ToArray();
        }

        double[][] GetEmptyNodes(
            int inputsAmount,
            int outputsAmount,
            int innerLayersAmount,
            int innerNodesAmount)
        {
            var nodes = new List<double[]>()
            {
                Enumerable.Repeat(0d, inputsAmount).ToArray()
            };

            nodes.AddRange(Enumerable
                   .Repeat(0, innerLayersAmount)
                   .Select(i => Enumerable.Repeat(0d, innerNodesAmount).ToArray())
                   .ToList());

            nodes.Add(Enumerable.Repeat(0d, outputsAmount).ToArray());

            return nodes.ToArray();
        }

        double[][] GetInitBias(
            int outputsAmount,
            int innerLayersAmount,
            int innerNodesAmount)
        {
            var randNum = new Random();

            var bias = Enumerable
                   .Repeat(0, innerLayersAmount)
                   .Select(i => Enumerable.Repeat(0d, innerNodesAmount).Select(_ => randNum.NextDouble()).ToArray())
                   .ToList();
            bias.Add(Enumerable.Repeat(0d, outputsAmount).Select(_ => randNum.NextDouble()).ToArray());

            return bias.ToArray();
        }

        double[][][] GetInitWeights(
            int inputsAmount,
            int outputsAmount,
            int innerLayersAmount,
            int innerNodesAmount)
        {
            var randNum = new Random();

            var weights = new List<double[][]>()
            {
                // input layer weights
                Enumerable
                .Repeat(0, inputsAmount)
                .Select(i => Enumerable
                    .Repeat(0, innerNodesAmount)
                    .Select(i => randNum.NextDouble())
                    .ToArray())
                .ToArray()
            };

            // inner layers weights
            weights.AddRange(Enumerable
               .Repeat(0, innerLayersAmount - 1)
               .Select(i => Enumerable
                   .Repeat(0, innerNodesAmount)
                   .Select(i => Enumerable
                       .Repeat(0, innerNodesAmount)
                       .Select(i => randNum.NextDouble())
                       .ToArray())
                   .ToArray())
               .ToList());

            // output layer weights
            weights.Add(Enumerable
                .Repeat(0, innerNodesAmount)
                .Select(i => Enumerable
                    .Repeat(0, outputsAmount)
                    .Select(i => randNum.NextDouble())
                    .ToArray())
                .ToArray());

            return weights.ToArray();
        }
    }
}