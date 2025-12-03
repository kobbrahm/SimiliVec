using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ML.OnnxRuntime;
using VectorDataBase.Embedding;
using VectorDataBase.Datahandling;
using VectorDataBase.Core;
using VectorDataBase.Services;
using VectorDataBase.App;
using Microsoft.Extensions.DependencyInjection;

namespace VectorDataBase.App;

internal class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddVectorDataBaseServices();

        // Build provider and run
        using var provider = services.BuildServiceProvider();
        var front = provider.GetRequiredService<FakeFrontEnd>();
        front.GetAnswers();
    }
}

