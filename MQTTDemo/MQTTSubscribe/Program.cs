using MQTTnet;
using MQTTnet.Client;
using System.Text;

var mqttFactory = new MqttFactory();

using (var client = mqttFactory.CreateMqttClient())
{
    var clientOptions = new MqttClientOptionsBuilder()
        .WithCleanSession(false)
        .WithClientId("subscriber")
        .WithCredentials("mqtt_client", "mqtt_password")
        .WithTcpServer("localhost")
        .Build();

    client.ApplicationMessageReceivedAsync += Client_ApplicationMessageReceivedAsync;

    await client.ConnectAsync(clientOptions, CancellationToken.None);

    var subscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(filter => filter.WithTopic("temperature"))
        .Build();

    await client.SubscribeAsync(subscribeOptions);
    Console.WriteLine("MQTT client subscribed to topic.");

    await Task.Delay(TimeSpan.FromSeconds(1));
    await client.DisconnectAsync();
    await Task.Delay(TimeSpan.FromSeconds(1));
    await client.ConnectAsync(clientOptions);
    
    Console.WriteLine("Press enter to exit.");
    await client.DisconnectAsync();
    Console.ReadLine();
}

Task Client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
{
    Console.WriteLine($"接收到消息: {e.ClientId} {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
    return Task.CompletedTask;
}