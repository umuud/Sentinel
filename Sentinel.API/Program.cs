using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Sentinel.API.Extensions;
using Sentinel.API.Middlewares;
using Sentinel.Application.DependencyInjection;
using Sentinel.Infrastructure.DependencyInjection;
using Sentinel.Persistence.Context;
using Sentinel.Persistence.DependencyInjection;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApi(builder.Configuration, builder.Host);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔥 Migration otomatik uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SentinelDbContext>();
    db.Database.Migrate();
}

// Swagger her ortamda açık
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();