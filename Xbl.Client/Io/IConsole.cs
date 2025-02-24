using Spectre.Console;
using Spectre.Console.Rendering;

namespace Xbl.Client.Io;

public interface IConsole : IOutput
{
    void Markup(string text);
    void MarkupLine(string text);
    void MarkupInterpolated(FormattableString text);
    void MarkupLineInterpolated(FormattableString text);

    int ShowError(string error);
    void Write(IRenderable table);

    Task Progress(Func<ProgressContext, Task> action);
}