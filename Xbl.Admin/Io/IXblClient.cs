using Xbl.Client.Models.Xbl.Marketplace;

namespace Xbl.Admin.Io;

public interface IXblClient
{
    Task<Dictionary<string, XblProduct>> GetGameDetailsByProductId(IEnumerable<string> ids);
    Task<Dictionary<string, XblProduct>> GetGameDetailsByTitleId(string titleId);
}