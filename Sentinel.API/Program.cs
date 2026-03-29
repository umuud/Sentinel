using Microsoft.EntityFrameworkCore;
using Sentinel.Persistence.Context;
using Sentinel.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);


// Add services to the container.
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

app.Run();
