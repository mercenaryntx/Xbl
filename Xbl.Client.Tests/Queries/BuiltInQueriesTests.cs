using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xbl.Client.Models;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Queries;
using Xbl.Client.Repositories;
using Xunit;

namespace Xbl.Client.Tests.Queries;

public class BuiltInQueriesTests
{
    private readonly Mock<IXblRepository> _repositoryMock;
    private readonly BuiltInQueries _builtInQueries;

    public BuiltInQueriesTests()
    {
        _repositoryMock = new Mock<IXblRepository>();
        var settings = new Settings { Limit = 50 };
        _builtInQueries = new BuiltInQueries(settings, _repositoryMock.Object);
    }

    [Fact]
    public async Task Count_ShouldReturnProfilesSummary()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Source = DataSource.Live, Achievement = new AchievementSummary { CurrentAchievements = 10, CurrentGamerscore = 1000 } },
            new Title { Name = "Game2", Source = DataSource.Xbox360, Achievement = new AchievementSummary { CurrentAchievements = 5, CurrentGamerscore = 500 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.Count();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.UniqueTitlesCount.Should().Be(2);
            result.Profiles.Should().BeEquivalentTo([
                new ProfileSummary("live", 1, 10, 1000, TimeSpan.FromMinutes(20)),
                new ProfileSummary("x360", 1, 5, 500, TimeSpan.Zero)
            ]);
        }
    }

    [Fact]
    public async Task Count_ShouldReturnCorrectUniqueTitles()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Source = DataSource.Live, Achievement = new AchievementSummary { CurrentAchievements = 10, CurrentGamerscore = 1000 } },
            new Title { Name = "Game1", Source = DataSource.Xbox360, Achievement = new AchievementSummary { CurrentAchievements = 5, CurrentGamerscore = 500 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.Count();

        // Assert
        result.Should().NotBeNull();
        result.UniqueTitlesCount.Should().Be(1);
    }

    [Fact]
    public async Task RarestAchievements_ShouldReturnRarestAchievements()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.RarestAchievements();
        var array = result?.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([new RarestAchievementItem("Game1", "Achievement1", 1)]);
    }

    [Fact]
    public async Task MostComplete_ShouldReturnMostComplete()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50, CurrentGamerscore = 500, TotalGamerscore = 1000 } },
            new Title { Name = "Game2", Achievement = new AchievementSummary { ProgressPercentage = 75, CurrentGamerscore = 375, TotalGamerscore = 500 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.MostComplete();
        var array = result?.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new CompletenessItem("Game2", 375, 500, 75),
            new CompletenessItem("Game1", 500, 1000, 50)
        ]);
    }

    [Fact]
    public async Task SpentMostTimeWith_ShouldReturnSpentMostTimeWith()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50, CurrentGamerscore = 500, TotalGamerscore = 1000 } },
            new Title { Name = "Game2", Achievement = new AchievementSummary { ProgressPercentage = 75, CurrentGamerscore = 375, TotalGamerscore = 500 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.SpentMostTimeWith();
        var array = result?.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new MinutesPlayed("Game2", 120),
            new MinutesPlayed("Game1", 20)
        ]);
    }

    [Fact]
    public async Task WeightedRarity_ShouldReturnWeightedRarity()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50, TotalGamerscore = 200 } }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.WeightedRarity();
        var array = result?.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().BeEquivalentTo([
            new WeightedAchievementItem("Game1", new AchievementSummary { ProgressPercentage = 50, TotalGamerscore = 200 }, 1, 1, 1, 198609832.90254205)
        ]);
    }

    [Fact]
    public async Task Categories_ShouldReturnCategories()
    {
        // Arrange
        var titles = new[]
        {
            new Title { Name = "Game1", Category = "Action" },
            new Title { Name = "Game2", Category = "Adventure" },
            new Title { Name = "Game3", Category = "Action" }
        };
        Setup(titles);

        // Act
        var result = await _builtInQueries.Categories();
        var array = result?.ToArray();

        // Assert
        array.Should().NotBeNull();
        array.Should().HaveCount(2);
        array.Should().BeEquivalentTo([
            new CategorySlice("Action", 2),
            new CategorySlice("Adventure", 1)
        ]);
    }

    private void Setup(Title[] titles)
    {
        var achievements = new[]
        {
            new Achievement { Name = "Achievement1", Unlocked = true, Rarity = new Rarity { CurrentCategory = "Rare", CurrentPercentage = 1.0 }, Gamerscore = 100 }
        };
        var stats20 = new[]
        {
            new Stat { Name = "MinutesPlayed", Value = "20", Type = "Integer" }
        };

        var stats120 = new[]
        {
            new Stat { Name = "MinutesPlayed", Value = "120", Type = "Integer" }
        };

        _repositoryMock.Setup(r => r.LoadTitles(It.IsAny<bool>())).ReturnsAsync(titles);
        _repositoryMock.Setup(r => r.LoadAchievements(It.IsAny<Title>())).ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.LoadStats(It.IsAny<Title>())).ReturnsAsync((Title title) =>
        {
            var stats = title.Name == "Game1" ? stats20 : stats120;
            return title.Source == DataSource.Xbox360 ? [] : stats;
        });
    }
}