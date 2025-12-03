using System;
using System.Collections.Generic;

namespace VectorDataBase.Core;

public class HsnwNode
{
    public int id { get; set; }
    public float[] Vector { get; init; } = Array.Empty<float>();
    public int level { get; set; }
    public List<int>[] Neighbors { get; set; } = Array.Empty<List<int>>();

}