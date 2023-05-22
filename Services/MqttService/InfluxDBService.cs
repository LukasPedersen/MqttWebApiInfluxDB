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

namespace Services.MqttService
{
    public class InfluxDBService
    {
        private string Token = "2RXocPA9ZEVzh2losh1CUvORek70ZxJ04bZJQLlraaycc2PQfa2QO0XeDQJv22mzY7wHXHaTIWq7ZEGtEN1nnQ==";
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
            return query.ToList();
        }
    }
}
