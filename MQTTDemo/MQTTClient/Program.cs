using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

await Task.Delay(TimeSpan.FromSeconds(1));

var mqttFactory = new MqttFactory();

using (var client = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithClientId("publisher")
        .WithCredentials("mqtt_client", "mqtt_password")
        .WithTcpServer("localhost")
        .Build();

    await client.ConnectAsync(mqttClientOptions);

    Random random = new();
    MqttApplicationMessage message;
    MqttClientPublishResult result;
    foreach (var item in Enumerable.Range(1, 5))
    {
        long temperature = random.NextInt64();
        message = new MqttApplicationMessageBuilder()
            .WithTopic("temperature")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithPayload(temperature.ToString())
            .Build();
        result = await client.PublishAsync(message);
        Console.WriteLine($"MQTT 发布消息: {temperature} {result.IsSuccess}");
    }

    await Task.Delay(TimeSpan.FromSeconds(3));

    message = new MqttApplicationMessageBuilder()
        .WithTopic("temperature")
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
        .WithPayload("test")
        .Build();
    result = await client.PublishAsync(message);
    Console.WriteLine($"MQTT 发布消息: {Encoding.UTF8.GetString(message.PayloadSegment)} {result.IsSuccess}");

    Console.WriteLine("MQTT 所有消息发送完毕。");
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
    await client.DisconnectAsync();
}
