using Core.ApplicationModels.KironTestAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities
{
    public static class ValidationHelper
    {
        public static Task<string> ValidateUserRegister(RegisterNewUserRequest user)
        {
            if (user == null)
            {
                return Task.FromResult("User not supplied");
            }

            var userValidationErrors = new StringBuilder(string.Empty);
            if (!RegexUtilities.IsValidEmail(user.Username))
            {
                userValidationErrors.AddValidationError("Please provide a valid email address as username");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                userValidationErrors.AddValidationError("Please provide a valid password");
            }

            if (user.ConfirmPassword != user.Password)
            {
                userValidationErrors.AddValidationError("Please ensure passwords match");
            }

            return Task.FromResult(userValidationErrors.ToString().TrimEnd());
        }

        private static void AddValidationError(this StringBuilder sb, string error)
        {
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                error = Environment.NewLine + error;
            }

            sb.Append(error);
        }
    }
}
