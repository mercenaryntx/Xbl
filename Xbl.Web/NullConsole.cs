using KustoLoco.Core;
using Spectre.Console.Rendering;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models;

namespace Xbl.Web;

public class NullConsole : IConsole
{
    public void Render(ProfilesSummary summary)
    {
    }

    public void Render(IEnumerable<RarestAchievementItem> data)
    {
    }

    public void Render(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
    }

    public void Render(IEnumerable<CompletenessItem> data)
    {
    }

    public void Render(IEnumerable<MinutesPlayed> data)
    {
    }

    public void Render(IEnumerable<CategorySlice> slices)
    {
    }

    public void KustoQueryResult(KustoQueryResult result)
    {
    }

    public void Markup(string text)
    {
    }

    public void MarkupLine(string text)
    {
    }

    public void MarkupInterpolated(FormattableString text)
    {
    }

    public void MarkupLineInterpolated(FormattableString text)
    {
    }

    public int ShowError(string error)
    {
        return 0;
    }

    public void Write(IRenderable table)
    {
    }

    public Task Progress(Func<IProgressContext, Task> action)
    {
        return action.Invoke(new NullProgressContext());
    }
}