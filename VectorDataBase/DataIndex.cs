using System;
using System.Collections.Generic;

/// <summary>
/// Class to hold indexed vectors
/// </summary>
public class DataIndex
{
    public List<Vector> Vectors { get; set; }
    public DataIndex()
    {
        Vectors = new List<Vector>();
    }
}