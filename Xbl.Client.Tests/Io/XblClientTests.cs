using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Spectre.Console;
using Xbl.Client.Extensions;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Repositories;
using Xbl.Client.Tests.Extensions;
using Xunit;

namespace Xbl.Client.Tests.Io;

public class XblClientTests
{
    private Mock<IXblRepository> _xblRepositoryMock;
    private Mock<IDboxRepository> _dboxRepositoryMock;
    private Mock<IConsole> _consoleMock;
    private Mock<IProgressContext> _progressContextMock;
    private HttpClient _httpClient;
    private XblClient _xblClient;
    private Settings _settings;

    private AchievementTitles _achievementsResponse;
    private Dictionary<string, TitleDetails<LiveAchievement>> _achievementsTitleResponse;
    private Dictionary<string, TitleDetails<Achievement>> _achievementsX360TitleResponse;
    private TitleStats _playerStatsResponse;

    private AchievementTitles _titlesFile;
    private readonly Dictionary<string, string> _achievementFiles = new();
    private readonly Dictionary<string, TitleStats> _statsFiles = new();

    private Dictionary<string, Product> _storeProducts = new();
    private Dictionary<string, Product> _marketplaceProducts = new();

    [Fact]
    public async Task Update_UpdateTitles_ShouldCreateCorrectXboxOneTitlesFile()
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
            t1.Products.Values.Should().BeEquivalentTo([new TitleProduct { ProductId = "12345678", TitleId = t1.HexId }
            ]);
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldCreateCorrectXbox360TitlesFile()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
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
            t1.IsBackCompat.Should().BeFalse();
            t1.Products.Values.Should().BeEquivalentTo([new TitleProduct { ProductId = "98765432", TitleId = t1.HexId }
            ]);
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldCreateCorrectBackCompatTitlesFile()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        var xone = CreateTestTitle(Device.XboxOne, "1745345352");
        xone.Name = $"[Fission] Backcompat game ({x360.IntId.ToHexId()})";
        xone.CompatibleDevices = [ Device.Xbox360 ];
        SetupTestTitles(x360);
        SetupTestStore(xone);
        SetupTestMarketplace(x360);
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
            t1.Products.Should().BeEquivalentTo(new[]
            {
                new KeyValuePair<string,TitleProduct>(Device.Xbox360, new TitleProduct { ProductId = "98765432", TitleId = t1.HexId }),
                new KeyValuePair<string,TitleProduct>("BackCompat", new TitleProduct { ProductId = "12345678", TitleId = xone.IntId.ToHexId() })
            });
        }
    }


    [Fact]
    public async Task Update_UpdateAchievements_ShouldCreateXboxOneAchievementFile()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _achievementFiles.Should().ContainKey("7231CA22");
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotCreateXboxOneAchievementFileIfLastTimePlayedIsOld()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(xone);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _achievementFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotCreateXboxOneAchievementFileIfUpdateIsStats()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(xone);
        Setup("stats");


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _achievementFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldCreateXbox360AchievementFile()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _achievementFiles.Should().ContainKey("7231CA22");
            _achievementFiles["7231CA22"].Should().Be(x360.Name);
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotCreateXbox360AchievementFileIfLastTimePlayedIsOld()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        x360.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _achievementFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldCreateXboxOneStatsFile()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _statsFiles.Should().ContainKey("7231CA22");
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotCreateXboxOneStatsFileIfLastTimePlayedIsOld()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(xone);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _statsFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotCreateXbox360StatsFile()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _statsFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotCreateMobileStatsFile()
    {
        // Arrange
        var mob = CreateTestTitle(Device.Mobile, "1915865634");
        SetupTestTitles(mob);
        Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _statsFiles.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotCreateXboxOneStatsFileIfUpdateUpdateIsAchievements()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(xone);
        Setup("achievements");


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            _statsFiles.Should().BeEmpty();
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
        _titlesFile.Titles.Should().BeEquivalentTo(_achievementsResponse.Titles, 
            o => o.Excluding(t => t.HexId)
                  .Excluding(t => t.Source)
                  .Excluding(t => t.OriginalConsole)
                  .Excluding(t => t.Products)
                  .Excluding(t => t.Category)
                  .Excluding(t => t.IsBackCompat));
        _titlesFile.Titles.Should().OnlyContain(t => t.Source == "live");
        _titlesFile.Titles.Should().AllSatisfy(t => t.HexId.Should().Be(t.IntId.ToHexId()));
        _titlesFile.Titles.Should().AllSatisfy(t => t.OriginalConsole.Should().Be(t.CompatibleDevices.First()));
    }

    private void Setup(string update = "all")
    {
        SetupXblRepositoryMock();
        SetupDboxRepositoryMock();
        SetupConsoleMock();
        SetupHttpClientMock();
        SetupProgressContextMock();

        _settings = new Settings { Update = update };
        _xblClient = new XblClient(_settings, _httpClient, _xblRepositoryMock.Object, _dboxRepositoryMock.Object, _consoleMock.Object);
    }

    private static Title CreateTestTitle(string device, string id)
    {
        return new Title
        {
            IntId = id,
            Name = $"[{id.ToHexId()}] {device} Title",
            Achievement = new AchievementSummary {CurrentAchievements = 1},
            CompatibleDevices = [device],
            TitleHistory = new TitleHistory
            {
                LastTimePlayed = new DateTime(2024, 4, 1)
            }
        };
    }

    private void SetupTestTitles(params Title[] titles)
    {
        _achievementsResponse = new AchievementTitles {Xuid = "123", Titles = titles};
        _achievementsTitleResponse = titles.ToDictionary(t => t.IntId, t => new TitleDetails<LiveAchievement>
        {
            Achievements =
            [
                new LiveAchievement
                {
                    Id = "1",
                    Name = "Achievement 1",
                    TitleAssociations = [new TitleAssociation { Id = int.Parse(t.IntId), Name = t.Name }],
                    ProgressState = "Achieved",
                    Progression = new Progression { TimeUnlocked = new DateTime(2024, 4, 1) },
                    Platforms = [t.CompatibleDevices.Single()],
                    Rewards = [new Reward { Value = "10", ValueType = "Int", Type = "Gamerscore" }],
                    Rarity = new Rarity { CurrentCategory = "Common", CurrentPercentage = 50 }
                }
            ]
        });
        _achievementsX360TitleResponse = titles.ToDictionary(t => t.IntId, t => new TitleDetails<Achievement>
        {
            Achievements =
            [
                new Achievement
                {
                    Id = 1,
                    Name = "Achievement 1",
                    TitleId = int.Parse(t.IntId),
                    TimeUnlocked = new DateTime(2024, 4, 1),
                    Unlocked = true,
                    Platform = t.CompatibleDevices.Single(),
                    Gamerscore = 10,
                    Rarity = new Rarity { CurrentCategory = "Common", CurrentPercentage = 50 }
                }
            ]
        });
        _playerStatsResponse = new TitleStats
        {
            StatListsCollection =
            [
                new StatList
                {
                    Stats = titles.Select(t => new Stat { TitleId = t.IntId, Name = "MinutesPlayed", Value = "150", Type = "Integer" }).ToArray()
                }
            ]
        };
    }

    private void SetupTestStore(params Title[] titles)
    {
        _storeProducts = titles.ToDictionary(t => t.IntId.ToHexId(), t => new Product
        {
            TitleId = t.IntId.ToHexId(),
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

    private void SetupTestMarketplace(params Title[] titles)
    {
        _marketplaceProducts = titles.ToDictionary(t => t.IntId.ToHexId(), t => new Product
        {
            TitleId = t.IntId.ToHexId(), // "7231CA22"
            Title = t.Name,
            Category = "Shooter",
            Versions = new Dictionary<string, ProductVersion>
            {
                [t.CompatibleDevices.Single()] = new()
                {
                    Title = t.IntId.ToHexId(),
                    ProductId = "98765432"
                }
            }
        });
    }

    private void SetupXblRepositoryMock()
    {
        var datetime = new DateTime(2023, 6, 1);
        _xblRepositoryMock = new Mock<IXblRepository>();
        _xblRepositoryMock.Setup(x => x.SaveTitles(It.IsAny<string>(), It.IsAny<AchievementTitles>())).Callback((string _, AchievementTitles titles) => _titlesFile = titles);
        _xblRepositoryMock.Setup(x => x.SaveAchievements(It.IsAny<Title>(), It.IsAny<string>())).Callback((Title title, string data) => _achievementFiles[title.IntId.ToHexId()] = data);
        _xblRepositoryMock.Setup(x => x.SaveAchievements(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TitleDetails<Achievement>>())).Callback((string _, string hexId, TitleDetails<Achievement> achievements) => _achievementFiles[hexId] = string.Join(',', achievements.Achievements.Select(a => a.TitleName)));
        _xblRepositoryMock.Setup(x => x.SaveStats(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TitleStats>())).Callback((string _, string hexId, TitleStats stats) => _statsFiles[hexId] = stats);
        _xblRepositoryMock.LoadJson(_ => _titlesFile.Titles);
        _xblRepositoryMock.LoadJson(path => _achievementsTitleResponse[path]);
        _xblRepositoryMock.Setup(x => x.GetAchievementSaveDate(It.IsAny<Title>())).Returns(datetime);
        _xblRepositoryMock.Setup(x => x.GetStatsSaveDate(It.IsAny<Title>())).Returns(datetime);
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

    private void SetupHttpClientMock()
    {
        _httpClient = new HttpClientMockBuilder("https://example.com")
            .AddResponse("/achievements/", _ => _achievementsResponse.ToHttpResponseMessage())
            .AddResponse("/achievements/title/1915865634", _ => _achievementsTitleResponse["1915865634"].ToHttpResponseMessage())
            .AddResponse("/achievements/x360/123/title/1915865634", _ => _achievementsX360TitleResponse["1915865634"].ToHttpResponseMessage())
            .AddResponse("/achievements/title/1745345352", _ => _achievementsTitleResponse["1745345352"].ToHttpResponseMessage())
            .AddResponse("/player/stats", _ => _playerStatsResponse.ToHttpResponseMessage())
            .Build();
    }

    private void SetupProgressContextMock()
    {
        _progressContextMock = new Mock<IProgressContext>();
        _progressContextMock.Setup(p => p.AddTask(It.IsAny<string>(), It.IsAny<double>())).Returns((string s, double i) => new ProgressTask(1, s, i));
    }
}