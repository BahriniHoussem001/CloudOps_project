namespace CloudOps.Api.Modules.Requests.Events
{
    public class RequestCreatedEvent
    {
        public Guid RequestId { get; set; }

        public Guid ClientId { get; set; }

        public Guid ServiceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
