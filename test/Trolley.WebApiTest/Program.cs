using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trolley;
using Trolley.AspNetCore;
using Trolley.MySqlConnector;
using Trolley.WebApiTest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTrolley(f =>
    f.LoadFromConfiguration(builder.Configuration, "Database")
     .AddTypeHandler<JsonTypeHandler>()
     .Configure<MySqlProvider, ModelConfiguration>());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
