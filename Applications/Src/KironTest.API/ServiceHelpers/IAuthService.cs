using Azure.Core;
using Core.ApplicationModels.KironTestAPI;
using Core.Utilities;
using KironTest.API.DataAccess;
using System.Configuration;

namespace KironTest.API.ServiceHelpers
{
    public interface IAuthService
    {
        Task<TokenResponse?> LoginAsync(LoginRequest request);
        Task<User?> RegisterNewUserAsync(RegisterNewUserRequest request);
    }
    public class AuthService : IAuthService
    {
        private readonly Logger.Logger mLog;
        private readonly ITokenService _tokenService;
        public AuthService(ITokenService tokenService)
        {
            mLog = Logger.Logger.GetLogger(typeof(AuthService));
            _tokenService = tokenService;
        }
       
        public async Task<TokenResponse?> LoginAsync(LoginRequest request)
        {
            using (var dp = new DataProvider())
            {
                try
                {
                    dp.StartTransaction();
                    var user = (await dp.Users.ReadAsync(new { UserName = request.Username })).FirstOrDefault();
                    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    {
                        return null; 
                    }

                    var accessToken = _tokenService.GenerateAccessToken(user);

                    dp.CommitTransaction();
                    return new TokenResponse
                    {
                        AccessToken = accessToken.Token,
                        AccessTokenExpiresAt = accessToken.ExpiresAt,
                    };
                }
                catch (Exception ex)
                {
                    mLog.Error($"Error signing in user: {ex}");
                    dp.RollbackTransaction();
                    return null; 
                }
            }
        }

        public async Task<User?> RegisterNewUserAsync(RegisterNewUserRequest request)
        {
            using (var dp = new DataProvider())
            {
                try
                {
                    var userValidationErrors = ValidationHelper.ValidateUserRegister(request);
                    if (!string.IsNullOrEmpty(userValidationErrors.Result))
                    {
                        mLog.Warn($"User registration failed due to validation errors: {string.Join(", ", userValidationErrors)}");
                        return null; // Validation failed
                    }

                    dp.StartTransaction();
                    var existingUser = (await dp.Users.ReadAsync(new { UserName = request.Username })).FirstOrDefault();
                    if (existingUser != null)
                    {
                        return null; // User already exists
                    }

                    var newUser = new User
                    {
                        UserName = request.Username,
                        EmailAddress = string.Empty,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                    };

                    var newlyCreatedUserId = await dp.Users.CreateAsync(newUser);
                    if (newlyCreatedUserId > 0)
                        mLog.Info($"New user {request.Username} registered successfully.");
                    else
                    {
                        mLog.Warn($"Failed to register new user {request.Username}.");
                        return null; 
                    }
                    dp.CommitTransaction();
                    return newUser;
                }
                catch (Exception ex)
                {
                    mLog.Error($"Error registering new user: {ex}");
                    dp.RollbackTransaction();
                    return null; 
                }
            }
        }
    }
}
