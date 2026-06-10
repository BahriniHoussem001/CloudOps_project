namespace CloudOps.Api.Modules.Requests.Dtos
{
    public class ServiceRequestDto
    {
        public Guid Id { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
