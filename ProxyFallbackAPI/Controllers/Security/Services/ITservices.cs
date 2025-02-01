public interface ITokenService
{
    string GenerateToken(string username);
    bool ValidateToken(string token);
}
