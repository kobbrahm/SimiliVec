
using System;
using System.Collections.Generic;

namespace VectorDataBase.Core;

public static class HNSWUtils
{
    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// </summary>
    /// <param name="vectorA"></param>
    /// <param name="vectorB"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of the same length");

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;
        
        // Calculate dot product and magnitudes
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        // Avoid division by zero
        if (magnitudeA == 0 || magnitudeB == 0)
            return 0f;

        // Calculate and return cosine similarity
        return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }


    /// <summary>
    /// Get random level for a new node based on inverseLogM
    /// </summary>
    /// <param name="inverseLogM"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public static int GetRandomLevel(float inverseLogM, Random random)
    {
        // Using negative logarithm to determine level
        return (int)(-Math.Log(random.NextDouble()) * inverseLogM);
    }

    
}