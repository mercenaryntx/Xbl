using Xbl.Client.Models.Xbl.Marketplace;

namespace Xbl.Admin.Io;

public interface IXblClient
{
    Task<Dictionary<string, XblProduct>> GetGameDetails(IEnumerable<string> ids);
}