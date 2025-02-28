using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Moq.Protected;
using Spectre.Console;
using Xbl.Client.Extensions;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl;
using Xbl.Client.Repositories;
using Xunit;

namespace Xbl.Client.Tests.Io;

public class XblClientTests
{
    private Mock<IXblRepository> _xblRepositoryMock;
    private Mock<IDboxRepository> _dboxRepositoryMock;
    private Mock<IConsole> _consoleMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IProgressContext> _progressContextMock;
    private HttpClient _httpClient;
    private XblClient _xblClient;
    private Settings _settings;

    private AchievementTitles _achievementsResponse;
    private AchievementTitles _titlesFile;
    private Dictionary<string, Product> _storeProducts;
    private Dictionary<string, Product> _marketplaceProducts;

    [Fact]
    public async Task Update_ShouldCreateCorrectXboxOneTitlesFile()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        SetupTestStore(xone);
        Setup();

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            AssertTitles();

            var t1 = _titlesFile.Titles.Single(t => t.IntId == xone.IntId);
            t1.Should().NotBeNull();
            t1.Category.Should().Be("Shooter");
            t1.IsBackCompat.Should().BeFalse();
            t1.Products.Values.Should().BeEquivalentTo(new[] { new TitleProduct { ProductId = "12345678", TitleId = t1.HexId } });
        }
    }

    [Fact]
    public async Task Update_ShouldCreateCorrectXbox360TitlesFile()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestStore(x360);
        Setup();

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            AssertTitles();

            var t1 = _titlesFile.Titles.Single(t => t.IntId == x360.IntId);
            t1.Should().NotBeNull();
            t1.Category.Should().Be("Shooter");
            t1.IsBackCompat.Should().BeTrue();
            t1.Products.Values.Should().BeEquivalentTo(new[] { new TitleProduct { ProductId = "12345678", TitleId = t1.HexId } });
        }
    }

    [Fact]
    public async Task Update_ShouldCreateCorrectXboxOneAchievementFile()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        SetupTestStore(xone);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
        }
    }

    [Fact]
    public async Task Update_ShouldReturnErrorCodeOnHttpRequestException()
    {
        // Arrange
        Setup();
        _consoleMock.Setup(c => c.Progress(It.IsAny<Func<IProgressContext, Task>>())).ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _xblClient.Update();

        // Assert
        result.Should().NotBe(0);
    }

    private void AssertTitles()
    {
        _titlesFile.Xuid.Should().Be(_achievementsResponse.Xuid);
        _titlesFile.Titles.Should().BeEquivalentTo(_achievementsResponse.Titles, o => o.Excluding(t => t.HexId).Excluding(t => t.Source).Excluding(t => t.OriginalConsole).Excluding(t => t.Products).Excluding(t => t.Category));
        _titlesFile.Titles.Should().OnlyContain(t => t.Source == "live");
        _titlesFile.Titles.Should().AllSatisfy(t => t.HexId = t.IntId.ToHexId());
    }

    //[Fact]
    //public async Task GetStatsBulk_ShouldSaveStats()
    //{
    //    // Arrange
    //    var titles = new List<Title>
    //    {
    //        new Title { IntId = "123", CompatibleDevices = new[] { "Device1" }, TitleHistory = new TitleHistory { LastTimePlayed = DateTime.Now } }
    //    };
    //    var responseContent = "{\"StatListsCollection\":[{\"Stats\":[{\"TitleId\":\"123\"}]}]}";
    //    _httpMessageHandlerMock.Protected()
    //        .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
    //        .ReturnsAsync(new HttpResponseMessage { Content = new StringContent(responseContent) });

    //    // Act
    //    await _xblClient.GetStatsBulk(new ProgressContext(), titles);

    //    // Assert
    //    _xblRepositoryMock.Verify(x => x.SaveJson(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    //}

    public void Setup()
    {
        SetupXblRepositoryMock();
        SetupDboxRepositoryMock();
        SetupConsoleMock();
        SetupHttpMessageHandlerMock();
        SetupProgressContextMock();

        _settings = new Settings { Update = "all" };
        _xblClient = new XblClient(_settings, _httpClient, _xblRepositoryMock.Object, _dboxRepositoryMock.Object, _consoleMock.Object);
    }

    private static Title CreateTestTitle(string device, string id)
    {
        return new Title
        {
            IntId = id,
            Name = $"[{id.ToHexId()}] {device} Title",
            Achievement = new AchievementSummary {CurrentAchievements = 1},
            CompatibleDevices = new[] {device},
            TitleHistory = new TitleHistory
            {
                LastTimePlayed = DateTime.Now
            }
        };
    }

    private void SetupTestTitles(params Title[] titles)
    {
        _achievementsResponse = new AchievementTitles {Xuid = "123", Titles = titles};
    }

    private void SetupTestStore(params Title[] titles)
    {
        _storeProducts = titles.ToDictionary(t => t.IntId.ToHexId(), t => new Product
        {
            TitleId = t.IntId.ToHexId(), // "7231CA22"
            Title = t.Name,
            Category = "Shooter",
            Versions = new Dictionary<string, ProductVersion>
            {
                [t.CompatibleDevices.Single()] = new()
                {
                    Title = t.IntId.ToHexId(),
                    ProductId = "12345678"
                }
            }
        });
    }

    private void SetupXblRepositoryMock()
    {
        _xblRepositoryMock = new Mock<IXblRepository>();
        _xblRepositoryMock.Setup(x => x.SaveTitles(It.IsAny<string>(), It.IsAny<AchievementTitles>())).Callback((string _, AchievementTitles titles) => _titlesFile = titles);
        _xblRepositoryMock.Setup(x => x.LoadJson<Title[]>(It.IsAny<string>())).ReturnsAsync((string _) => _titlesFile.Titles);
        _xblRepositoryMock.Setup(x => x.GetAchievementSaveDate(It.IsAny<Title>())).Returns(DateTime.Now);
        _xblRepositoryMock.Setup(x => x.GetStatsSaveDate(It.IsAny<Title>())).Returns(DateTime.Now);
    }

    private void SetupDboxRepositoryMock()
    {
        _dboxRepositoryMock = new Mock<IDboxRepository>();
        _dboxRepositoryMock.Setup(d => d.GetMarketplaceProducts()).ReturnsAsync(_marketplaceProducts);
        _dboxRepositoryMock.Setup(d => d.GetStoreProducts()).ReturnsAsync(_storeProducts);
    }

    private void SetupConsoleMock()
    {
        _consoleMock = new Mock<IConsole>();
        _consoleMock.Setup(c => c.Progress(It.IsAny<Func<IProgressContext, Task>>())).Callback<Func<IProgressContext, Task>>(f => f(_progressContextMock.Object).GetAwaiter().GetResult());
        _consoleMock.Setup(c => c.ShowError(It.IsAny<string>())).Returns(1);
    }

    private void SetupHttpMessageHandlerMock()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                return req.RequestUri?.ToString() switch
                {
                    "https://example.com/achievements/" => new HttpResponseMessage { Content = new StringContent(JsonSerializer.Serialize(_achievementsResponse)) },
                    _ => new HttpResponseMessage { Content = new StringContent("wrong") }
                };
            });

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };
    }

    private void SetupProgressContextMock()
    {
        _progressContextMock = new Mock<IProgressContext>();
        _progressContextMock.Setup(p => p.AddTask(It.IsAny<string>(), It.IsAny<double>())).Returns((string s, double i) => new ProgressTask(1, s, i));
    }
}