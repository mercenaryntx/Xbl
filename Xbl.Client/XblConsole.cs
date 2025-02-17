﻿using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client;

public class XblConsole : IOutput
{
    public void RarestAchievements(IEnumerable<RarestAchievementItem> data)
    {
        var table = new Table();
        table.AddColumn("[bold]No.[/]");
        table.AddColumn("[bold]Title[/]");
        table.AddColumn("[bold]Achievement[/]");
        table.AddColumn("[bold]Rarity[/]");

        var i = 0;
        foreach (var (title, achievement, currentPercentage) in data)
        {
            table.AddRow(
                $"[silver]{++i:D3}[/]", 
                $"[cyan1]{title}[/]", 
                $"[silver]{achievement}[/]", 
                $"[#16c60c]{currentPercentage:F}%[/]"
            );
        }
        AnsiConsole.Write(table);
    }

    public void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        var table = new Table();
        table.AddColumn("[bold]No.[/]");
        table.AddColumn("[bold]Title[/]");
        table.AddColumn("[bold]Gamerscore[/]");
        table.AddColumn("[bold]R/A/T[/]");
        table.AddColumn("[bold]Weight[/]");

        var i = 0;
        foreach (var (title, summary, totalCount, achievedCount, rareCount, weight) in weightedRarity)
        {
            table.AddRow(
                $"[silver]{++i:D3}[/]", 
                $"[cyan1]{title}[/]", 
                $"[silver]{summary.CurrentGamerscore}/{summary.TotalGamerscore}[/]", 
                $"[silver]{rareCount}/{achievedCount}/{totalCount}[/]", 
                $"[#16c60c]{weight:F}[/]"
            );
        }
        AnsiConsole.Write(table);
    }

    public void MostComplete(IEnumerable<Title> data)
    {
        var table = new Table();
        table.AddColumn("[bold]No.[/]");
        table.AddColumn("[bold]Title[/]");
        table.AddColumn("[bold]Gamerscore[/]");
        table.AddColumn("[bold]Progress[/]");

        var i = 0;
        foreach (var title in data)
        {
            table.AddRow(
                $"[silver]{++i:D3}[/]",
                $"[cyan1]{title.Name}[/]",
                $"[silver]{title.Achievement?.CurrentGamerscore ?? 0}/{title.Achievement?.TotalGamerscore ?? 0}[/]",
                $"[#16c60c]{title.Achievement?.ProgressPercentage ?? 0:F}%[/]"
            );
        }
        AnsiConsole.Write(table);
    }

    public void SpentMostTimeWith(IEnumerable<MinutesPlayed> data)
    {
        var chart = new BreakdownChart().Width(120);

        var exclude = new[] {"Default", "Black", "White", "Silver"};
        var type = typeof(Color);
        var colors = type.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(pi =>
                pi.PropertyType == type && !exclude.Contains(pi.Name) &&
                !pi.Name.Contains("grey", StringComparison.InvariantCultureIgnoreCase)).Select(pi => pi.GetValue(null))
            .Cast<Color>()
            .ToArray();

        var i = 0;
        foreach (var (title, played) in data)
        {
            chart.AddItem(title, Math.Round(TimeSpan.FromMinutes(played).TotalHours, 2), colors[i++]);
        }

        AnsiConsole.Write(chart);

        //var table = new Table();
        //table.AddColumn("[bold]No.[/]");
        //table.AddColumn("[bold]Title[/]");
        //table.AddColumn("[bold]Hours[/]");

        //var i = 0;
        //foreach (var (title, played) in data)
        //{
        //    table.AddRow(
        //        $"[silver]{++i:D3}[/]",
        //        $"[cyan1]{title}[/]",
        //        $"[silver]{TimeSpan.FromMinutes(played).TotalHours:#.0}[/]"
        //    );
        //}
        //AnsiConsole.Write(table);
    }

    public static Color GenerateColor(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        byte r = hash[0];
        byte g = hash[1];
        byte b = hash[2];
        return new Color(r, g, b);
    }
}