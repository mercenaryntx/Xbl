namespace Xbl.Client.Io;

public interface IXblClient
{
    Task<int> Update();
    Task GetGameDetails(IEnumerable<string[]> ids);
}