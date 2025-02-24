using Spectre.Console;

namespace Xbl.Client.Infrastructure;

public interface IProgressContext
{
    ProgressTask AddTask(string description, double maxValue);
}