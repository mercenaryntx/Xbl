using System.Reflection;
using KustoLoco.Core;
using KustoLoco.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xbl.Client.Infrastructure;
using Xbl.Client.Models.Xbl;

namespace Xbl.Client.Io;

public class ConsoleOutput : IConsole
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
        var colors = GetColorScheme();

        var i = 0;
        foreach (var (title, played) in data)
        {
            chart.AddItem(title, Math.Round(TimeSpan.FromMinutes(played).TotalHours, 2), colors[i++]);
        }

        AnsiConsole.Write(chart);
    }

    public void Categories(IEnumerable<CategorySlice> slices)
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
            .Columns(new ProgressColumn[]
            {
                new SpinnerColumn(Spinner.Known.Dots2),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn()
            })
            .StartAsync(ctx => action(new ProgressContextWrapper(ctx)));
    }
}