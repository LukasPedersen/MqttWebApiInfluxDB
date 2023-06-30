using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using System.Globalization;
using static WebApiMqtt.Services.ObjectExtensions;

namespace WebApiMqtt.Services.InfluxDBService
{
    public class InfluxDBHandler
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

        private string DBIP = "http://10.135.16.161:8086";
        private string BucketName = "HeatWatcher-Telementry";
        private string MyBaseURL = "http://luka0591.local:32768";

        private string Token = "eLqW_w9TJ1y5sGBlHa-fCih6gY7DjoN6iQtbr6r7HXdNzX6FSxKQlXZJU3J27grOK0LdS4lVjYZwkCEG1dRBrA==";
        public void Publish(Telemetry newTelemetry)
        {
            using var client = new InfluxDBClient(DBIP, Token);
            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteRecord($"home,room=Kontor temperature={newTelemetry.Temperature.ToString("F", new CultureInfo("en-US"))},humidity={newTelemetry.Humidity.ToString("F", new CultureInfo("en-US"))}", WritePrecision.S, BucketName, "da8ac4c4c1d2b7ba");
            }
        }

        public List<Telemetry> GetData()
        {
            using var client = new InfluxDBClient(DBIP, Token);
            var queryApi = client.GetQueryApiSync();
            var query = from s in InfluxDBQueryable<Telemetry>.Queryable(BucketName, "da8ac4c4c1d2b7ba", queryApi) select s;
            List<Telemetry> data = new();
            foreach (Telemetry tele in query.ToList())
            {
                tele.Time.AddHours(2);
                data.Add(tele);
            }
            return data;
        }

        public Telemetry GetLatestData()
        {
            Telemetry newTelemetry = new Telemetry
            {
                Temperature = randomTemp,
                Humidity = randomHumit,
                Time = DateTime.Now
            };

            return newTelemetry;
        }

        public List<Telemetry> GetData(DateTime timeFrom, DateTime timeTo)
        {
            using var client = new InfluxDBClient(DBIP, Token);
            var queryApi = client.GetQueryApiSync();
            var query = from s in InfluxDBQueryable<Telemetry>
                        .Queryable(BucketName, "da8ac4c4c1d2b7ba", queryApi, 
                        new QueryableOptimizerSettings
                        { 
                            DropStartColumn = true, 
                            DropStopColumn = true,
                        })
                        select s;

            List<Telemetry> data = new();
            foreach (Telemetry tele in query.ToList())
            {
                if (tele.Time.AddHours(2).Ticks >= timeFrom.Ticks && tele.Time.AddHours(2).Ticks <= timeTo.Ticks)
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
                //using var client = new InfluxDBClient("https://eu-central-1-1.aws.cloud2.influxdata.com", Token);
                using var client = new InfluxDBClient(DBIP, Token);
                using (var writeApi = client.GetWriteApi())
                {
                    //writeApi.WriteRecord($"home,room=Kontor temperature={Convert.ToDouble(randomTemp).ToString("F", new CultureInfo("en-US"))},humidity={Convert.ToDouble(randomHumit).ToString("F", new CultureInfo("en-US"))}", WritePrecision.S, "HouseWatcher", "b8fb8b6306ae7726");
                    writeApi.WriteRecord($"home,room=Kontor temperature={Convert.ToDouble(randomTemp).ToString("F", new CultureInfo("en-US"))},humidity={Convert.ToDouble(randomHumit).ToString("F", new CultureInfo("en-US"))}", WritePrecision.S, BucketName, "da8ac4c4c1d2b7ba");
                }

            }
        }

        public async Task LoopLiveData()
        {
            //http://luka0591.local:32768/ChillWatcherHub

            var connection = new HubConnectionBuilder()
            .WithUrl($"http://10.135.16.161:32768/ChillWatcherHub")
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
            .WithUrl($"{MyBaseURL}/ChillWatcherHub")
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
