using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedGaussianMethodLES
{
    public class GaussianSolverDistributed : GaussianSolver 
    {
        private ThreadPool<(int, int)> pool;

        private int n;
        private double[,] matrix;

        public GaussianSolverDistributed(int threads)
        {
            pool = new ThreadPool<(int, int)>(threads);
        }

        public override double[] Solve(double[,] matrix)
        {
            this.matrix = matrix;
            n = matrix.GetLength(0);
            if (n + 1 != matrix.GetLength(1))
            {
                throw new ArgumentException("Matrix is not a augmented");
            }

            for (int i = 0; i < n; i++)
            {
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > Math.Abs(matrix[maxRow, i]))
                    {
                        maxRow = k;
                    }
                }

                for (int k = i; k < n + 1; k++)
                {
                    double temp = matrix[i, k];
                    matrix[i, k] = matrix[maxRow, k];
                    matrix[maxRow, k] = temp;
                } 

                for (int k = i + 1; k < n; k++)
                {
                    pool.AddTask(ForwardHandle, (i, k));
                }

                while (pool.WorkingNow) ;
            }

            for (int i = n - 1; i >= 0; i--)
            {
                matrix[i, n] /= matrix[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    matrix[k, n] -= matrix[k, i] * matrix[i, n];
                }
            }

            double[] result = new double[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = matrix[i, n];
            }

            pool.EndPool();
            return result;
        }

        private void ForwardHandle((int, int) tuple)
        {
            int leading = tuple.Item1;
            int computing = tuple.Item2;
            double factor = matrix[computing, leading] / matrix[leading, leading];
            for (int j = leading; j < n + 1; j++)
            {
                matrix[computing, j] -= factor * matrix[leading, j];
            }
        }
    }
}
