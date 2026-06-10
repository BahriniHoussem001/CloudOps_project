namespace CloudOps.Api.Modules.Users.Dtos
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
