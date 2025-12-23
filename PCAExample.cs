using System;

namespace PCAExample
{
    /// <summary>
    /// Principal Component Analysis (PCA) Implementation
    /// PCA is a dimensionality reduction technique that transforms data into a new coordinate system
    /// where the greatest variance lies on the first principal component, the second greatest on the second, etc.
    /// </summary>
    public class PCA
    {
        /// <summary>
        /// Performs PCA on the input data matrix
        /// </summary>
        /// <param name="data">Input data matrix where rows are samples and columns are features</param>
        /// <param name="numComponents">Number of principal components to keep</param>
        /// <returns>Transformed data with reduced dimensions</returns>
        /// HÄR SKULLE DET TAS IN RENA VECTORER IFRÅN INDEX.NODES OCH RETURNERA RENA VECTORER
        public static double[,] PerformPCA(double[,] data, int numComponents)
        {
            int numSamples = data.GetLength(0);  // Number of data points (rows)
            int numFeatures = data. GetLength(1); // Number of features (columns)

            // Step 1: Standardize the data (center by subtracting the mean)
            // This ensures each feature has zero mean, which is essential for PCA
            double[,] centeredData = CenterData(data);
            Console.WriteLine("Step 1: Data centered (mean subtracted from each feature)");

            // Step 2:  Compute the covariance matrix
            // The covariance matrix captures how features vary together
            // High covariance = features change together; Low = they're independent
            double[,] covarianceMatrix = ComputeCovarianceMatrix(centeredData);
            Console. WriteLine("Step 2: Covariance matrix computed");
            PrintMatrix(covarianceMatrix, "Covariance Matrix");

            // Step 3: Compute eigenvalues and eigenvectors of the covariance matrix
            // Eigenvectors point in the directions of maximum variance (principal components)
            // Eigenvalues indicate the amount of variance in each direction
            (double[] eigenvalues, double[,] eigenvectors) = ComputeEigen(covarianceMatrix);
            Console. WriteLine("Step 3: Eigenvalues and eigenvectors computed");
            
            PrintArray(eigenvalues, "Eigenvalues (variance explained by each component)");

            // Step 4: Sort eigenvectors by eigenvalues in descending order
            // We want the components that explain the most variance first
            SortEigenByValue(eigenvalues, eigenvectors);
            Console.WriteLine("Step 4: Eigenvectors sorted by eigenvalue (descending)");

            // Step 5: Select top k eigenvectors (principal components)
            // This is where dimensionality reduction happens
            double[,] selectedComponents = SelectTopComponents(eigenvectors, numComponents);
            Console. WriteLine($"Step 5: Selected top {numComponents} principal components");

            // Step 6: Transform the original data to the new subspace
            // Project data onto the principal components
            double[,] transformedData = TransformData(centeredData, selectedComponents);
            Console. WriteLine("Step 6: Data transformed to new coordinate system");

            // Calculate and display variance explained
            double totalVariance = 0;
            double selectedVariance = 0;
            for (int i = 0; i < eigenvalues.Length; i++)
                totalVariance += eigenvalues[i];
            for (int i = 0; i < numComponents; i++)
                selectedVariance += eigenvalues[i];
            
            Console.WriteLine($"\nVariance explained by {numComponents} components:  {(selectedVariance / totalVariance * 100):F2}%");

            return transformedData;
        }

        /// <summary>
        /// Centers the data by subtracting the mean of each feature (column)
        /// </summary>
        private static double[,] CenterData(double[,] data)
        {
            int rows = data.GetLength(0);
            int cols = data. GetLength(1);
            double[,] centered = new double[rows, cols];

            // For each feature (column), calculate mean and subtract it
            for (int j = 0; j < cols; j++)
            {
                // Calculate mean for this feature
                double mean = 0;
                for (int i = 0; i < rows; i++)
                    mean += data[i, j];
                mean /= rows;

                // Subtract mean from each value in this feature
                for (int i = 0; i < rows; i++)
                    centered[i, j] = data[i, j] - mean;
            }

            return centered;
        }

        /// <summary>
        /// Computes the covariance matrix:  Cov(X) = (1/(n-1)) * X^T * X
        /// where X is the centered data matrix
        /// </summary>
        private static double[,] ComputeCovarianceMatrix(double[,] centeredData)
        {
            int rows = centeredData. GetLength(0);
            int cols = centeredData.GetLength(1);
            double[,] covariance = new double[cols, cols];

            // Covariance between feature i and feature j
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < rows; k++)
                    {
                        sum += centeredData[k, i] * centeredData[k, j];
                    }
                    // Divide by (n-1) for sample covariance (Bessel's correction)
                    covariance[i, j] = sum / (rows - 1);
                }
            }

            return covariance;
        }

        /// <summary>
        /// Computes eigenvalues and eigenvectors using the Power Iteration method
        /// Note: For production use, consider using a math library like Math.NET Numerics
        /// This is a simplified implementation for educational purposes
        /// </summary>
        private static (double[], double[,]) ComputeEigen(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[] eigenvalues = new double[n];
            double[,] eigenvectors = new double[n, n];
            double[,] workMatrix = (double[,])matrix.Clone();

            // Find each eigenvalue/eigenvector pair using power iteration with deflation
            for (int ev = 0; ev < n; ev++)
            {
                // Initialize random vector
                double[] vector = new double[n];
                Random rand = new Random(42 + ev);
                for (int i = 0; i < n; i++)
                    vector[i] = rand.NextDouble();

                // Power iteration: repeatedly multiply by matrix and normalize
                // This converges to the dominant eigenvector
                for (int iter = 0; iter < 100; iter++)
                {
                    double[] newVector = MultiplyMatrixVector(workMatrix, vector);
                    double norm = VectorNorm(newVector);
                    
                    for (int i = 0; i < n; i++)
                        vector[i] = newVector[i] / norm;
                }

                // Compute eigenvalue using Rayleigh quotient:  λ = (v^T * A * v) / (v^T * v)
                double[] Av = MultiplyMatrixVector(workMatrix, vector);
                double eigenvalue = DotProduct(vector, Av);
                
                eigenvalues[ev] = eigenvalue;
                for (int i = 0; i < n; i++)
                    eigenvectors[i, ev] = vector[i];

                // Deflate matrix:  A = A - λ * v * v^T
                // This removes the found component so we can find the next one
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        workMatrix[i, j] -= eigenvalue * vector[i] * vector[j];
            }

            return (eigenvalues, eigenvectors);
        }

        /// <summary>
        /// Sorts eigenvectors by their corresponding eigenvalues in descending order
        /// </summary>
        private static void SortEigenByValue(double[] eigenvalues, double[,] eigenvectors)
        {
            int n = eigenvalues.Length;
            
            // Simple bubble sort (sufficient for small matrices)
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (eigenvalues[j] < eigenvalues[j + 1])
                    {
                        // Swap eigenvalues
                        (eigenvalues[j], eigenvalues[j + 1]) = (eigenvalues[j + 1], eigenvalues[j]);
                        
                        // Swap corresponding eigenvector columns
                        for (int k = 0; k < n; k++)
                        {
                            (eigenvectors[k, j], eigenvectors[k, j + 1]) = 
                                (eigenvectors[k, j + 1], eigenvectors[k, j]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Selects the top k eigenvectors (principal components)
        /// </summary>
        private static double[,] SelectTopComponents(double[,] eigenvectors, int k)
        {
            int n = eigenvectors. GetLength(0);
            double[,] selected = new double[n, k];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < k; j++)
                    selected[i, j] = eigenvectors[i, j];

            return selected;
        }

        /// <summary>
        /// Transforms the data by projecting it onto the principal components
        /// Result = CenteredData × PrincipalComponents
        /// </summary>
        private static double[,] TransformData(double[,] centeredData, double[,] components)
        {
            int numSamples = centeredData.GetLength(0);
            int numFeatures = centeredData.GetLength(1);
            int numComponents = components. GetLength(1);
            
            double[,] transformed = new double[numSamples, numComponents];

            // Matrix multiplication: each sample projected onto each component
            for (int i = 0; i < numSamples; i++)
            {
                for (int j = 0; j < numComponents; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < numFeatures; k++)
                    {
                        sum += centeredData[i, k] * components[k, j];
                    }
                    transformed[i, j] = sum;
                }
            }

            return transformed;
        }

        #region Helper Methods
        
        private static double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            double[] result = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                result[i] = 0;
                for (int j = 0; j < n; j++)
                    result[i] += matrix[i, j] * vector[j];
            }
            
            return result;
        }

        private static double VectorNorm(double[] vector)
        {
            double sum = 0;
            foreach (double v in vector)
                sum += v * v;
            return Math.Sqrt(sum);
        }

        private static double DotProduct(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        private static void PrintMatrix(double[,] matrix, string name)
        {
            Console.WriteLine($"\n{name}:");
            for (int i = 0; i < matrix. GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                    Console.Write($"{matrix[i, j],10: F4} ");
                Console.WriteLine();
            }
        }

        private static void PrintArray(double[] array, string name)
        {
            Console. WriteLine($"\n{name}:");
            foreach (double v in array)
                Console.Write($"{v,10:F4} ");
            Console.WriteLine();
        }
        
        #endregion
    }

    /// <summary>
    /// Example usage demonstrating PCA on sample data
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Principal Component Analysis (PCA) Demo ===\n");

            // Sample dataset:  6 samples with 4 features each
            // Imagine these as measurements:  height, weight, age, income
            double[,] data = new double[,]
            {
                { 2.5, 2.4, 3.5, 0.5 },
                { 0.5, 0.7, 1.0, 0.2 },
                { 2.2, 2.9, 3.0, 0.8 },
                { 1.9, 2.2, 2.8, 0.6 },
                { 3.1, 3.0, 4.2, 1.0 },
                { 2.3, 2.7, 3.3, 0.7 }
            };

            Console.WriteLine("Original Data (6 samples × 4 features):");
            for (int i = 0; i < data.GetLength(0); i++)
            {
                Console. Write($"Sample {i + 1}: ");
                for (int j = 0; j < data.GetLength(1); j++)
                    Console.Write($"{data[i, j],6:F2} ");
                Console.WriteLine();
            }
            Console.WriteLine("\n" + new string('-', 60) + "\n");

            // Reduce from 4 dimensions to 2 dimensions
            int numComponentsToKeep = 2;
            double[,] reducedData = PCA.PerformPCA(data, numComponentsToKeep);

            Console.WriteLine($"\n=== Transformed Data ({numComponentsToKeep} principal components) ===");
            for (int i = 0; i < reducedData.GetLength(0); i++)
            {
                Console.Write($"Sample {i + 1}: ");
                for (int j = 0; j < reducedData.GetLength(1); j++)
                    Console.Write($"{reducedData[i, j],10:F4} ");
                Console.WriteLine();
            }

            Console.WriteLine("\n=== Key Takeaways ===");
            Console.WriteLine("• Original data:  4 features per sample");
            Console.WriteLine("• Reduced data: 2 features per sample (dimensionality reduction!)");
            Console. WriteLine("• The new features (principal components) capture maximum variance");
            Console.WriteLine("• Useful for visualization, noise reduction, and faster ML training");
        }
    }
}