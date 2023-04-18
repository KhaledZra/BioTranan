using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BioTransWebApi;

// This class helps minimize cluster on Program.cs
public static class SetupClass
{
    public static void MongoDbInjections(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<MongoDBService<Movie>>(provider => 
            new MongoDBService<Movie>(
                mongoClient: provider.GetService<MongoClient>()!,
                databaseName: provider.GetService<IOptions<MongoDBSettings>>()!.Value.DatabaseName,
                collectionName: "movies"));

        builder.Services.AddScoped<MongoDBService<Salon>>(provider => 
            new MongoDBService<Salon>(
                mongoClient: provider.GetService<MongoClient>()!,
                databaseName: provider.GetService<IOptions<MongoDBSettings>>()!.Value.DatabaseName,
                collectionName: "Salons"));

        builder.Services.AddScoped<MongoDBService<Showing>>(provider => 
            new MongoDBService<Showing>(
                mongoClient: provider.GetService<MongoClient>()!,
                databaseName: provider.GetService<IOptions<MongoDBSettings>>()!.Value.DatabaseName,
                collectionName: "Showings"));

        builder.Services.AddScoped<MongoDBService<Reservation>>(provider => 
            new MongoDBService<Reservation>(
                mongoClient: provider.GetService<MongoClient>()!,
                databaseName: provider.GetService<IOptions<MongoDBSettings>>()!.Value.DatabaseName,
                collectionName: "Reservations"));
    }
}