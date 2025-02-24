using Spectre.Console;

namespace Xbl.Client.Infrastructure;

public class ProgressContextWrapper : IProgressContext
{
    private readonly ProgressContext _context;

    public ProgressContextWrapper(ProgressContext context)
    {
        _context = context;
    }
    public ProgressTask AddTask(string description, double maxValue = 100) => _context.AddTask(description, true, maxValue);
}