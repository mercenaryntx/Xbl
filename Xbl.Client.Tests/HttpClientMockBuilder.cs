using Moq;
using Moq.Protected;

namespace Xbl.Client.Tests;

public class HttpClientMockBuilder(string baseAddress)
{
    private readonly Uri _baseAddress = new(baseAddress);

    private readonly Dictionary<Uri, Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public HttpClientMockBuilder AddResponse(string url, HttpResponseMessage response)
    {
        var uri = new Uri(_baseAddress, url);
        _responses[uri] = _ => response;
        return this;
    }

    public HttpClientMockBuilder AddResponse(string url, Func<HttpRequestMessage, HttpResponseMessage> response)
    {
        var uri = new Uri(_baseAddress, url);
        _responses[uri] = response;
        return this;
    }

    public HttpClient Build()
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                if (_responses.TryGetValue(req.RequestUri!, out var func)) return func(req);
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                    {Content = new StringContent("wrong")};
            });

        return new HttpClient(mock.Object)
        {
            BaseAddress = _baseAddress
        };
    }
}