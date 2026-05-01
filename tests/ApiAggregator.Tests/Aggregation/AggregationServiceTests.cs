using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Tests.Mocks;
using FluentAssertions;

namespace ApiAggregator.Tests.Aggregation;

public sealed class AggregationServiceTests
{
    [Fact]
    public async Task GetAggregatedDataAsync_WhenProvidersSucceed_ReturnsCombinedItems() {
        var providers = new[] {
            new MockExternalApiProvider(
                "ProviderA",
                [
                    new AggregatedItemDto {
                        Source = "ProviderA",
                        Title = "Unity repository",
                        Category = "technology",
                        PublishedAt = DateTimeOffset.UtcNow.AddDays(-1),
                        RelevanceScore = 10
                    }
                ]),
            new MockExternalApiProvider(
                "ProviderB",
                [
                    new AggregatedItemDto
                    {
                        Source = "ProviderB",
                        Title = "Weather in Athens",
                        Category = "weather",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 5
                    }
                ])
        };

        var statisticsStore = new MockRequestStatisticsStore();
        var service = new AggregationService(providers, statisticsStore);

        var response = await service.GetAggregatedDataAsync(
            new AggregationQuery(),
            CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Warnings.Should().BeEmpty();

        statisticsStore.Records.Should().HaveCount(2);
        statisticsStore.Records.Select(record => record.ApiName)
            .Should()
            .BeEquivalentTo(["ProviderA", "ProviderB"]);
    }
}