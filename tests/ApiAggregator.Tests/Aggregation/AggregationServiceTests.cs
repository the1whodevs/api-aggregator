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
    
    [Fact]
    public async Task GetAggregatedDataAsync_WhenOneProviderThrows_ReturnsPartialDataAndWarning() {
        var providers = new[] {
            new MockExternalApiProvider(
                "SuccessfulProvider",
                [
                    new AggregatedItemDto {
                        Source = "SuccessfulProvider",
                        Title = "Successful item",
                        Category = "technology",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 10
                    }
                ]),
            new MockExternalApiProvider(
                "FailingProvider",
                new InvalidOperationException("Simulated failure"))
        };

        var statisticsStore = new MockRequestStatisticsStore();
        var service = new AggregationService(providers, statisticsStore);

        var response = await service.GetAggregatedDataAsync(
            new AggregationQuery(),
            CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Source.Should().Be("SuccessfulProvider");

        response.Warnings.Should().ContainSingle();
        response.Warnings[0].Should().Contain("FailingProvider");

        statisticsStore.Records.Should().HaveCount(2);
        statisticsStore.Records.Select(record => record.ApiName)
            .Should()
            .BeEquivalentTo(["SuccessfulProvider", "FailingProvider"]);
    }
    
    [Fact]
    public async Task GetAggregatedDataAsync_WhenCategoryIsProvided_FiltersItemsByCategory() {
        var providers = new[] {
            new MockExternalApiProvider(
                "ProviderA",
                [
                    new AggregatedItemDto {
                        Source = "ProviderA",
                        Title = "C# article",
                        Category = "technology",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 10
                    },
                    new AggregatedItemDto {
                        Source = "ProviderA",
                        Title = "Weather update",
                        Category = "weather",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 5
                    }
                ])
        };

        var statisticsStore = new MockRequestStatisticsStore();
        var service = new AggregationService(providers, statisticsStore);

        var response = await service.GetAggregatedDataAsync(
            new AggregationQuery {
                Category = "technology"
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Category.Should().Be("technology");
    }
    
    [Fact]
    public async Task GetAggregatedDataAsync_WhenSortByRelevanceDescending_ReturnsItemsInExpectedOrder() {
        var providers = new[] {
            new MockExternalApiProvider(
                "ProviderA",
                [
                    new AggregatedItemDto {
                        Source = "ProviderA",
                        Title = "Low relevance",
                        Category = "technology",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 1
                    },
                    new AggregatedItemDto {
                        Source = "ProviderA",
                        Title = "High relevance",
                        Category = "technology",
                        PublishedAt = DateTimeOffset.UtcNow,
                        RelevanceScore = 100
                    }
                ])
        };

        var statisticsStore = new MockRequestStatisticsStore();
        var service = new AggregationService(providers, statisticsStore);

        var response = await service.GetAggregatedDataAsync(
            new AggregationQuery {
                SortBy = "relevance",
                SortDirection = "desc"
            },
            CancellationToken.None);

        response.Items.Select(item => item.Title)
            .Should()
            .ContainInOrder("High relevance", "Low relevance");
    }
}