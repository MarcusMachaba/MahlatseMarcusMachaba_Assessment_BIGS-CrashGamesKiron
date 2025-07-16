using CachingLayer;
using Core.ApplicationModels.KironTestAPI;
using Core.Utilities;
using DatabaseLayer.Custom.Services;
using KironTest.API.DataAccess;
using KironTest.API.Hosting;
using KironTest.API.Interfaces;
using KironTest.API.ServiceHelpers;
using Logger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KironTest.API
{
    public static class KironApiStartupExtension
    {
        public static WebApplicationBuilder ConfigureStaticDependencies(this WebApplicationBuilder builder)//or callItSetupAll()  // call it this way from Program.cs AuthenticationServiceSetup.SetupAll(services, mvcBuilder, Config, GetIdentityModel);
        {
            LoggingDependencies.SetLog4NetDatabaseConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"));
            DALConfiguration.Config = builder.Configuration;
            InitializeDeployBaseDatabaseModels(builder);
            builder.Services.SetupEnvironmentAddCachingLayer();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("TokenAuthentication"));
            InjectAndResolveInterfaces(builder);
            ConfigureJWTAuthentication(builder);
            // schedule daily refresh at midnight [On app start: fire InitializeAsync() once (dueTime = 0 or TimeSpan.Zero param).]
            builder.Services.AddHostedService(sp =>
              new TimerHostedService(async _ => await sp.GetRequiredService<IBankHolidayService>().InitializeAsync(), TimeSpan.Zero, TimeSpan.FromHours(24))
            );

            return builder;
        }

        private static void ConfigureJWTAuthentication(WebApplicationBuilder builder)
        {
            // Setup JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("TokenAuthentication");
            //var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
            var base64Key = jwtSettings["SecretKey"]!;
            var keyBytes = Convert.FromBase64String(base64Key);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = builder.Configuration.GetValue<TimeSpan>("TokenAuthentication:ClockSkew") // Set clock skew to zero to avoid delays in token expiration
                };
            });
        }
        private static void InjectAndResolveInterfaces(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDefaultDataExtension>(new DefaultDataExtension(builder.Services));
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<ITokenService, JwtService>();
            builder.Services.AddScoped<INavigationService, NavigationService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<DataProvider>();
            builder.Services.AddSingleton<IBankHolidayService, BankHolidayService>();
            builder.Services.AddSingleton<IDragonBallCharacterService, DragonBallCharacterService>();
        }

        private static void InitializeDeployBaseDatabaseModels(WebApplicationBuilder builder)
        {
            using (DataProvider db = new DataProvider())
            {
                //// TODO: Implement DAL logic to create database from connString accessing the thread
                //// Check if the database exists, if not create it
                // db.EnsureDatabaseExists();

                var result = db.CompareModelToDatabase();
                if (result.TableDifferences.Count > 0 || result.StoredProcedureDifferences.Count > 0 || result.IndexDifferences.Count > 0)
                    db.Deploy(result);
            }
        }
    }
}
