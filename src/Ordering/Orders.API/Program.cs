using FluentValidation;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orders.API.Data;
using Orders.API.Behaviors;
using Orders.API.Exceptions;
using Orders.API.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Orders API", Version = "v1", Description = "Microservicio de órdenes de compra con MongoDB e idempotencia." });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOptions<MongoOptions>()
    .Bind(builder.Configuration.GetSection(MongoOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "MongoDb:ConnectionString es obligatorio.")
    .ValidateOnStart();
builder.Services.AddOptions<OrderOptions>()
    .Bind(builder.Configuration.GetSection(OrderOptions.SectionName))
    .Validate(options => options.TaxRate is >= 0 and <= 1, "Order:TaxRate debe estar entre 0 y 1.")
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(provider =>
    new MongoClient(provider.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString));
builder.Services.AddScoped<MongoOrderRepository>();
builder.Services.AddScoped<IOrderRepository>(provider => provider.GetRequiredService<MongoOrderRepository>());
builder.Services.AddHostedService<MongoIndexInitializer>();

builder.Services.AddHttpClient<IBasketClient, BasketClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BasketUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CatalogUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://*.onrender.com", "https://*.netlify.app"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(allowedOrigins)
    .SetIsOriginAllowedToAllowWildcardSubdomains()
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddHealthChecks().AddMongoDb(
    sp => sp.GetRequiredService<IMongoClient>(),
    name: "mongodb",
    timeout: TimeSpan.FromSeconds(5));

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI();
app.MapCarter();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
