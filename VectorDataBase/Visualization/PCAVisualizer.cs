using System;
using System.Collections.Generic;
using System.Linq;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Core;

/// <summary>
/// PCA Visualizer for projecting high-dimensional vectors to 2D/3D for visualization
/// </summary>
public class PCAVisualizer : IPCAVisualization
{
    // === Learned Parameters ===
    private float[] _featureMeans;
    private float[,] _principalComponents;
    private float[] _eigenvalues;
    private int _inputDimensions;

    // === Configuration ===
    public int OutputDimensions { get; }
    public float VarianceExplained { get; private set; }
    public bool IsFitted { get; private set; }

    /// <summary>
    /// Number of power iterations for eigenvalue computation
    /// </summary>
    public int PowerIterations { get; init; } = 100;

    /// <summary>
    /// Random seed for reproducibility
    /// </summary>
    public int RandomSeed { get; init; } = 42;

    /// <summary>
    /// Creates a PCA visualizer
    /// </summary>
    /// <param name="dimensions">2 for 2D scatter plot, 3 for 3D visualization</param>
    public PCAVisualizer(int dimensions = 2)
    {
        if (dimensions < 2 || dimensions > 3)
            throw new ArgumentException("Output dimensions must be 2 or 3 for visualization", nameof(dimensions));

        OutputDimensions = dimensions;
        IsFitted = false;
    }

    /// <summary>
    /// Fits PCA to the dataset.  Call this once before projecting.
    /// </summary>
    /// <param name="vectors">Vectors from your database to fit on</param>
    public void Fit(IEnumerable<float[]> vectors)
    {
        // Convert to array for multiple passes
        float[][] data = vectors.ToArray();
        ValidateData(data);

        _inputDimensions = data[0].Length;

        // Step 1: Compute means for centering
        _featureMeans = ComputeMeans(data);

        // Step 2: Center the data
        float[][] centered = CenterData(data);

        // Step 3: Compute covariance matrix
        float[,] covariance = ComputeCovarianceMatrix(centered);

        // Step 4: Find top eigenvectors (principal components)
        (_eigenvalues, _principalComponents) = ComputeTopEigenpairs(covariance, OutputDimensions);

        // Step 5: Calculate variance explained
        VarianceExplained = CalculateVarianceExplained(covariance);

        IsFitted = true;
    }

    /// <summary>
    /// Projects all vectors to visualization coordinates. 
    /// Use this to generate the initial scatter plot data.
    /// </summary>
    /// <param name="vectors">Vectors to project (typically all nodes in your index)</param>
    /// <returns>Array of visualization points ready for the frontend</returns>
    public VisualizationPoint[] Project(IEnumerable<float[]> vectors)
    {
        EnsureFitted();

        return vectors. Select(v => ProjectSingle(v)).ToArray();
    }

    /// <summary>
    /// Projects a single vector.  Useful for: 
    /// - Highlighting a query point on the visualization
    /// - Adding a new point without re-fitting
    /// </summary>
    public VisualizationPoint ProjectSingle(float[] vector)
    {
        EnsureFitted();

        if (vector.Length != _inputDimensions)
            throw new ArgumentException($"Vector dimension {vector.Length} doesn't match fitted dimension {_inputDimensions}");

        // Center the vector
        float[] centered = new float[_inputDimensions];
        for (int i = 0; i < _inputDimensions; i++)
        {
            centered[i] = vector[i] - _featureMeans[i];
        }

        // Project onto principal components
        float[] projected = new float[OutputDimensions];
        for (int j = 0; j < OutputDimensions; j++)
        {
            float sum = 0f;
            for (int i = 0; i < _inputDimensions; i++)
            {
                sum += centered[i] * _principalComponents[i, j];
            }
            projected[j] = sum;
        }

        return new VisualizationPoint
        {
            X = projected[0],
            Y = projected[1],
            Z = OutputDimensions == 3 ? projected[2] : 0f
        };
    }

    private void EnsureFitted()
    {
        if (!IsFitted)
            throw new InvalidOperationException("PCA must be fitted before projecting.  Call Fit() first.");
    }

    private void ValidateData(float[][] data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty");

        if (data.Length < OutputDimensions)
            throw new ArgumentException($"Need at least {OutputDimensions} samples for {OutputDimensions}D projection");

        int dim = data[0]. Length;
        if (dim < OutputDimensions)
            throw new ArgumentException($"Vector dimension {dim} is less than output dimension {OutputDimensions}");
    }

    #region PCA Math Implementation

    private float[] ComputeMeans(float[][] data)
    {
        int numFeatures = data[0].Length;
        float[] means = new float[numFeatures];

        foreach (var vector in data)
        {
            for (int j = 0; j < numFeatures; j++)
                means[j] += vector[j];
        }

        for (int j = 0; j < numFeatures; j++)
            means[j] /= data. Length;

        return means;
    }

    private float[][] CenterData(float[][] data)
    {
        int numFeatures = data[0].Length;
        float[][] centered = new float[data.Length][];

        for (int i = 0; i < data.Length; i++)
        {
            centered[i] = new float[numFeatures];
            for (int j = 0; j < numFeatures; j++)
            {
                centered[i][j] = data[i][j] - _featureMeans[j];
            }
        }

        return centered;
    }

    private float[,] ComputeCovarianceMatrix(float[][] centered)
    {
        int n = centered.Length;
        int d = centered[0]. Length;
        float[,] cov = new float[d, d];

        for (int i = 0; i < d; i++)
        {
            for (int j = i; j < d; j++)
            {
                float sum = 0f;
                for (int k = 0; k < n; k++)
                    sum += centered[k][i] * centered[k][j];

                float value = sum / (n - 1);
                cov[i, j] = value;
                cov[j, i] = value;
            }
        }

        return cov;
    }

    private (float[], float[,]) ComputeTopEigenpairs(float[,] matrix, int k)
    {
        int n = matrix.GetLength(0);
        float[] eigenvalues = new float[k];
        float[,] eigenvectors = new float[n, k];
        float[,] work = (float[,])matrix.Clone();
        Random random = new Random(RandomSeed);

        for (int ev = 0; ev < k; ev++)
        {
            float[] v = new float[n];
            for (int i = 0; i < n; i++)
                v[i] = (float)(random.NextDouble() - 0.5);

            // Power iteration
            for (int iter = 0; iter < PowerIterations; iter++)
            {
                float[] newV = new float[n];
                for (int i = 0; i < n; i++)
                {
                    float sum = 0f;
                    for (int j = 0; j < n; j++)
                        sum += work[i, j] * v[j];
                    newV[i] = sum;
                }

                float norm = 0f;
                foreach (float val in newV)
                    norm += val * val;
                norm = (float)Math.Sqrt(norm);

                if (norm < 1e-10f) break;

                for (int i = 0; i < n; i++)
                    v[i] = newV[i] / norm;
            }

            // Compute eigenvalue
            float[] Av = new float[n];
            for (int i = 0; i < n; i++)
            {
                float sum = 0f;
                for (int j = 0; j < n; j++)
                    sum += work[i, j] * v[j];
                Av[i] = sum;
            }

            float eigenvalue = 0f;
            for (int i = 0; i < n; i++)
                eigenvalue += v[i] * Av[i];

            eigenvalues[ev] = eigenvalue;
            for (int i = 0; i < n; i++)
                eigenvectors[i, ev] = v[i];

            // Deflate
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    work[i, j] -= eigenvalue * v[i] * v[j];
        }

        return (eigenvalues, eigenvectors);
    }

    private float CalculateVarianceExplained(float[,] covariance)
    {
        float total = 0f;
        for (int i = 0; i < covariance.GetLength(0); i++)
            total += covariance[i, i];

        float selected = _eigenvalues.Sum();

        return total > 0 ?  selected / total : 0f;
    }

    #endregion
}