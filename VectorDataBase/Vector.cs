using System;
using System.Collections.Generic;

/// <summary>
/// Represents a vector with its embedded values and original text
/// </summary>
public class Vector
{
    public float[] EmbeddedValues { get; set; }
    public string OriginalText { get; set; }

    public Vector(float[] embeddedValues, string originalText)
    {
        EmbeddedValues = embeddedValues;
        OriginalText = originalText;
    }
}