namespace CloudOps.Api.Modules.Notifications.Dtos
{
    public class UpdateNotificationRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }
    }
}
