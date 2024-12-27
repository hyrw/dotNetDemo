using HttpClientDemo;
using Microsoft.Extensions.DependencyInjection;

var collection = new ServiceCollection();

collection
    .AddHttpClient()
    .ConfigureHttpClientDefaults(builder =>
    {
        builder
        .AddHttpMessageHandler<LogHandler>()
        .AddHttpMessageHandler<RetryHandler>();
    });

collection
    .AddTransient<GreetingMessageApi>()
    .AddTransient<GreetingMessageApiOptions>()
    .AddTransient<LogHandler>()
    .AddTransient<RetryHandler>();
//collection.AddHttpClient<GreetingMessageApi>()
//    .AddHttpMessageHandler<LogHandler>()
//    .AddHttpMessageHandler<RetryHandler>()
//    .UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2))
//    .SetHandlerLifetime(Timeout.InfiniteTimeSpan);


//collection
//.AddHttpClient<GreetingMessageApi>()
//.AddHttpMessageHandler<LogHandler>()
//.AddHttpMessageHandler<RetryHandler>()
//.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());

var provider = collection.BuildServiceProvider();
var greetingApi = provider.GetRequiredService<GreetingMessageApi>();
var greeting = await greetingApi.GetGreetingMessageAsync();
Console.WriteLine(greeting);
