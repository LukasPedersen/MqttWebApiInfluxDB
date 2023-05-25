using Microsoft.AspNetCore.Http;
using Services.MqttService;
using System.Net.Http;
using System.Text.Json;
using Services.MqttService;
using static MQTTnet.Samples.Helpers.ObjectExtensions;
using WebApiMqtt.Hubs;

namespace WebApiMqtt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddHostedService<MqttClientWorker>();
            builder.Services.AddSingleton<IMqttClientPublish, MqttClientPublish>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapHub<ChillWathcerHub>("/ChillWatcherHub");

            InfluxDBService dBService = new();
            app.MapPost("/setLED", async (string info, IMqttClientPublish service) =>
            {
                await service.Publish_Application_Message(info);
            });
            app.MapPost("/setServo", async (string info, IMqttClientPublish service) =>
            {
                await service.Publish_Application_Message(info);
            });
            app.MapPost("/createTelemetry", async (Telemetry info, IMqttClientPublish service) =>
            {
                dBService.Publish(info);
            });
            app.MapGet("/getTelemetry", (IMqttClientPublish service) =>
            {
                return dBService.GetData();
            });
            dBService.LoopData();
            app.Run();
        }
    }
}