using FluentAssertions;
using FluentAssertions.Execution;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Xbl.Client.Models;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Queries;
using Xbl.Data;
using Xunit;

namespace Xbl.Client.Tests.Queries;

public class BuiltInQueriesTests
{
    private DatabaseContext _liveDb;
    private DatabaseContext _x360Db;
    private BuiltInQueries _builtInQueries;

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
            result.UniqueTitlesCount.Should().Be(974);
            result.Profiles.Should().BeEquivalentTo([
                new ProfileSummary("Xbox Live", 601, 4274, 104993, TimeSpan.FromMinutes(371*24*60+8*60+58)),
                new ProfileSummary("Xbox 360", 379, 3006, 50105, TimeSpan.Zero)
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
        var array = result.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new RarestAchievementItem("Bless Unleashed", "An Anonymous Odyssey", 0.05),
            new RarestAchievementItem("Bless Unleashed", "Encyclopedia Historia", 0.06),
            new RarestAchievementItem("Bless Unleashed", "Battlefield Warrior", 0.11)
        ]);
    }

    [Fact]
    public async Task MostComplete_ShouldReturnMostComplete()
    {
        // Arrange
        Setup(2);

        // Act
        var result = await _builtInQueries.MostComplete();
        var array = result.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new CompletenessItem("Borderlands 2", 1875, 1875, 100),
            new CompletenessItem("Indiana Jones and the Great Circle", 1000, 1000, 100)
        ]);
    }

    [Fact]
    public async Task SpentMostTimeWith_ShouldReturnSpentMostTimeWith()
    {
        // Arrange
        Setup();

        // Act
        var result = await _builtInQueries.SpentMostTimeWith();
        var array = result.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
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
        var array = result.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
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
        var array = result.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new CategorySlice("Action & adventure", 540),
            new CategorySlice("Other", 158),
            new CategorySlice("Role playing", 59),
            new CategorySlice("Shooter", 47),
            new CategorySlice("Fighting", 35),
            new CategorySlice("Racing & flying", 27),
            new CategorySlice("Platformer", 26),
            new CategorySlice("Card & board", 11),
            new CategorySlice("Puzzle & trivia", 11),
            new CategorySlice("Strategy", 10)
        ]);
    }

    private void Setup(int limit = 10)
    {
        _liveDb = new DatabaseContext("live");
        _x360Db = new DatabaseContext("x360");

        MicroOrmConfig.SqlProvider = SqlProvider.SQLite;
        var settings = new Settings { Limit = limit };
        _builtInQueries = new BuiltInQueries(settings, _liveDb, _x360Db);
    }
}