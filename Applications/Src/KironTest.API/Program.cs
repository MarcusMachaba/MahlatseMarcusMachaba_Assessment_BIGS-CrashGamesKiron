using KironTest.API;
using KironTest.API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.ConfigureStaticDependencies();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Seed data from outside the dataprovider
//app.Services.GetRequiredService<IDefaultDataExtension>().SetupDefaultTestData();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
