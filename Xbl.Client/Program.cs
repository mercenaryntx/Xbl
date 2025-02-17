using Spectre.Console;
using Spectre.Console.Cli;

namespace Xbl.Client;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        AnsiConsole.MarkupLine("[white]Xbox Achievement Stats v1.0[/]");
        Console.WriteLine();

        var app = new CommandApp<XblApp>();
        app.Configure(c =>
        {
            c.SetApplicationName("xbl");
            c.AddExample("-a={apiKey} -u");
            c.AddExample("-r");
            c.AddExample("-p=E00001D5D85ED487 -m");
        });

        args = args.Length == 0 
            ? new[] {"-h"} 
            : args.Select(a => a == "-u" ? "-u=all" : a).ToArray();

        return await app.RunAsync(args);

    }
}