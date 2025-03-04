using AutoMapper;
using FluentAssertions;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Moq;
using Xbl.Client.Io;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Queries;
using Xbl.Data;
using Xunit;

namespace Xbl.Client.Tests.Queries;

public class KustoQueryExecutorTests
{
    private DatabaseContext _db;
    private Mock<IConsole> _consoleMock;
    private Mock<IMapper> _mapperMock;
    private Settings _settings;
    private KustoQueryExecutor _kustoQueryExecutor;

    [Fact]
    public async Task RunKustoQuery_ShouldReturnKustoQueryResult()
    {
        // Arrange
        await Setup("titles");

        // Act
        var result = await _kustoQueryExecutor.RunKustoQuery();

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task RunKustoQuery_ShouldHandleKqlFile()
    {
        // Arrange
        const string kql = "test.kql";
        const string kqlContent = "titles";
        await Setup(kql);
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
        await Setup(kql);

        // Act
        var result = await _kustoQueryExecutor.RunKustoQuery();

        // Assert
        result.Should().BeNull();
        _consoleMock.Verify(c => c.ShowError("KQL file cannot be found"), Times.Once);
    }

    private async Task Setup(string kql)
    {
        if (_db == null)
        {
            var titles = new[]
            {
                new Title { IntId = "123", Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50 } }
            };
            var achievements = new[]
            {
                new Achievement { Id = 1, TitleId = 123, Name = "Achievement1", Unlocked = true, Rarity = new Rarity { CurrentPercentage = 1.0 }, Gamerscore = 100 }
            };
            var stats = new[]
            {
                new Stat { TitleId = "123", Name = "MinutesPlayed", Value = "120", Type = "Integer" }
            };
            var kqlAchievement = new KqlAchievement();
            var kqlMinutesPlayed = new KqlMinutesPlayed();
            var kqlTitle = new KqlTitle();

            _db = new DatabaseContext();
            var t = await _db.GetRepository<Title>();
            await t.BulkInsert(titles);

            var a = await _db.GetRepository<Achievement>();
            await a.BulkInsert(achievements);

            var s = await _db.GetRepository<Stat>();
            await s.BulkInsert(stats);

            _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(m => m.Map<KqlAchievement>(It.IsAny<Achievement>())).Returns(kqlAchievement);
            _mapperMock.Setup(m => m.Map<KqlMinutesPlayed>(It.IsAny<Stat>())).Returns(kqlMinutesPlayed);
            _mapperMock.Setup(m => m.Map<KqlTitle>(It.IsAny<Title>())).Returns(kqlTitle);

            _consoleMock = new Mock<IConsole>();
        }

        _settings = new Settings { KustoQuery = kql };
        _kustoQueryExecutor = new KustoQueryExecutor(_settings, _consoleMock.Object, _mapperMock.Object, _db, _db);
    }
}