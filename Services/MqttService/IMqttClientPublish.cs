using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MQTTnet.Samples.Helpers.ObjectExtensions;

namespace Services.MqttService
{
    public interface IMqttClientPublish
    {
        Task Publish_Application_Message(string info);
    }
}
