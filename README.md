# Api Aggregator

Api Aggregator is a .NET 8 Web API that combines results from multiple external APIs into one normalized response. It currently aggregates data from GitHub repositories, Hacker News top stories, and Open-Meteo weather data for Athens.

The application also records basic per-provider request statistics and exposes them through a separate endpoint.

## Solution Structure

```text
ApiAggregator.sln
src/
  ApiAggregator.Api/             ASP.NET Core Web API entry point and controllers
  ApiAggregator.Application/     Aggregation logic, DTOs, provider abstractions, statistics contracts
  ApiAggregator.Infrastructure/  External API providers, in-memory cache, in-memory statistics store
tests/
  ApiAggregator.Tests/           xUnit tests for aggregation and statistics behavior
```

## Requirements

- .NET 8 SDK
- Internet access when using the real external providers

## Running Locally

Restore and build the solution:

```powershell
dotnet restore ApiAggregator.sln
dotnet build ApiAggregator.sln
```

Run the API:

```powershell
dotnet run --project src/ApiAggregator.Api/ApiAggregator.Api.csproj
```

The development launch profile uses:

- HTTP: `http://localhost:5057`
- HTTPS: `https://localhost:7198`

Swagger UI is enabled at `/swagger`.

## API Endpoints

### Get Aggregated Data

```http
GET /api/aggregation
```

Query parameters:

| Name | Default | Description |
| --- | --- | --- |
| `query` | `null` | Text filter applied to item title and description. Also drives the GitHub repository search term. |
| `category` | `null` | Category filter. Current categories include `technology` and `weather`. |
| `sortBy` | `date` | Sort field. Supported values are `date`, `title`, and `relevance`. |
| `sortDirection` | `desc` | Sort direction. Use `asc` or `desc`. |

Example:

```powershell
Invoke-RestMethod "http://localhost:5057/api/aggregation?query=csharp&category=technology&sortBy=relevance&sortDirection=desc"
```

Response shape:

```json
{
  "items": [
    {
      "source": "Github",
      "title": "dotnet/runtime",
      "description": "The .NET runtime",
      "category": "technology",
      "url": "https://github.com/dotnet/runtime",
      "publishedAt": "2026-05-01T00:00:00+00:00",
      "relevanceScore": 123456
    }
  ],
  "warnings": []
}
```

If one provider fails, the API returns partial data from the successful providers and includes a warning. Providers also keep short-lived fallback cache entries so a failing external API can return the most recent successful result when available.

### Get Provider Statistics

```http
GET /api/statistics
```

Example:

```powershell
Invoke-RestMethod "http://localhost:5057/api/statistics"
```

Statistics include total request count, average response time in milliseconds, and response time buckets:

- `fast`: under 100 ms
- `average`: 100-200 ms
- `slow`: over 200 ms

## External Providers

| Provider | Data returned | Cache duration |
| --- | --- | --- |
| GitHub | Top 5 repositories for the query, sorted by stars | 5 minutes |
| Hacker News | Top 10 current stories | 5 minutes |
| Open-Meteo | Current temperature and wind speed for Athens | 10 minutes |

Fallback cache entries are retained for 1 hour after successful provider calls.

## Testing

Run the test suite:

```powershell
dotnet test ApiAggregator.sln
```

The tests cover:

- Combining results from multiple providers
- Returning partial data and warnings when a provider fails
- Filtering by category
- Sorting by relevance
- Thread-safe in-memory statistics recording and performance buckets

