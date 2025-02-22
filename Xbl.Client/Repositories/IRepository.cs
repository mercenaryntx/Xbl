namespace Xbl.Client.Repositories;

public interface IRepository
{
    Task<T> LoadJson<T>(string path);
    Task SaveJson(string path, string json);
    Task SaveJson(string path, object data);
}