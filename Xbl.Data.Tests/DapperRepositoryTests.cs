using FluentAssertions;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Data.Repositories;
using Xunit;

namespace Xbl.Data.Tests;

public class DapperRepositoryTests
{
    private DatabaseContext _db;
    private Title[] _titles;
    private IRepository<Title> _repository;

    [Fact]
    public async Task AsQueryable_Where()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Where(t => t.Name == "Game1").ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Where(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_WhereWithAnd()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Where(t => t.Name == "Game1" && t.IntId == "123").ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Where(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_WhereWithOr()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Where(t => t.Name == "Game1" || t.IntId == "123").ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Where(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_WhereMerge()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Where(t => t.Name == "Game1").Where(t => t.IntId == "123").ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Where(t => t.Name == "Game1"));
    }

    [Fact(Skip = "Projection is not supported yet")]
    public async Task AsQueryable_Select()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Select(t => t.Name).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Select(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderBy()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderBy(t => t.Name).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderBy(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderByThenBy()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderBy(t => t.Name).ThenBy(t => t.IntId).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderBy(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderByThenByDescending()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderBy(t => t.Name).ThenByDescending(t => t.IntId).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderBy(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderByDescending()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderByDescending(t => t.Name).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderByDescending(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderByDescendingThenBy()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderByDescending(t => t.Name).ThenBy(t => t.IntId).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderByDescending(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_OrderByDescendingThenByDescending()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().OrderByDescending(t => t.Name).ThenByDescending(t => t.IntId).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.OrderByDescending(t => t.Name));
    }

    [Fact]
    public async Task AsQueryable_First()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().First(t => t.Name == "Game1");

        // Assert
        result.Should().BeEquivalentTo(_titles.First(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_FirstOrDefault()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().FirstOrDefault(t => t.Name == "Game1");

        // Assert
        result.Should().BeEquivalentTo(_titles.FirstOrDefault(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_Single()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Single(t => t.Name == "Game1");

        // Assert
        result.Should().BeEquivalentTo(_titles.Single(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_SingleOrDefault()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().SingleOrDefault(t => t.Name == "Game1");

        // Assert
        result.Should().BeEquivalentTo(_titles.SingleOrDefault(t => t.Name == "Game1"));
    }

    [Fact]
    public async Task AsQueryable_Count()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Count();

        // Assert
        result.Should().Be(_titles.Length);
    }

    [Fact]
    public async Task AsQueryable_Any()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Any(t => t.Name == "Game1");

        // Assert
        result.Should().Be(_titles.Any(t => t.Name == "Game1"));
    }

    [Fact(Skip = "All is not supported yet")]
    public async Task AsQueryable_All()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().All(t => t.Name.Contains("Game"));

        // Assert
        result.Should().Be(_titles.All(t => t.Name.Contains("Game")));
    }

    [Fact]
    public async Task AsQueryable_Skip()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Skip(1).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Skip(1));
    }

    [Fact]
    public async Task AsQueryable_Take()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().Take(1).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Take(1));
    }

    [Fact]
    public async Task AsQueryable_DistinctBy()
    {
        // Arrange
        await Setup();

        // Act
        var result = _repository.AsQueryable().DistinctBy(t => t.IntId).ToArray();

        // Assert
        result.Should().BeEquivalentTo(_titles.Select(t => new Title { IntId = t.IntId}));
    }

    private async Task Setup()
    {
        _titles =
        [
            new Title { IntId = "123", Name = "Game1", Achievement = new AchievementSummary { ProgressPercentage = 50 } },
            new Title { IntId = "124", Name = "Game2", Achievement = new AchievementSummary { ProgressPercentage = 75 } }
        ];

        MicroOrmConfig.SqlProvider = SqlProvider.SQLite;
        _db = new DatabaseContext();
        _repository = await _db.GetRepository<Title>();
        await _repository.BulkInsert(_titles);
    }
}
