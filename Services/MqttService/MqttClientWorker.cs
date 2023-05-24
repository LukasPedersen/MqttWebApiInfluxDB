using Microsoft.Extensions.Hosting;
using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Protocol;
using MQTTnet.Samples.Helpers;
using InfluxDB.Client.Api.Domain;
using Services.MqttService;
using InfluxDB.Client;
using static MQTTnet.Samples.Helpers.ObjectExtensions;

namespace Services.MqttService
{
    public class MqttClientWorker : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Subing to lukas/Telemetry");
            var mqttFactory = new MqttFactory();
            InfluxDBService dBService = new InfluxDBService();

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

                    e.DumpToConsole();
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
