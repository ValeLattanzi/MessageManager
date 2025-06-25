using MessageManager.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .WriteTo.File($"./logs/{DateTime.Now.Date:yyyy-MM-dd}.txt", rollingInterval: RollingInterval.Day)
  .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<MongoService>();

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddHttpClient("InsecureClient")
    .ConfigurePrimaryHttpMessageHandler(() => {
      var handler = new HttpClientHandler();
      handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
      return handler;
    });
}
else
  builder.Services.AddHttpClient("InsecureClient");

builder.Services.AddControllers();

builder.Services.AddHostedService<MessageDispatcherService>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Message Manager API V1");
    options.RoutePrefix = "swagger";
  });
}

app.MapGet("/", () => "API funcionando");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();