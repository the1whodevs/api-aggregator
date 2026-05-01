using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.Caching;
using ApiAggregator.Application.ExternalApis;
using ApiAggregator.Application.Statistics;
using ApiAggregator.Infrastructure.Caching;
using ApiAggregator.Infrastructure.ExternalApis;
using ApiAggregator.Infrastructure.Statistics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAggregationService, AggregationService>();

builder.Services.AddHttpClient<IExternalApiProvider, GitHubApiProvider>();
builder.Services.AddScoped<IExternalApiProvider, MockWeatherProvider>();
builder.Services.AddScoped<IExternalApiProvider, MockHackerNewsProvider>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IExternalApiCache, MemoryExternalApiCache>(); 
builder.Services.AddSingleton<IRequestStatisticsStore, InMemoryRequestStatisticsStore>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();