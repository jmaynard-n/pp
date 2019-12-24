﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.IO;

namespace ClassMatrix
{
    public static class Constants
    {
        public const double DoubleComparisonDelta = 0.01;
    }
    public class Matrix
    {
        private double[,] data;
        public double[,] minor;
        //private double precalculatedDeterminant = double.NaN;

        private int m;
        public int M { get => this.m; }

        private int n;
        public int N { get => this.n; }

        public bool IsSquare { get => this.M == this.N; }

        public void ProcessFunctionOverData(Action<int, int> func)
        {
            for (var i = 0; i < this.M; i++)
            {
                for (var j = 0; j < this.N; j++)
                {
                    func(i, j);
                }
            }
        }

        public void Print()
        {
            for (var i = 0; i < this.N; i++)
            {
                for (var j = 0; j < this.M; j++)
                    Console.Write("[{0:f2}] ", this[i, j]);
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public int CheckIdentity()
        {
            if (this.N != this.M)
            {
                Console.WriteLine("This matrix is not square!");
                return 0;
            }

            for (var i = 0; i < this.N; i++)
            {
                for (var j = 0; j < this.M; j++)
                {
                    if (i == j && this[i, j] - 1 > Constants.DoubleComparisonDelta)
                    {
                        Console.WriteLine("There is {0} on diagonal, instead of 1!", this[i, j]);
                        return 0;
                    }
                    else if (i != j && this[i, j] > Constants.DoubleComparisonDelta)
                    {
                        Console.WriteLine("There is {0} outside of diagonal, instead of zero!", this[i, j]);
                        return 0;
                    }
                }
            }
            return 1;
        }

        public Matrix CreateTransposeMatrix()
        {
            var result = new Matrix(this.N, this.M);
            result.ProcessFunctionOverData((i, j) => result[i, j] = this[j, i]);
            return result;
        }

        public Matrix(int m, int n)
        {
            this.m = m;
            this.n = n;
            this.data = new double[m, n];
            this.minor = new double[m, n];
            Random rnd = new Random();

            this.ProcessFunctionOverData((i, j) => this.data[i, j] = rnd.Next(1, 10)); //rnd.NextDouble() + rnd.Next(0, 10));
            this.ProcessFunctionOverData((i, j) => this.minor[i, j] = 0);
        }

        public Matrix(string path)
        {
            string str;
            var i = 0;
            var j = 0;
            int size;
            int num;
                       
            using (StreamReader sr = new StreamReader(path))
            {
                size = Convert.ToInt32(sr.ReadLine());
                this.n = size;
                this.m = size;
                this.data = new double[n, m];
                this.minor = new double[n, m];
                this.ProcessFunctionOverData((k, l) => this.minor[k, l] = 0);

                while ((str = sr.ReadLine()) != null)
                {
                    string[] split = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in split)
                    {
                        num = Convert.ToInt32(s);
                        this.data[i, j] = num;
                        j++;
                    }
                    i++;
                    j = 0;
                }
            }
        }

        public double this[int x, int y]
        {
            get
            {
                return this.data[x, y];
            }
            set
            {
                this.data[x, y] = value;
            }
        }

        private Matrix SwapRowsWith0Pivot()
        {
            Matrix result = new Matrix(this.N, this.N);
            Boolean singular = false;
            int n = this.N;
            var i = 0;

            result.ProcessFunctionOverData((i, j) => result[i, j] = this[i, j]);
            while (i < n && !singular)
            {
                if (result[i, i] == 0)
                {
                    var j = 0;
                    while (j < n - 1 && result[j, i] == 0)
                        j++;
                    if (result[j, i] != 0)
                    {
                        for (var k = 0; k < n; k++)
                        {
                            double temp = result[i, k];
                            result[i, k] = result[j, k];
                            result[j, k] = temp;
                        }
                    }
                    else
                        singular = true;
                }
                i++;
            }
            if (singular)
                return null;
            return result;
        }

        public double CalculateDeterminant()
        {
            double pivot = 1;
            double det;
            Matrix result = new Matrix(this.N, this.N);

            if (!this.IsSquare)
            {
                throw new InvalidOperationException("determinant can be calculated only for square matrix");
            }
            //result = this.SwapRowsWith0Pivot();
            //if (result == null)
            //    return 0;
            //this.Print();
            //result.Print();
            result.ProcessFunctionOverData((i, j) => result[i, j] = this[i, j]);

            for (var k = 0; k < result.N - 1; k++)
            {
                for (var i = k + 1; i < result.N; i++)
                {
                    for (var j = k + 1; j < result.N; j++)
                    {
                        result[i, j] = result[k, k] * result[i, j] - result[i, k] * result[k, j];
                        result[i, j] = result[i, j] / pivot;
                    }
                }
                pivot = result[k, k];
            }
            det = result[result.N - 1, result.N - 1];
            return det;
        }

        public Matrix CreateInvertibleMatrix()
        {
            if (this.M != this.N)
                return null;
            var determinant = CalculateDeterminant();
            if (Math.Abs(determinant) < Constants.DoubleComparisonDelta)
                return null;

            var result = new Matrix(M, M);
            ProcessFunctionOverData((i, j) =>
            {
                result[i, j] = (((i + j) % 2 == 1 ? -1 : 1) * this.minor[i, j] / determinant);
            });

            result = result.CreateTransposeMatrix();
            return result;
        }

        //private double CalculateMinor(int i, int j)
        //{
        //    minor[i, j] = CreateMatrixWithoutColumn(j).CreateMatrixWithoutRow(i).CalculateDeterminant();
        //    return minor[i, j];
        //}

        public double Minor(int val)
        {
            System.Diagnostics.Stopwatch clocks = new System.Diagnostics.Stopwatch();
            TimeSpan ts;
            string elapsedTime;
            int i = val / 100;
            int j = val % 100;

            //clocks.Start();
           // Console.WriteLine("Task No {1} [{2}, {3}]", val, i, j);
            double minor = this.CreateMatrixWithoutColumn(j).CreateMatrixWithoutRow(i).CalculateDeterminant();
            //clocks.Stop();
           // ts = clocks.Elapsed;
            //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //Console.WriteLine("Task No [{0}, {1}] finished with runtime " + elapsedTime, i, j);
            return minor;
        }

        public Matrix CreateMatrixWithoutRow(int row)
        {
            if (row < 0 || row >= this.M)
            {
                throw new ArgumentException("invalid row index");
            }
            var result = new Matrix(this.M - 1, this.N);
            result.ProcessFunctionOverData((i, j) => result[i, j] = i < row ? this[i, j] : this[i + 1, j]);
            return result;
        }

        public Matrix CreateMatrixWithoutColumn(int column)
        {
            if (column < 0 || column >= this.N)
            {
                Console.WriteLine("exeption {0}", column);
                throw new ArgumentException("invalid column index");
            }
            var result = new Matrix(this.M, this.N - 1);
            result.ProcessFunctionOverData((i, j) => result[i, j] = j < column ? this[i, j] : this[i, j + 1]);
            return result;
        }

        public static Matrix operator *(Matrix matrix, int value)
        {
            var result = new Matrix(matrix.M, matrix.N);
            result.ProcessFunctionOverData((i, j) => result[i, j] = matrix[i, j] * value);
            return result;
        }

        public static Matrix operator *(Matrix matrix, Matrix matrix2)
        {
            if (matrix.N != matrix2.M)
            {
                throw new ArgumentException("matrixes can not be multiplied");
            }
            var result = new Matrix(matrix.M, matrix2.N);
            result.ProcessFunctionOverData((i, j) => result.data[i, j] = 0);
            result.ProcessFunctionOverData((i, j) =>
            {
                for (var k = 0; k < matrix.N; k++)
                {
                    result[i, j] += matrix[i, k] * matrix2[k, j];
                }
            });
            return result;
        }

        public static Matrix operator +(Matrix matrix, Matrix matrix2)
        {
            if (matrix.M != matrix2.M || matrix.N != matrix2.N)
            {
                throw new ArgumentException("matrixes dimensions should be equal");
            }
            var result = new Matrix(matrix.M, matrix.N);
            result.ProcessFunctionOverData((i, j) => result[i, j] = matrix[i, j] + matrix2[i, j]);
            return result;
        }

        public static Matrix operator -(Matrix matrix, Matrix matrix2)
        {
            return matrix + (matrix2 * -1);
        }

        public void ToFile()
        {
            string writePath = @"Matrix.txt";
            using (StreamWriter sw = new StreamWriter(writePath))
            {
                sw.WriteLine(Convert.ToString(this.N));
                for (var i = 0; i < this.N; i++)
                {
                    for (var j = 0; j < this.M; j++)
                        sw.Write("{0:0.00} ", Convert.ToString(this[i, j]));
                    sw.WriteLine();
                }
            }
            Console.WriteLine("Matrix is written to the file {0}", writePath);
        }
    }
}
