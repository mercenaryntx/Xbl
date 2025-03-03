using Moq;
using Xbl.Client.Repositories;

namespace Xbl.Client.Tests.Extensions;

public static class MockExtensions
{
    public static void LoadJson<T, TResult>(this Mock<T> mock, Func<string, TResult> func) where T : class, IRepository
    {
        mock.Setup(r => r.LoadJson<TResult>(It.IsAny<string>())).ReturnsAsync(func);
    }

    public static void SaveJson<T>(this Mock<T> mock, Action<string, string> action) where T : class, IRepository
    {
        mock.Setup(r => r.SaveJson(It.IsAny<string>(), It.IsAny<string>())).Callback(action);
    }
}