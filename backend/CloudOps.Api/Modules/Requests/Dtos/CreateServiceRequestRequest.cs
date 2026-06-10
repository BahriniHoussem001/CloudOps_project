namespace CloudOps.Api.Modules.Requests.Dtos
{
    public class CreateServiceRequestRequest
    {
        public Guid ClientId { get; set; }

        public Guid ServiceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
