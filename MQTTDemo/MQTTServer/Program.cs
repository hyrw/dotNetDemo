using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

var mqttFactory = new MqttFactory();
var serverOption = new MqttServerOptionsBuilder()
    .WithDefaultEndpoint()
    .Build();

using (var mqttServer = mqttFactory.CreateMqttServer(serverOption))
{
    mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
    mqttServer.ClientAcknowledgedPublishPacketAsync += MqttServer_ClientAcknowledgedPublishPacketAsync;
    mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
    mqttServer.ApplicationMessageEnqueuedOrDroppedAsync += MqttServer_ApplicationMessageEnqueuedOrDroppedAsync;
    mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;

    await mqttServer.StartAsync();

    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();

    await mqttServer.StopAsync();
}

Task MqttServer_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
{
    Console.WriteLine($"客户端断开连接 {arg.Endpoint} {arg.ClientId} {arg.DisconnectType} ");
    return Task.CompletedTask;
}

Task MqttServer_ApplicationMessageEnqueuedOrDroppedAsync(ApplicationMessageEnqueuedEventArgs arg)
{
    Console.WriteLine($"消息将被丢弃？{arg.IsDropped} 发送客户端id:{arg.SenderClientId} 接收客户端id:{arg.ReceiverClientId} Topic:{arg.ApplicationMessage.Topic} {arg.ApplicationMessage.PayloadSegment}");
    return Task.CompletedTask;
}

Task MqttServer_ClientAcknowledgedPublishPacketAsync(ClientAcknowledgedPublishPacketEventArgs e)
{
    Console.WriteLine($"Client '{e.ClientId}' acknowledged packet {e.PublishPacket.PacketIdentifier} with topic '{e.PublishPacket.Topic}'");

    // It is also possible to read additional data from the client response. This requires casting the response packet.
    var qos1AcknowledgePacket = e.AcknowledgePacket as MqttPubAckPacket;
    Console.WriteLine($"QoS 1 reason code: {qos1AcknowledgePacket?.ReasonCode}");

    var qos2AcknowledgePacket = e.AcknowledgePacket as MqttPubCompPacket;
    Console.WriteLine($"QoS 2 reason code: {qos1AcknowledgePacket?.ReasonCode}");
    return Task.CompletedTask;
}

Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs e)
{
    if (e.UserName != "mqtt_client" || e.Password != "mqtt_password")
    {
        Console.WriteLine($"{e.ClientId} 校验失败，{e.UserName} {e.Password}");
        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
    }
    return Task.CompletedTask;
}

Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs e)
{
    Console.WriteLine($"已连接的客户端 {e.Endpoint} {e.ClientId} {e.ProtocolVersion}");
    return Task.CompletedTask;
}
