namespace HttpClientDemo;

public class RetryHandler : DelegatingHandler
{
    public RetryHandler() { }

    public RetryHandler(HttpMessageHandler handler) : base(handler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // retry 3 times
        for (int i = 0; i < 3; i++)
        {
            try
            {
                var responseMessage = await base.SendAsync(request, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
                return responseMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Retry {i + 1} failed: {ex.Message}");
            }
        }
        // throw an exception if all retries fail
        throw new HttpRequestException("Failed to send request after 3 retries.");
    }
}
