using DotNetEnv;
using Xbl.Client.Io;

namespace Xbl.Client.Integration.Tests.Io;

public class XblApiFixture
{
    public XblApiClientV2 Api { get; }
    public string Xuid { get; }
    public string TitleId { get; }
    public string X360TitleId { get; }

    public XblApiFixture()
    {
        Env.TraversePath().Load();

        var apiKey = Environment.GetEnvironmentVariable("XBL_API_KEY")
            ?? throw new InvalidOperationException("Environment variable XBL_API_KEY is not set.");
        Xuid = Environment.GetEnvironmentVariable("XBL_XUID")
            ?? throw new InvalidOperationException("Environment variable XBL_XUID is not set.");
        TitleId = Environment.GetEnvironmentVariable("XBL_TITLE_ID")
            ?? throw new InvalidOperationException("Environment variable XBL_TITLE_ID is not set.");
        X360TitleId = Environment.GetEnvironmentVariable("XBL_X360_TITLE_ID")
            ?? throw new InvalidOperationException("Environment variable XBL_X360_TITLE_ID is not set.");

        var client = new HttpClient
        {
            BaseAddress = new Uri("https://xbl.io/api/")
        };
        client.DefaultRequestHeaders.Add("x-authorization", apiKey);

        Api = new XblApiClientV2(client);
    }
}
