using System.Diagnostics.CodeAnalysis;
using Spectre.Console;

namespace Xbl.Client.Infrastructure;

[ExcludeFromCodeCoverage]
public class ProgressContextWrapper : IProgressContext
{
    private readonly ProgressContext _context;

    public ProgressContextWrapper(ProgressContext context)
    {
        _context = context;
    }
    public ProgressTask AddTask(string description, double maxValue = 100) => _context.AddTask(description, true, maxValue);
}