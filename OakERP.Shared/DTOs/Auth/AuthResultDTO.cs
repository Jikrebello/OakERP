namespace OakERP.Shared.DTOs.Auth
{
    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Error { get; set; }

        public static AuthResultDTO Failed(string error) =>
            new() { Success = false, Error = error };

        public static AuthResultDTO SuccessResult(string token) =>
            new() { Success = true, Token = token };
    }
}
