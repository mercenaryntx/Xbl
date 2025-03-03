namespace Xbl.Data;

public class DatabaseAttribute(params string[] databases) : Attribute
{
    public string[] Databases { get; } = databases;
}