﻿using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLogging(configure => configure.AddSerilog(
            new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                // Add the property to the template explicitly, or log all properties with {Properties}.
                .WriteTo.Console(outputTemplate: "Message: {Message:lj}{NewLine}\tCorrelation ID: {CorrelationId}{NewLine}")
                .WriteTo.Debug()
                .CreateLogger()
        )
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
