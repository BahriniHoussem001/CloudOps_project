namespace CloudOps.Api.Modules.Services.Dtos
{
    public class CreateServiceRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

    }
}
