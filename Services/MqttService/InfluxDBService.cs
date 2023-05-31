using InfluxDB.Client.Api.Domain;
using InfluxDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MQTTnet.Samples.Helpers.ObjectExtensions;
using System.Globalization;
using InfluxDB.Client.Linq;
using System.Net;
using Microsoft.AspNetCore.SignalR.Client;

namespace Services.MqttService
{
    public class InfluxDBService
    {
        public int randomTemp
        {
            get
            {
                Random rnd = new Random();
                return rnd.Next(18, 28);
            }
        }
        public int randomHumit
        {
            get
            {
                Random rnd = new Random();
                return rnd.Next(50, 80);
            }
        }

        private string Token = "acv-8n3_PLB1CCzim9C8XA0o7IM41uGpcg8oeouzbBQDXBkEy5FVM8BzwH7MU-nF9PUyvQOgRVRBezJMAZJafg==";
        public void Publish(Telemetry newTelemetry)
        {
            using var client = new InfluxDBClient("https://eu-central-1-1.aws.cloud2.influxdata.com", Token);
            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteRecord($"home,room=Kontor temperature={newTelemetry.Temperature.ToString("F", new CultureInfo("en-US"))},humidity={newTelemetry.Humidity.ToString("F", new CultureInfo("en-US"))}", WritePrecision.S, "HouseWatcher", "b8fb8b6306ae7726");
            }
        }

        public List<Telemetry> GetData()
        {
            using var client = new InfluxDBClient("https://eu-central-1-1.aws.cloud2.influxdata.com", Token);
            var queryApi = client.GetQueryApiSync();
            var query = from s in InfluxDBQueryable<Telemetry>.Queryable("HouseWatcher", "b8fb8b6306ae7726", queryApi) select s;
            List<Telemetry> data = new();
            foreach (Telemetry tele in query.ToList())
            {
                tele.Time.AddHours(2);
                data.Add(tele);
            }
            return data;
        }

        public List<Telemetry> GetData(DateTime timeFrom, DateTime timeTo)
        {
            using var client = new InfluxDBClient("https://eu-central-1-1.aws.cloud2.influxdata.com", Token);
            var queryApi = client.GetQueryApiSync();
            var query = from s in InfluxDBQueryable<Telemetry>
                        .Queryable("HouseWatcher", "b8fb8b6306ae7726", queryApi) select s;

            List<Telemetry> data = new();
            foreach (Telemetry tele in query.ToList())
            {
                if (tele.Time.Ticks >= timeFrom.Ticks && tele.Time.Ticks <= timeTo.Ticks)
                {
                    data.Add(tele);
                }
            }
            return data;
        }

        public async Task LoopData()
        {
            while (true)
            {
                await Task.Delay(60000);
                using var client = new InfluxDBClient("https://eu-central-1-1.aws.cloud2.influxdata.com", Token);
                using (var writeApi = client.GetWriteApi())
                {
                    writeApi.WriteRecord($"home,room=Kontor temperature={Convert.ToDouble(randomTemp).ToString("F", new CultureInfo("en-US"))},humidity={Convert.ToDouble(randomHumit).ToString("F", new CultureInfo("en-US"))}", WritePrecision.S, "HouseWatcher", "b8fb8b6306ae7726");
                }
            }
        }

        public async Task LoopLiveData()
        {
            var connection = new HubConnectionBuilder()
            .WithUrl("https://jswzjk6b-7117.euw.devtunnels.ms/ChillWatcherHub")
            .Build();

            await connection.StartAsync();

            while (true)
            {
                await Task.Delay(10000);
                Telemetry newTelemetry = new Telemetry
                {
                    Temperature = randomTemp,
                    Humidity = randomHumit,
                    Time = DateTime.Now
                };
                await connection.SendAsync("UpdateLatestTelementry", newTelemetry);
            }
        }

        public async Task TestHub()
        {
            var connection = new HubConnectionBuilder()
            .WithUrl("https://jswzjk6b-7117.euw.devtunnels.ms/ChillWatcherHub")
            .Build();

            await connection.StartAsync();

            Telemetry newTelemetry = new Telemetry
            {
                Temperature = randomTemp,
                Humidity = randomHumit,
                Time = DateTime.Now
            };

            await connection.SendAsync("UpdateLatestTelementry", newTelemetry);
        }
    }
}
