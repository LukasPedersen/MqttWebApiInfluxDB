namespace WebApiMqtt.Services.MQTTService
{
    public interface IMQTTPublish
    {
        Task Publish_Application_Message(string info);
    }
}
