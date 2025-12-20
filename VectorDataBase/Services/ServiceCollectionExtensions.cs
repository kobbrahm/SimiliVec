using Microsoft.Extensions.DependencyInjection;
using VectorDataBase.Interfaces;
using VectorDataBase.Datahandling;
using VectorDataBase.Embedding;
using VectorDataBase.Core;
using VectorDataBase;

namespace VectorDataBase.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVectorDataBaseServices(this IServiceCollection services)
    {
        // Core/index - register configured DataIndex instance
        services.AddSingleton<IDataIndex>(sp => new DataIndex
        {
            MaxNeighbours = 4,
            EfConstruction = 20,
            InverseLogM = 1.0f / 1.5f
        });

        // Embedding and tokenizer
        services.AddSingleton<IEmbeddingModel, EmbeddingModel>();

        // Data loading
        services.AddSingleton<IDataLoader, DataLoader>();

        // VectorService holds state and coordinates index/embeddings
        services.AddSingleton<IVectorService, VectorService>();

        // FakeFrontEnd is a placeholder application component used for manual testing.
        // Keep registered as transient for now; replace or remove when integrating the real frontend.
        //services.AddTransient<FakeFrontEnd>();

        return services;
    }
}
