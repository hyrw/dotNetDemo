namespace HttpClientDemo;

public class HttpHandlerBuilder
{
    readonly List<DelegatingHandler> handlers = [];

    public HttpHandlerBuilder AddHandler(DelegatingHandler handler)
    {
        handlers.Add(handler);
        return this;
    }

    public HttpMessageHandler Build() => Build(new SocketsHttpHandler());

    public HttpMessageHandler Build(SocketsHttpHandler httpClientHandler)
    {
        var first = handlers.FirstOrDefault();
        if (first is null) return httpClientHandler;

        var lastHandler = handlers.Aggregate((x, y) =>
        {
            x.InnerHandler = y;
            return y;
        });
        lastHandler.InnerHandler = httpClientHandler;
        return handlers[0];
    }
}
