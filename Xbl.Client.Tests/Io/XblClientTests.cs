using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Moq.Protected;
using Spectre.Console;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl;
using Xbl.Client.Repositories;

namespace Xbl.Client.Tests.Io;

[TestClass]
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

    private AchievementTitles _titles;

    [TestInitialize]
    public void Setup()
    {
        SetupTestData();
        SetupXblRepositoryMock();
        SetupDboxRepositoryMock();
        SetupConsoleMock();
        SetupHttpMessageHandlerMock();
        SetupProgressContextMock();

        _settings = new Settings { Update = "all" };
        _xblClient = new XblClient(_settings, _httpClient, _xblRepositoryMock.Object, _dboxRepositoryMock.Object, _consoleMock.Object);
    }

    [TestMethod]
    public async Task Update_ShouldReturnZeroOnSuccess()
    {
        // Arrange

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
        }

    }

    [TestMethod]
    public async Task Update_ShouldReturnErrorCodeOnHttpRequestException()
    {
        // Arrange
        _consoleMock.Setup(c => c.Progress(It.IsAny<Func<IProgressContext, Task>>())).ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _xblClient.Update();

        // Assert
        result.Should().NotBe(0);
    }

    //[TestMethod]
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

    private void SetupTestData()
    {
        var titles = new[] { new Title
        {
            IntId = "1915865634",
            Achievement = new AchievementSummary { CurrentAchievements = 1 },
            CompatibleDevices = new[] { Device.XboxOne },
            TitleHistory = new TitleHistory
            {
                LastTimePlayed = DateTime.Now
            }
        }};
        _titles = new AchievementTitles { Xuid = "123", Titles = titles };
    }

    private void SetupXblRepositoryMock()
    {
        _xblRepositoryMock = new Mock<IXblRepository>();
        _xblRepositoryMock.Setup(x => x.LoadJson<Title[]>(It.IsAny<string>())).ReturnsAsync(_titles.Titles);
        _xblRepositoryMock.Setup(x => x.GetAchievementSaveDate(It.IsAny<Title>())).Returns(DateTime.Now);
        _xblRepositoryMock.Setup(x => x.GetStatsSaveDate(It.IsAny<Title>())).Returns(DateTime.Now);
    }

    private void SetupDboxRepositoryMock()
    {
        _dboxRepositoryMock = new Mock<IDboxRepository>();
        _dboxRepositoryMock.Setup(d => d.GetMarketplaceProducts()).ReturnsAsync(new Dictionary<string, Product>());
        _dboxRepositoryMock.Setup(d => d.GetStoreProducts()).ReturnsAsync(new Dictionary<string, Product>());
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
                    "https://example.com/achievements/" => new HttpResponseMessage { Content = new StringContent(JsonSerializer.Serialize(_titles)) },
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