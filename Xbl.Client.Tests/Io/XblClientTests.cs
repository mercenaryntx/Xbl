using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Moq;
using Spectre.Console;
using Xbl.Client.Extensions;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Tests.Extensions;
using Xbl.Data;
using Xunit;

namespace Xbl.Client.Tests.Io;

public class XblClientTests
{
    private DatabaseContext _db;

    private Mock<IConsole> _consoleMock;
    private Mock<IProgressContext> _progressContextMock;
    private HttpClient _httpClient;
    private XblClient _xblClient;
    private Settings _settings;

    private AchievementTitles _achievementsResponse;
    private Dictionary<string, TitleDetails<LiveAchievement>> _achievementsTitleResponse;
    private Dictionary<string, TitleDetails<Achievement>> _achievementsX360TitleResponse;
    private TitleStats _playerStatsResponse;

    private Product[] _storeProducts;
    private Product[] _marketplaceProducts;

    [Fact]
    public async Task Update_UpdateTitles_ShouldInsertXboxOneTitles()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        SetupTestStore(xone);
        await Setup();

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            var titles = await AssertTitles();

            var t1 = titles.Single(t => t.IntId == xone.IntId);
            t1.Should().NotBeNull();
            t1.Category.Should().Be("Shooter");
            t1.IsBackCompat.Should().BeFalse();
            t1.Products.Values.Should().BeEquivalentTo([new TitleProduct { ProductId = "12345678", TitleId = t1.HexId }
            ]);
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldUpdateXboxOneTitlesIfHttpResponseIsNewer()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = DateTime.Now.AddDays(1);
        SetupTestTitles(xone);
        SetupTestStore(xone);
        await Setup();

        var r = await _db.GetRepository<Title>();
        await r.Insert(new Title
        {
            IntId = "1915865634"
        });

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            var titles = await AssertTitles();

            var t1 = titles.Single(t => t.IntId == xone.IntId);
            t1.Should().NotBeNull();
            t1.Category.Should().Be("Shooter");
            t1.IsBackCompat.Should().BeFalse();
            t1.Products.Values.Should().BeEquivalentTo([new TitleProduct { ProductId = "12345678", TitleId = t1.HexId }
            ]);
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldNotUpdateXboxOneTitlesIfHttpResponseIsOlder()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        SetupTestStore(xone);
        await Setup();

        var r = await _db.GetRepository<Title>();
        await r.Insert(new Title
        {
            IntId = "1915865634"
        });

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            var title = await r.Get(1915865634);
            title.Should().NotBeEquivalentTo(_achievementsResponse.Titles[0],
                o => o.Excluding(t => t.HexId)
                    .Excluding(t => t.Source)
                    .Excluding(t => t.OriginalConsole)
                    .Excluding(t => t.Products)
                    .Excluding(t => t.Category)
                    .Excluding(t => t.IsBackCompat));
            title.Source.Should().NotBe(DataSource.Live);
            title.Category.Should().NotBe("Shooter");
            title.Products.Should().BeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldInsertXbox360Titles()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        await Setup();

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            var titles = await AssertTitles();

            var t1 = titles.Single(t => t.IntId == x360.IntId);
            t1.Should().NotBeNull();
            t1.Category.Should().Be("Shooter");
            t1.IsBackCompat.Should().BeFalse();
            t1.Products.Values.Should().BeEquivalentTo([new TitleProduct { ProductId = "98765432", TitleId = t1.HexId }
            ]);
        }
    }

    [Fact]
    public async Task Update_UpdateTitles_ShouldInsertBackCompatTitles()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        var xone = CreateTestTitle(Device.XboxOne, "1745345352");
        xone.Name = $"[Fission] Backcompat game ({x360.IntId.ToHexId()})";
        xone.CompatibleDevices = [ Device.Xbox360 ];
        SetupTestTitles(x360);
        SetupTestStore(xone);
        SetupTestMarketplace(x360);
        await Setup();

        // Act
        var result = await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(0);
            var titles = await AssertTitles();

            var t1 = titles.Single(t => t.IntId == x360.IntId);
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
    public async Task Update_UpdateAchievements_ShouldInsertXboxOneAchievement()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        await Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Achievement>();
            var i = await r.Get(1, 1915865634);
            i.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotUpdateXboxOneAchievementIfItsNewer()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        await Setup();

        var r = await _db.GetRepository<Achievement>();
        await r.Insert(new Achievement
        {
            Id =1,
            TitleId = 1915865634,
            Name = "I am the newest",
            TimeUnlocked = DateTime.Now
        });

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var i = await r.Get(1, 1915865634);
            i.Should().NotBeNull();
            i.Name.Should().Be("I am the newest");
            i.TimeUnlocked.Should().BeAfter(new DateTime(2020, 4, 1));
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotUpdateXboxOneAchievementIfUpdateIsStats()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        await Setup("stats");


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Achievement>();
            var i = await r.Get(1, 1915865634);
            i.Should().BeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldInsertXbox360Achievement()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        await Setup();

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Achievement>();
            var i = await r.Get(1, 1915865634);
            i.Should().NotBeNull();
            i.TitleName.Should().Be(x360.Name);
        }
    }

    [Fact]
    public async Task Update_UpdateAchievements_ShouldNotUpdateXbox360AchievementIfItsNewer()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        await Setup();

        var r = await _db.GetRepository<Achievement>();
        await r.Insert(new Achievement
        {
            Id = 1,
            TitleId = 1915865634,
            Name = "I am the newest",
            TimeUnlocked = DateTime.Now
        });

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var i = await r.Get(1, 1915865634);
            i.Should().NotBeNull();
            i.Name.Should().Be("I am the newest");
            i.TimeUnlocked.Should().BeAfter(new DateTime(2020, 4, 1));
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldInsertXboxOneStats()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        await Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Stat>();
            var i = await r.Get(1915865634);
            i.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotCreateXboxOneStatsIfItsNewer()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        SetupTestTitles(xone);
        await Setup();

        var r = await _db.GetRepository<Title>();
        await r.Insert(new Title
        {
            IntId = "1915865634"
        });

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r2 = await _db.GetRepository<Stat>();
            var i = await r2.Get(1915865634);
            i.Should().BeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotInsertXbox360Stats()
    {
        // Arrange
        var x360 = CreateTestTitle(Device.Xbox360, "1915865634");
        SetupTestTitles(x360);
        SetupTestMarketplace(x360);
        await Setup();

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Stat>();
            var i = await r.Get(1,1915865634);
            i.Should().BeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotInsertMobileStats()
    {
        // Arrange
        var mob = CreateTestTitle(Device.Mobile, "1915865634");
        SetupTestTitles(mob);
        await Setup();


        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Stat>();
            var i = await r.Get(1, 1915865634);
            i.Should().BeNull();
        }
    }

    [Fact]
    public async Task Update_UpdateStats_ShouldNotInsertXboxOneStatsIfUpdateUpdateIsAchievements()
    {
        // Arrange
        var xone = CreateTestTitle(Device.XboxOne, "1915865634");
        xone.TitleHistory.LastTimePlayed = new DateTime(2020, 4, 1);
        SetupTestTitles(xone);
        await Setup("achievements");

        // Act
        await _xblClient.Update();

        // Assert
        using (new AssertionScope())
        {
            var r = await _db.GetRepository<Stat>();
            var i = await r.Get(1, 1915865634);
            i.Should().BeNull();
        }
    }

    [Fact]
    public async Task Update_ShouldReturnErrorCodeOnHttpRequestException()
    {
        // Arrange
        await Setup();
        _consoleMock.Setup(c => c.Progress(It.IsAny<Func<IProgressContext, Task>>())).ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _xblClient.Update();

        // Assert
        result.Should().NotBe(0);
    }

    private async Task<Title[]> AssertTitles()
    {
        var r = await _db.GetRepository<Title>();
        var titles = (await r.GetAll()).ToArray();

        titles.Should().BeEquivalentTo(_achievementsResponse.Titles, 
            o => o.Excluding(t => t.HexId)
                  .Excluding(t => t.Source)
                  .Excluding(t => t.OriginalConsole)
                  .Excluding(t => t.Products)
                  .Excluding(t => t.Category)
                  .Excluding(t => t.IsBackCompat));
        titles.Should().AllSatisfy(t => t.Source.Should().Be(DataSource.Live));
        titles.Should().AllSatisfy(t => t.HexId.Should().Be(t.IntId.ToHexId()));
        titles.Should().AllSatisfy(t => t.OriginalConsole.Should().Be(t.CompatibleDevices.First()));

        return titles;
    }

    private async Task Setup(string update = "all")
    {
        await SetupDatabaseMock();
        SetupConsoleMock();
        SetupHttpClientMock();
        SetupProgressContextMock();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();

        _settings = new Settings { Update = update };
        _xblClient = new XblClient(_settings, _httpClient, _consoleMock.Object, mapper, _db, _db, _db);
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
        _storeProducts = titles.Select( t => new Product
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
        }).ToArray();
    }

    private void SetupTestMarketplace(params Title[] titles)
    {
        _marketplaceProducts = titles.Select(t => new Product
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
        }).ToArray();
    }

    private async Task SetupDatabaseMock()
    {
        _db = new DatabaseContext();
        if (_marketplaceProducts != null)
        {
            var marketplace = await _db.GetRepository<Product>(DataTable.Marketplace);
            await marketplace.BulkInsert(_marketplaceProducts);
        }

        if (_storeProducts != null)
        {
            var store = await _db.GetRepository<Product>(DataTable.Store);
            await store.BulkInsert(_storeProducts);
        }
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