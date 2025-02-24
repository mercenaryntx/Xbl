using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Spectre.Console;
using Spectre.Console.Json;
using Xbl.Client.Io;
using Xbl.Client.Models.Kql;

namespace Xbl.Client.Queries;

public class ExtendedHelp : IExtendedHelp
{
    private readonly IConsole _console;

    public ExtendedHelp(IConsole console)
    {
        _console = console;
    }

    public int Print()
    {
        var titles = PrintJsonPanel(GetTitleExample(), "titles");
        var stats = PrintJsonPanel(GetStatExample(), "stats");
        var achievements = PrintJsonPanel(GetAchievementExample(), "achievements");
        _console.Write(new Columns(titles, stats, achievements));
        return 0;
    }

    private static Panel PrintJsonPanel(object o, string header)
    {
        return new Panel(GetJsonText(o))
            .Header(header)
            .Collapse()
            .BorderColor(new Color(249, 251, 165));
    }

    private static KqlTitle GetTitleExample()
    {
        return new KqlTitle
        {
            TitleId = "1915865634",
            Name = "Lorem Ipsum: The Game",
            CompatibleWith = $"{Device.PC}|{Device.Mobile}|{Device.Xbox360}|{Device.XboxOne}|{Device.XboxSeries}",
            CurrentAchievements = 18,
            TotalAchievements = 0,
            CurrentGamerscore = 560,
            TotalGamerscore = 1000,
            ProgressPercentage = 56,
            LastTimePlayed = DateTime.Parse("2025-02-02T15:54:38.4848326Z"),
            Category = "Shooter",
            Xbox360ProductId = "66acd000-77fe-1000-9115-d802305807d3",
            Xbox360ReleaseDate = DateTime.Parse("2020-01-01T00:00:00.0000000Z"),
            XboxOneProductId = "9MVHHT21X5F6",
            XboxOneReleaseDate = DateTime.Parse("2020-01-01T00:00:00.0000000Z"),
            XboxSeriesProductId = "bx3zk30jl2dl",
            XboxSeriesReleaseDate = DateTime.Parse("2020-01-01T00:00:00.0000000Z"),
        };
    }

    private static KqlMinutesPlayed GetStatExample()
    {
        return new KqlMinutesPlayed
        {
            XUID = "2533274845176708",
            SCID = "40950100-bb3f-4769-906a-5eae3b67330a",
            TitleId = "996619018",
            Minutes = 341
        };
    }

    private static KqlAchievement GetAchievementExample()
    {
        return new KqlAchievement
        {
            Name = "Lorem ipsum",
            TitleId = "10027721",
            TitleName = "dolor sit amet",
            IsUnlocked = true,
            TimeUnlocked = DateTime.Parse("2016-09-11T12:25:10.6860000Z"),
            Platform = "Xbox360|XboxOne|XboxSeries",
            Description = "Lorem ipsum",
            LockedDescription = "dolor sit amet",
            Gamerscore = 100,
            IsRare = false,
            RarityPercentage = 12.34,
        };
    }

    private static JsonText GetJsonText(object o)
    {
        var scalar = new Color(22, 198, 12);
        var member = Color.Cyan1;
        var str = Color.Cyan3;
        var colon = new Color(249, 251, 165);

        var jso = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    typeInfo =>
                    {
                        foreach (var property in typeInfo.Properties)
                        {
                            var pi = property.AttributeProvider as PropertyInfo;
                            if (pi != null) property.Name = pi.Name;
                        }
                    }
                }
            }
        };

        return new JsonText(JsonSerializer.Serialize(o, jso))
        {
            BooleanStyle = scalar,
            NumberStyle = scalar,
            BracesStyle = Color.Silver,
            BracketsStyle = Color.Silver,
            MemberStyle = member,
            ColonStyle = colon,
            CommaStyle = Color.Silver,
            StringStyle = str,
            NullStyle = Color.Silver
        };
    }
}