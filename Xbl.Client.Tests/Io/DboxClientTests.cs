using System.Text.Json;
using FluentAssertions;
using Moq;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Repositories;
using Xbl.Client.Tests.Extensions;
using Xunit;

namespace Xbl.Client.Tests.Io;

public class DboxClientTests
{
    private Mock<IDboxRepository> _repositoryMock;
    private Mock<IConsole> _consoleMock;
    private HttpClient _httpClient;
    private DboxClient _dboxClient;

    private readonly Dictionary<string, string> _repository = new();

    [Fact]
    public async Task Update_SavesThreeFilesIfEverythingFitsInto10000()
    {
        // Arrange
        Setup();

        // Act
        await _dboxClient.Update();

        // Assert
        _repository.Keys.Should().BeEquivalentTo(@"data\dbox\_marketplace.1.json", @"data\dbox\_marketplace.14.json", @"data\dbox\_store.000.json");
    }

    [Fact]
    public async Task Update_SavesFourFilesIfStoreIsBiggerThan10000()
    {
        // Arrange
        Setup(10001);

        // Act
        await _dboxClient.Update();

        // Assert
        _repository.Keys.Should().BeEquivalentTo(@"data\dbox\_marketplace.1.json", @"data\dbox\_marketplace.14.json", @"data\dbox\_store.000.json", @"data\dbox\_store.001.json");
    }


    private void Setup(int storeCount = 1)
    {
        var marketplaceProductCollection = new MarketplaceProductCollection
        {
            Products = [],
            Count = 1
        };
        var storeProductCollection = new StoreProductCollection
        {
            Products = [],
            Count = storeCount
        };

        _consoleMock = new Mock<IConsole>();

        _repositoryMock = new Mock<IDboxRepository>();
        _repositoryMock.SaveJson((path, s) => _repository[path] = s);
        _repositoryMock.LoadJson(path => JsonSerializer.Deserialize<MarketplaceProductCollection>(_repository[path]));
        _repositoryMock.LoadJson(path => JsonSerializer.Deserialize<StoreProductCollection>(_repository[path]));

        _httpClient = new HttpClientMockBuilder("https://example.com")
            .AddResponse("/marketplace/products?limit=10000&offset=0&product_type=1", marketplaceProductCollection.ToHttpResponseMessage())
            .AddResponse("/marketplace/products?limit=10000&offset=0&product_type=14", marketplaceProductCollection.ToHttpResponseMessage())
            .AddResponse("/store/products?limit=10000&offset=0", storeProductCollection.ToHttpResponseMessage())
            .AddResponse("/store/products?limit=10000&offset=10000", storeProductCollection.ToHttpResponseMessage())
            .Build();

        _dboxClient = new DboxClient(_httpClient, _repositoryMock.Object, _consoleMock.Object);

    }
}