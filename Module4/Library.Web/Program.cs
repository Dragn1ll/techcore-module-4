using FluentValidation.AspNetCore;
using Library.Contracts.Books.Request;
using Library.Data.PostgreSql;
using Library.Domain.Abstractions.Services;
using Library.Domain.Services;
using Library.SharedKernel.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddHealthChecks();

services.AddControllers()
    .AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateBookRequest>();
        fv.AutomaticValidationEnabled = true;
    });
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddScoped<IBookService, BookService>();

services.Configure<MySettings>(
    builder.Configuration.GetSection("MySettings"));

services.AddDbContext<BookContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));
});

var app = builder.Build();

var mySettings = app.Services.GetRequiredService<IOptions<MySettings>>().Value;

if (mySettings.EnableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var pds = context.RequestServices.GetService<IProblemDetailsService>();

        if (pds == null || !await pds.TryWriteAsync(new ProblemDetailsContext { HttpContext = context }))
        {
            await context.Response.WriteAsync("Fallback: An error occurred.");
        }
    });
});

app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Запрос начал обрабатываться: {context.Request.Method} {context.Request.Path}");
    
    await next();
    
    var endTime = DateTime.UtcNow;
    var executionTime = endTime - startTime;
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Запрос обработан: {context.Request.Method} {context.Request.Path} - " +
                      $"Время выполнения: {executionTime.TotalMilliseconds} мс - " +
                      $"Статус: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();