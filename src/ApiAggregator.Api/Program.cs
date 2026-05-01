using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.ExternalApis;
using ApiAggregator.Infrastructure.ExternalApis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAggregationService, AggregationService>();

builder.Services.AddScoped<IExternalApiProvider, MockGitHubProvider>();
builder.Services.AddScoped<IExternalApiProvider, MockWeatherProvider>();
builder.Services.AddScoped<IExternalApiProvider, MockHackerNewsProvider>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();