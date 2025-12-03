using System;
using System.Collections.Generic;

namespace VectorDataBase.Datahandling;

public record VectorRecord(int id,
Dictionary<string, string> Metadata,
string OriginalText
);