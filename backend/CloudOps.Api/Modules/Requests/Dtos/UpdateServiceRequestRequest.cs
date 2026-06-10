namespace CloudOps.Api.Modules.Requests.Dtos
{

    public class UpdateServiceRequestRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
