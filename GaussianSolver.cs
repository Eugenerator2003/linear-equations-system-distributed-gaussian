using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedGaussianMethodLES
{
    public class GaussianSolver
    {
        public virtual double[] Solve(double[,] matrix)
        {
            int n = matrix.GetLength(0);
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
                    double factor = matrix[k, i] / matrix[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                    }
                }
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

            return result;
        }
    }
}
