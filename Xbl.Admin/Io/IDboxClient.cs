using Xbl.Client.Models.Dbox;

namespace Xbl.Admin.Io;

public interface IDboxClient
{
    Task<int> GetBaseline();
    Task<IEnumerable<Product>> GetDelta();
}