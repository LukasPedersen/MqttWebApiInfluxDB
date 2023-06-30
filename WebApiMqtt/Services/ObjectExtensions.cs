namespace WebApiMqtt.Services
{
    public class ObjectExtensions
    {
        #region Payload objects
        public class ApplicationMessage
        {
            public object ContentType { get; set; }
            public object CorrelationData { get; set; }
            public bool Dup { get; set; }
            public int MessageExpiryInterval { get; set; }
            public string Payload { get; set; }
            public List<int> PayloadSegment { get; set; }
            public int PayloadFormatIndicator { get; set; }
            public int QualityOfServiceLevel { get; set; }
            public object ResponseTopic { get; set; }
            public bool Retain { get; set; }
            public object SubscriptionIdentifiers { get; set; }
            public string Topic { get; set; }
            public int TopicAlias { get; set; }
            public object UserProperties { get; set; }
        }

        public class PayloadObject
        {
            public ApplicationMessage ApplicationMessage { get; set; }
            public bool AutoAcknowledge { get; set; }
            public string ClientId { get; set; }
            public bool IsHandled { get; set; }
            public int PacketIdentifier { get; set; }
            public bool ProcessingFailed { get; set; }
            public int ReasonCode { get; set; }
            public object ResponseReasonString { get; set; }
            public List<object> ResponseUserProperties { get; set; }
            public object Tag { get; set; }
        }

        #endregion

        #region Publish objects
        public class Telemetry
        {
            public double Temperature { get; set; }
            public double Humidity { get; set; }
            public DateTime Time { get; set; }
        }
        #endregion

        public enum BroadCastType
        {
            Notification,
            Warning,
            Error
        }
    }
}
