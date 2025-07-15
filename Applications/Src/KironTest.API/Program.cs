using KironTest.API;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.ConfigureStaticDependencies();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
if (enableSwagger)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(builder.Configuration["ApplicationVersion"], new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = builder.Configuration["ApplicationName"],
            Version = builder.Configuration["ApplicationVersion"]
        });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme="oauth2",
                    Name="Bearer",
                    In=ParameterLocation.Header
                },
                new List<string>()
            }
        });
    });
}

var app = builder.Build();

//Seed data from outside the dataprovider
//app.Services.GetRequiredService<IDefaultDataExtension>().SetupDefaultTestData();

if (app.Environment.IsDevelopment() && enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"/swagger/{builder.Configuration["ApplicationVersion"]}/swagger.json", $"{builder.Configuration["ApplicationName"]} {builder.Configuration["ApplicationVersion"]}");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
