﻿using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MQTTnet.Samples.Helpers.ObjectExtensions;

namespace Services.MqttService
{
    public class MqttClientPublish : IMqttClientPublish
    {
        public async Task Publish_Application_Message(string info)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("dd3284e565894d2a941755a18191be1d.s2.eu.hivemq.cloud")
                    .WithCredentials("TelemetryAPI", "dQeQ8U+H")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lukas/Telemetry")
                    .WithPayload(info)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine($"MQTT application message is published:{info}");
            }
        }
    }
}
