using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using KustoLoco.Core;
using KustoLoco.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xbl.Client.Infrastructure;
using Xbl.Client.Models;

namespace Xbl.Client.Io;

[ExcludeFromCodeCoverage]
public class ConsoleOutput : IConsole
{
    public void Render(ProfilesSummary data)
    {
        var table = new Table();
        table.AddColumn("[bold]Profile[/]");
        table.AddColumn("[bold]Games[/]", c =>
        {
            c.Alignment = Justify.Right;
            c.Footer($"{data.Profiles.Sum(p => p.Games)}|{data.UniqueTitlesCount}");

        });
        table.AddColumn("[bold]Achievements[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Gamerscore[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Hours played[/]", c => c.Alignment = Justify.Right);

        RenderProfile(table, data.Profiles[0], "Xbox Live", "green3_1");
        if (data.Profiles.Length > 1)
        {
            RenderProfile(table, data.Profiles[1], "Xbox 360", "cyan1");
            table.ShowFooters = true;
        }
        AnsiConsole.Write(table);
    }

    private static void RenderProfile(Table table, ProfileSummary summary, string prefix, string color)
    {
        var profile = $"[{color}]{prefix}[/]";
        var c1 = $"[{color}]{summary.Games}[/]";
        var count = $"[{color}]{summary.Achievements}[/]";
        var sum = $"[{color}]{summary.Gamerscore}[/]";
        var hours = $"[{color}]{summary.MinutesPlayed.TotalHours:0.0}[/]";

        table.AddRow(profile, c1, count, sum, hours);
    }

    public void Render(IEnumerable<RarestAchievementItem> data)
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

    public void Render(IEnumerable<WeightedAchievementItem> weightedRarity)
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

    public void Render(IEnumerable<CompletenessItem> data)
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
                $"[silver]{title.CurrentGamerscore}/{title.TotalGamerscore}[/]",
                $"[#16c60c]{title.ProgressPercentage:F}%[/]"
            );
        }
        AnsiConsole.Write(table);
    }

    public void Render(IEnumerable<MinutesPlayed> data)
    {
        var chart = new BreakdownChart().Width(120);
        var colors = GetColorScheme();

        var i = 0;
        foreach (var (title, played) in data)
        {
            chart.AddItem(title, Math.Round(TimeSpan.FromMinutes(played).TotalHours, 2), colors[i++]);
        }

        AnsiConsole.Write(chart);
    }

    public void Render(IEnumerable<CategorySlice> slices)
    {
        var chart = new BreakdownChart().Width(120);
        var colors = GetColorScheme();

        var i = 0;
        foreach (var (category, count) in slices)
        {
            chart.AddItem(category, count, colors[i++]);
        }

        AnsiConsole.Write(chart);
    }

    public void KustoQueryResult(KustoQueryResult result)
    {
        var table = new Table();
        foreach (var column in result.ColumnDefinitions()) table.AddColumn(column.Name);

        foreach (var row in result.EnumerateRows())
        {
            var rowCells = row.Select(CellToString).ToArray();
            table.AddRow(rowCells.Select(cell => new Markup(cell.EscapeMarkup())));
        }

        AnsiConsole.Write(table);

        KustoResultRenderer.RenderChartInBrowser(result);
    }

    private static string CellToString(object cell)
    {
        return cell?.ToString() ?? "<null>";
    }

    private static Color[] GetColorScheme()
    {
        var exclude = new[] { "Default", "Black", "White", "Silver" };
        var type = typeof(Color);
        return type.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(pi =>
                pi.PropertyType == type && !exclude.Contains(pi.Name) &&
                !pi.Name.Contains("grey", StringComparison.InvariantCultureIgnoreCase)).Select(pi => pi.GetValue(null))
            .Cast<Color>()
            .ToArray();
    }

    public void Markup(string text)
    {
        AnsiConsole.Markup(text);
    }

    public void MarkupLine(string text)
    {
        AnsiConsole.MarkupLine(text);
    }

    public void MarkupInterpolated(FormattableString text)
    {
        AnsiConsole.MarkupInterpolated(text);
    }

    public void MarkupLineInterpolated(FormattableString text)
    {
        AnsiConsole.MarkupLineInterpolated(text);
    }

    public int ShowError(string error)
    {
        AnsiConsole.MarkupLine("[red]Error:[/] " + error);
        return -1;
    }

    public void Write(IRenderable table)
    {
        AnsiConsole.Write(table);
    }

    public Task Progress(Func<IProgressContext, Task> action)
    {
        return AnsiConsole
            .Progress()
            .AutoClear(false)
            .Columns(new SpinnerColumn(Spinner.Known.Dots2), new TaskDescriptionColumn { Alignment = Justify.Left }, new ProgressBarColumn(), new PercentageColumn())
            .StartAsync(ctx => action(new ProgressContextWrapper(ctx)));
    }
}