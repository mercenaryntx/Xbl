using FluentAssertions;
using FluentAssertions.Execution;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Xbl.Client.Models;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Queries;
using Xbl.Data;
using Xunit;

namespace Xbl.Client.Tests.Queries;

public class SqliteBuiltInQueriesTests
{
    private DatabaseContext _liveDb;
    private DatabaseContext _x360Db;
    private SqliteBuiltInQueries _builtInQueries;

    [Fact]
    public async Task Count_ShouldReturnProfilesSummary()
    {
        // Arrange
        Setup();

        // Act
        var result = await _builtInQueries.Count();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.UniqueTitlesCount.Should().Be(22);
            result.Profiles.Should().BeEquivalentTo([
                new ProfileSummary("Xbox Live", 11, 489, 11450, TimeSpan.FromMinutes(247*24*60+22*60+21)),
                new ProfileSummary("Xbox 360", 12, 452, 8045, TimeSpan.Zero)
            ]);
        }
    }

    [Fact]
    public async Task RarestAchievements_ShouldReturnRarestAchievements()
    {
        // Arrange
        Setup(3);

        // Act
        var result = await _builtInQueries.RarestAchievements();

        // Assert
        result.Should().BeEquivalentTo([
            new RarestAchievementItem("Bless Unleashed", "An Anonymous Odyssey", 0.05),
            new RarestAchievementItem("Bless Unleashed", "Encyclopedia Historia", 0.06),
            new RarestAchievementItem("Bless Unleashed", "Battlefield Warrior", 0.11)
        ]);
    }

    [Fact]
    public async Task MostComplete_ShouldReturnMostComplete()
    {
        // Arrange
        Setup();

        // Act
        var result = await _builtInQueries.MostComplete();

        // Assert
        result.Should().BeEquivalentTo([
            new CompletenessItem("Borderlands 2", Device.XboxOne, 1875, 1875, 100),
            new CompletenessItem("The First Descendant", Device.XboxOne, 1000, 1000, 100),
            new CompletenessItem("Lies of P", Device.XboxOne, 1000, 1000, 100),
            new CompletenessItem("Bless Unleashed", Device.XboxOne, 1000, 1000, 100),
            new CompletenessItem("The Walking Dead", Device.Xbox360, 600, 600, 100),
            new CompletenessItem("Walking Dead: Season 2", Device.Xbox360, 500, 500, 100),
            new CompletenessItem("DeathSpank: T.O.V.", Device.Xbox360, 215, 215, 100),
            new CompletenessItem("DeathSpank", Device.Xbox360, 200, 200, 100),
            new CompletenessItem("Saints Row: The Third", Device.Xbox360, 1280, 1300, 98),
            new CompletenessItem("Saints Row IV", Device.Xbox360, 1170, 1200, 97)
        ]);
    }

    [Fact]
    public async Task SpentMostTimeWith_ShouldReturnSpentMostTimeWith()
    {
        // Arrange
        Setup();

        // Act
        var result = await _builtInQueries.SpentMostTimeWith();

        // Assert
        result.Should().BeEquivalentTo([
            new MinutesPlayed("Bless Unleashed", 173365),
            new MinutesPlayed("The Binding of Isaac: Rebirth", 68377),
            new MinutesPlayed("Borderlands 2", 32594),
            new MinutesPlayed("Borderlands 3", 29693),
            new MinutesPlayed("The First Descendant", 19289),
            new MinutesPlayed("Tiny Tina's Wonderlands for Xbox Series X|S", 8587),
            new MinutesPlayed("Borderlands: The Pre-Sequel", 8330),
            new MinutesPlayed("Lies of P", 6762),
            new MinutesPlayed("Rocket League\u00ae", 5307),
            new MinutesPlayed("Injustice\u2122 2", 4717)
        ]);
    }

    [Fact]
    public async Task WeightedRarity_ShouldReturnWeightedRarity()
    {
        // Arrange
        Setup(1);

        // Act
        var result = await _builtInQueries.WeightedRarity();

        // Assert
        result.Should().BeEquivalentTo([
            new WeightedAchievementItem("Bless Unleashed", 
                new AchievementSummary 
                {
                    CurrentAchievements = 43,
                    CurrentGamerscore = 1000,
                    ProgressPercentage = 100,
                    SourceVersion = 2,
                    TotalAchievements = 0,
                    TotalGamerscore = 1000
                }, 43,43, 38, 366041460.18101364)
        ]);
    }

    [Fact]
    public async Task Categories_ShouldReturnCategories()
    {
        // Arrange
        Setup();

        // Act
        var result = await _builtInQueries.Categories();

        // Assert
        result.Should().BeEquivalentTo([
            new CategorySlice("Action & adventure", 7),
            new CategorySlice("Role playing", 6),
            new CategorySlice("Shooter", 3),
            new CategorySlice("Other", 4),
            new CategorySlice("Sports", 1),
            new CategorySlice("Fighting", 1)
        ]);
    }

    private void Setup(int limit = 10)
    {
        var gc = new GlobalConfig { DataFolder = DataSource.DataFolder };
        _liveDb = new DatabaseContext("live", gc);
        _x360Db = new DatabaseContext("x360", gc);

        MicroOrmConfig.SqlProvider = SqlProvider.SQLite;
        var settings = new Settings { Limit = limit };
        _builtInQueries = new SqliteBuiltInQueries(settings, _liveDb, _x360Db);
    }
}