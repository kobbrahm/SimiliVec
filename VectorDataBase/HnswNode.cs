using System;
using System.Collections.Generic;

public class HsnwNode
{
    public int id { get; set; }
    public float[] Vector { get; init; }
    public int level { get; set; }
    public List<int>[] Neighbors { get; set; } = Array.Empty<List<int>>();

}