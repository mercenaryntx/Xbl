namespace Xbl.Data.Entities;

public interface IJsonEntity
{
    DateTime UpdatedOn { get; set; }
    public string Data { get; set; }
}