using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet;
using WebApiMqtt.Services.InfluxDBService;
using System.Text;
using System.Text.Json;
using static WebApiMqtt.Services.ObjectExtensions;

namespace WebApiMqtt.Services.MQTTService
{
    public class MQTTWorker : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Subing to lukas/Telemetry");
            var mqttFactory = new MqttFactory();
            InfluxDBHandler dBService = new InfluxDBHandler();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTls()
                    .WithTcpServer("dd3284e565894d2a941755a18191be1d.s2.eu.hivemq.cloud")
                    .WithClientId("Lukas-API-Sub")
                    .WithCleanSession(false)
                    .WithCleanStart(false)
                    .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithCredentials("ArduinoTelemetryTest", "5f8UeQ9G")
                    .Build();

                // Setup message handling before connecting so that queued messages
                // are also handled properly. When there is no event handler attached all
                // received messages get lost.
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("Received application message.");
                    string jsonString = Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                    try
                    {
                        Telemetry newTelementry = JsonSerializer.Deserialize<Telemetry>(jsonString);
                        dBService.Publish(newTelementry);
                    }
                    catch (Exception)
                    {

                    }
                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic("+/+/climate");
                        })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                Console.ReadKey();
            }
        }
    }
}
