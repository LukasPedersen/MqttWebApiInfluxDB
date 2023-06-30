using WebApiMqtt.Hubs;
using WebApiMqtt.Services.InfluxDBService;
using WebApiMqtt.Services.MQTTService;
using static WebApiMqtt.Services.ObjectExtensions;

namespace WebApiMqtt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            //builder.Services.AddHostedService<MqttClientWorker>();
            builder.Services.AddSingleton<IMQTTPublish, MQTTPublish>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.MapHub<ChillWathcerHub>("/ChillWatcherHub");

            InfluxDBHandler dBService = new();
            app.MapPost("/setLED", async (string info, IMQTTPublish service) =>
            {
                await service.Publish_Application_Message(info);
            });
            app.MapPost("/setServo", async (string info, IMQTTPublish service) =>
            {
                await service.Publish_Application_Message(info);
            });
            app.MapPost("/createTelemetry", async (Telemetry info, IMQTTPublish service) =>
            {
                dBService.Publish(info);
            });
            app.MapGet("/getTelemetry", (IMQTTPublish service) =>
            {
                return dBService.GetData();
            });
            app.MapGet("/getLatestTelemetry", (IMQTTPublish service) =>
            {
                return dBService.GetLatestData();
            });
            app.MapGet("/getTelemetryByDate", (IMQTTPublish service, DateTime from, DateTime to) =>
            {
                return dBService.GetData(from.ToUniversalTime(), to.ToUniversalTime());
            });
            app.MapPost("/testHub", (IMQTTPublish service) =>
            {
                return dBService.TestHub();
            });
            dBService.LoopLiveData();
            dBService.LoopData();
            app.Run();
        }
    }
}