using BuildingBlocks.Configuration;

var builder = WebApplication.CreateBuilder(args);

var databaseConnection = PostgresConnectionString.Normalize(
    builder.Configuration.GetConnectionString("Database")!);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddCarter();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://*.onrender.com", "https://*.netlify.app"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddMarten(opts =>
{
    opts.Connection(databaseConnection);
}).UseLightweightSessions();

var app = builder.Build();

app.UseCors("Frontend");
app.MapCarter();

app.Run();
