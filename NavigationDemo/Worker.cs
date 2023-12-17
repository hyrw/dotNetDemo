using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace NavigationDemo
{
    public class Worker : BackgroundService
    {
        private readonly ChannelWriter<int> channelWriter;
        private readonly Random random;

        public Worker(ChannelWriter<int> channelWriter)
        {
            this.channelWriter = channelWriter;
            random = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                await channelWriter.WriteAsync(random.Next(), stoppingToken);
            }
        }
    }
}
