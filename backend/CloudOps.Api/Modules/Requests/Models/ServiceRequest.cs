namespace CloudOps.Api.Modules.Requests.Models
{
    public class ServiceRequest
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        public Guid ServiceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
