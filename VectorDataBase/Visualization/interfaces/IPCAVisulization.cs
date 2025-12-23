using System;
using System.Collections.Generic;
public interface IPCAVisualization
{
    /// <summary>
    /// Fits the PCA model to the provided data
    /// </summary>
    /// <param name="data"></param>
   void Fit(IEnumerable<float[]> data);
   /// <summary>
   /// Projects a set of vectors into 3D or 2D space for visualization
   /// </summary>
   /// <param name="vectors"></param>
   /// <returns></returns>
   VisualizationPoint[] Project (IEnumerable<float[]> vectors);
   /// <summary>
   /// Projects a single vector into 3D or 2D space for visualization
   /// </summary>
   /// <param name="vector"></param>
   /// <returns></returns>
   VisualizationPoint ProjectSingle (float[] vector);
   /// <summary>
   /// Number of output dimensions (2 or 3)
   /// </summary>
   int OutputDimensions { get; }
   /// <summary>
   /// Variance explained by the selected components
   /// </summary>
   float VarianceExplained { get; }
   /// <summary>
   /// Indicates if the PCA model has been fitted
   /// </summary>
   bool IsFitted { get; }
}

public class VisualizationPoint
{
    public float X { get; set;} 
    public float Y { get; set; }
    public float Z { get; set; }
    public int NodeId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

}