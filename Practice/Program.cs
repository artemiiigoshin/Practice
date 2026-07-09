using Practice.Application.Extensions;
using Practice.Infrastructure.Extensions;
using Practice.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var version = 7;

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v" + version, new() { Title = "Event API", Version = "sprint-" + version });
});

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v" + version + "/swagger.json", "Event API V" + version);
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();