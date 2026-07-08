using CurrencyTrendConverter.Models;
using CurrencyTrendConverter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

const string frontendCorsPolicy = "AllowReactFrontend";

builder.Services.AddMemoryCache();
var exchangeApiSection = builder.Configuration.GetSection("ExchangeRateApi");
builder.Services.Configure<ExchangeRateApiOptions>(exchangeApiSection);
var exchangeOptions = exchangeApiSection.Get<ExchangeRateApiOptions>() ?? new ExchangeRateApiOptions();

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri(exchangeOptions.BaseUrl ?? "https://api.exchangerate.host/");
    client.Timeout = TimeSpan.FromSeconds(exchangeOptions.TimeoutSeconds);
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);
app.MapControllers();

app.Run();
