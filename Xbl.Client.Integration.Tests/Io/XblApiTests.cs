using FluentAssertions;
using FluentAssertions;
using FluentAssertions.Execution;
using Xbl.Client.Io;
using Xbl.Client.Models.Xbl.Player;
using Xunit;

namespace Xbl.Client.Integration.Tests.Io;

/// <summary>
/// Integration tests for <see cref="XblApiClientV2"/> that call the real xbl.io API.
/// Requires the following environment variables (or .env file) to be set:
///   XBL_API_KEY       — your OpenXBL API key
///   XBL_XUID         — your Xbox Live user ID
///   XBL_TITLE_ID     — a modern title ID you own achievements for (decimal)
///   XBL_X360_TITLE_ID — an Xbox 360 title ID you own achievements for (decimal)
/// </summary>
public class XblApiTests : IClassFixture<XblApiFixture>
{
    private readonly XblApiClientV2 _api;
    private readonly string _xuid;
    private readonly string _titleId;
    private readonly string _x360TitleId;

    public XblApiTests(XblApiFixture fixture)
    {
        _api = fixture.Api;
        _xuid = fixture.Xuid;
        _titleId = fixture.TitleId;
        _x360TitleId = fixture.X360TitleId;
    }

    [Fact]
    public async Task GetTitles_ShouldReturnWellFormedResponse()
    {
        var result = await _api.GetTitles();

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Xuid.Should().NotBeNullOrEmpty();
        result.Titles.Should().NotBeNullOrEmpty();
        result.Titles.Should().AllSatisfy(t =>
        {
            t.IntId.Should().NotBeNullOrEmpty();
            t.Name.Should().NotBeNullOrEmpty();
            t.TitleHistory.Should().NotBeNull();
            t.Achievement.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task GetAchievements_ShouldReturnWellFormedResponse()
    {
        var result = await _api.GetAchievements(_titleId);

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(a =>
        {
            a.Id.Should().NotBeNullOrEmpty();
            a.Name.Should().NotBeNullOrEmpty();
            a.TitleAssociations.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Get360Achievements_ShouldReturnWellFormedResponse()
    {
        var result = await _api.Get360Achievements(_xuid, _x360TitleId);

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(a =>
        {
            a.Id.Should().BeGreaterThan(0);
            a.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetMarketplaceDetails_ShouldReturnWellFormedResponse()
    {
        var result = await _api.GetMarketplaceDetails(_titleId);

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(p =>
        {
            p.ProductId.Should().NotBeNullOrEmpty();
            p.LocalizedProperties.Should().NotBeNullOrEmpty();
            p.Properties.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task GetPlayerStats_ShouldReturnWellFormedResponse()
    {
        var request = new PlayerStatsRequest
        {
            XUIDs = [_xuid],
            Stats = [new Stat { Name = "MinutesPlayed", TitleId = _titleId }]
        };

        var result = await _api.GetPlayerStats(request);

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.StatListsCollection.Should().NotBeNullOrEmpty();
        result.StatListsCollection[0].Stats.Should().NotBeNullOrEmpty();
    }
}
