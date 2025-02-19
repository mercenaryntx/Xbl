﻿using System.Text.Json;
using Xbl.Client.Models;
using Xbl.Xbox360.Extensions;
using Xbl.Xbox360.Io.Gpd;
using Xbl.Xbox360.Io.Stfs;
using Xbl.Xbox360.Io.Stfs.Data;
using Xbl.Xbox360.Models;

namespace Xbl.Client.Io;

public class Xbox360ProfileImporter : IXbox360ProfileImporter
{
    private readonly Settings _settings;
    private readonly IJsonRepository _repository;
    private readonly IConsole _console;

    public Xbox360ProfileImporter(Settings settings, IJsonRepository repository, IConsole console)
    {
        _settings = settings;
        _repository = repository;
        _console = console;
    }

    public async Task<int> Import()
    {
        _console.MarkupInterpolated($"[white]Importing Xbox 360 profile...[/] ");
        var cursor = Console.GetCursorPosition();
        _console.MarkupLine("[#f9f1a5]0%[/]");

        try
        {
            var profilePath = _settings.ProfilePath;
            var bytes = await File.ReadAllBytesAsync(profilePath);
            var profile = ModelFactory.GetModel<StfsPackage>(bytes);
            profile.ExtractGames();

            var profileHex = Path.GetFileName(profilePath);
            var titles = new AchievementTitles
            {
                Xuid = profileHex,
                Titles = GetTitlesFromProfile(profile)
            };

            var json = JsonSerializer.Serialize(titles);
            await _repository.SaveJson(_repository.GetTitlesFilePath(Constants.Xbox360), json);

            var i = 0;
            var n = 0;
            foreach (var (fileEntry, game) in profile.Games)
            {
                game.Parse();

                var achievements = new TitleDetails<Achievement>
                {
                    Achievements = GetAchievementsFromGameFile(fileEntry, game, out var hadBug)
                };
                if (hadBug) n++;

                json = JsonSerializer.Serialize(achievements);
                await _repository.SaveJson(_repository.GetAchievementFilePath(Constants.Xbox360, game.TitleId), json);

                Console.SetCursorPosition(cursor.Left, cursor.Top);
                _console.MarkupLineInterpolated($"[#f9f1a5]{++i * 100 / profile.Games.Count}%[/]");
            }

            for (i = 0; i < n; i++)
            {
                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            return _console.ShowError($"[silver]Failed to import. {ex.Message}[/]");
        }
    }

    private static Title[] GetTitlesFromProfile(StfsPackage profile)
    {
        return profile
            .ProfileInfo
            .TitlesPlayed
            .Where(g => !string.IsNullOrEmpty(g.TitleName))
            .Select(g =>
            {
                var t = new Title
                {
                    TitleId = BitConverter.ToInt32(g.TitleCode.FromHex()).ToString(),
                    HexId = g.TitleCode,
                    Name = g.TitleName,
                    Type = "Game",
                    IsXbox360 = true,
                    Achievement = new AchievementSummary
                    {
                        CurrentAchievements = g.AchievementsUnlocked,
                        CurrentGamerscore = g.GamerscoreUnlocked,
                        ProgressPercentage = g.TotalGamerScore > 0 ? 100 * g.GamerscoreUnlocked / g.TotalGamerScore : 0,
                        TotalGamerscore = g.TotalGamerScore,
                        TotalAchievements = g.AchievementCount
                    }
                };

                return t;

            })
            .ToArray();
    }

    private Achievement[] GetAchievementsFromGameFile(FileEntry fileEntry, GameFile game, out bool hadBug)
    {
        var bugReported = false;
        var achievements = game.Achievements.Select(a =>
        {
            try
            {
                if (a.Gamerscore < 0 || a.AchievementId < 0) throw new Exception();

                return new Achievement
                {
                    Id = a.AchievementId,
                    Name = a.Name,
                    TitleId = BitConverter.ToInt32(game.TitleId.FromHex()),
                    TitleName = game.Title,
                    Unlocked = a.IsUnlocked,
                    TimeUnlocked = a.IsUnlocked ? a.UnlockTime : DateTime.MinValue,
                    Platform = "Xbox360",
                    IsSecret = a.IsSecret,
                    Description = a.UnlockedDescription,
                    LockedDescription = a.LockedDescription,
                    Gamerscore = a.Gamerscore
                };
            }
            catch
            {
                if (bugReported) return null;
                _console.MarkupLineInterpolated($"[#f9fba5]Warning:[/] {game.Title} ([grey]{fileEntry.Name}[/]) is corrupted. Invalid entries are omitted.");
                bugReported = true;
                return null;
            }
        }).Where(a => a != null).ToArray();

        hadBug = bugReported;
        return achievements;
    }

}