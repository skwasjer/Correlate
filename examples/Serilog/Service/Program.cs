using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseSerilog((ctx, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        // Add the property to the template explicitly, or log all properties with {Properties}.
        .WriteTo.Console(outputTemplate: "Message: {Message:lj}{NewLine}\tCorrelation ID: {CorrelationId}{NewLine}")
        .WriteTo.Debug()
    );

builder.Services
    .AddCorrelate()
    .AddControllers();

WebApplication app = builder.Build();

app.UseCorrelate();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
