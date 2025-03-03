using AutoMapper;
using FluentAssertions;
using Moq;
using Xbl.Client.Io;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Queries;
using Xbl.Client.Repositories;
using Xunit;

namespace Xbl.Client.Tests.Queries;

public class KustoQueryExecutorTests
{
    private Mock<IXblRepository> _repositoryMock;
    private Mock<IConsole> _consoleMock;
    private Mock<IMapper> _mapperMock;
    private Settings _settings;
    private KustoQueryExecutor _kustoQueryExecutor;

    [Fact]
    public async Task RunKustoQuery_ShouldReturnKustoQueryResult()
    {
        // Arrange
        Setup("query");

        // Act
        var result = await _kustoQueryExecutor.RunKustoQuery();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunKustoQuery_ShouldHandleKqlFile()
    {
        // Arrange
        const string kql = "test.kql";
        const string kqlContent = "query";
        Setup(kql);
        await File.WriteAllTextAsync(kql, kqlContent);

        // Act
        var result = await _kustoQueryExecutor.RunKustoQuery();

        // Assert
        result.Should().NotBeNull();

        // Cleanup
        File.Delete(kql);
    }

    [Fact]
    public async Task RunKustoQuery_ShouldHandleMissingKqlFile()
    {
        // Arrange
        var kql = "missing.kql";
        Setup(kql);

        // Act
        var result = await _kustoQueryExecutor.RunKustoQuery();

        // Assert
        result.Should().BeNull();
        _consoleMock.Verify(c => c.ShowError("KQL file cannot be found"), Times.Once);
    }

    private void Setup(string kql)
    {
        var titles = new[]
        {
            new Title { Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50 } }
        };
        var achievements = new[]
        {
            new Achievement { Name = "Achievement1", Unlocked = true, Rarity = new Rarity { CurrentPercentage = 1.0 }, Gamerscore = 100 }
        };
        var stats = new[]
        {
            new Stat { Name = "MinutesPlayed", Value = "120", Type = "Integer" }
        };
        var kqlAchievement = new KqlAchievement();
        var kqlMinutesPlayed = new KqlMinutesPlayed();
        var kqlTitle = new KqlTitle();

        _repositoryMock = new Mock<IXblRepository>();
        _repositoryMock.Setup(r => r.LoadTitles(It.IsAny<bool>())).ReturnsAsync(titles);
        _repositoryMock.Setup(r => r.LoadAchievements(It.IsAny<Title>())).ReturnsAsync(achievements);
        _repositoryMock.Setup(r => r.LoadStats(It.IsAny<Title>())).ReturnsAsync(stats);

        _mapperMock = new Mock<IMapper>();
        _mapperMock.Setup(m => m.Map<KqlAchievement>(It.IsAny<Achievement>())).Returns(kqlAchievement);
        _mapperMock.Setup(m => m.Map<KqlMinutesPlayed>(It.IsAny<Stat>())).Returns(kqlMinutesPlayed);
        _mapperMock.Setup(m => m.Map<KqlTitle>(It.IsAny<Title>())).Returns(kqlTitle);

        _consoleMock = new Mock<IConsole>();
        _settings = new Settings { KustoQuery = kql };
        _kustoQueryExecutor = new KustoQueryExecutor(_settings, _repositoryMock.Object, _consoleMock.Object, _mapperMock.Object);
    }
}