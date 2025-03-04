namespace Xbl.Data.Entities;

public interface IHaveStringId : IHaveId
{
    string Id { get; }
}