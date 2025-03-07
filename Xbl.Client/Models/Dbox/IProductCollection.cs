namespace Xbl.Client.Models.Dbox;

public interface IProductCollection<T>
{
    T[] Products { get; set; }
    int Count { get; set; }
}