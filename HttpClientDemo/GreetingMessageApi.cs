namespace HttpClientDemo;

public class GreetingMessageApi(IHttpClientFactory factory, GreetingMessageApiOptions options)
{
    readonly HttpClient client = factory.CreateClient("GreetingMessageApi");
    readonly GreetingMessageApiOptions options = options;

    public async Task<string> GetGreetingMessageAsync()
    {
        try
        {
            var response = await client.GetAsync($"{options.BaseAddress}/api/getGreetingMessage?type=string");
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            Console.WriteLine("Failed to get greeting message.");
        }
        return string.Empty;
    }
}
