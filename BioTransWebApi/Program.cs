using BioTrananDomain.Models;
using BioTransWebApi;
using BioTransWebApi.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// MongoDb Services
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB")); // key from appsettings.json

builder.Services.AddSingleton<MongoClient>(provider =>
    new MongoClient(provider.GetService<IOptions<MongoDBSettings>>()!.Value.ConnectionURI));

// Generic mongoDbService injections
SetupClass.MongoDbInjections(builder);

// Model services
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<SalonService>();
builder.Services.AddScoped<ShowingService>();
builder.Services.AddScoped<ReservationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();