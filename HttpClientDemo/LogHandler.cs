using System.Text.Json;
namespace HttpClientDemo;

public class LogHandler : DelegatingHandler
{
    public LogHandler() { }

    public LogHandler(HttpMessageHandler handler) : base(handler) { }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize<HttpRequestMessage>(request);
        Console.WriteLine(json);

        return base.SendAsync(request, cancellationToken);
    }
}
