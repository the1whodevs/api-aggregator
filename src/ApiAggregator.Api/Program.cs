using System.Text;
using ApiAggregator.Api.Auth;
using ApiAggregator.Api.Performance;
using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.Caching;
using ApiAggregator.Application.ExternalApis;
using ApiAggregator.Application.Statistics;
using ApiAggregator.Infrastructure.Caching;
using ApiAggregator.Infrastructure.ExternalApis;
using ApiAggregator.Infrastructure.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt settings are missing.");

jwtOptions.Validate();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "Paste only the JWT access token. Swagger sends it as a Bearer token.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<PerformanceAnomalyOptions>(
    builder.Configuration.GetSection(PerformanceAnomalyOptions.SectionName));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddHostedService<PerformanceAnomalyBackgroundService>();

// Register each provider against the same abstraction. AggregationService receives
// all of them through IEnumerable<IExternalApiProvider> and runs them together.
builder.Services.AddHttpClient<IExternalApiProvider, GitHubApiProvider>();
builder.Services.AddHttpClient<IExternalApiProvider, OpenMeteoApiProvider>();
builder.Services.AddHttpClient<IExternalApiProvider, HackerNewsApiProvider>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IExternalApiCache, MemoryExternalApiCache>(); 
builder.Services.AddSingleton<IRequestStatisticsStore, InMemoryRequestStatisticsStore>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
