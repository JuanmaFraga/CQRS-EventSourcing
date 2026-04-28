using Confluent.Kafka;
using CQRS.Core.Consumers;
using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.Consumers;
using Post.Query.Infrastructure.DataAccess;
using Post.Query.Infrastructure.Handlers;
using Post.Query.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

////////////////////// ZONA PARA AGREGAR SERVICIOS DE LA APLICACION //////////////////////
Action<DbContextOptionsBuilder> configureDbContext = o => o.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));     //UseLazyLoadingProxies() habilita la carga diferida de las entidades relacionadas, lo que significa que las entidades relacionadas se cargarán automáticamente cuando se acceda a ellas por primera vez.

builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
builder.Services.AddSingleton<DatabaseContextFactory>(new DatabaseContextFactory(configureDbContext));     // Agrega el DatabaseContextFactory como un servicio singleton en el contenedor de servicios de la aplicación, lo que permite que una única instancia del factory se comparta a lo largo de toda la aplicación para crear instancias de DatabaseContext según sea necesario, por la utilización de Factory.

builder.Services.AddScoped<IPostRepository, PostRepository>();     // Agrega el servicio de repositorio de posts al contenedor de servicios con un tiempo de vida scoped, lo que significa que se creará una nueva instancia del repositorio para cada solicitud HTTP, y se compartirá dentro de esa solicitud, lo que es adecuado para manejar operaciones relacionadas con la base de datos en el contexto de una aplicación web.
builder.Services.AddScoped<ICommentRepository, CommentRepository>();     // Agrega el servicio de repositorio de comentarios al contenedor de servicios con un tiempo de vida scoped
builder.Services.AddScoped<IEventHandler, Post.Query.Infrastructure.Handlers.EventHandler>();     // Hay un System.EventHandler en el espacio de nombres System, por lo que es necesario especificar el espacio de nombres completo para evitar ambigüedades.

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));     // Configura las opciones de ConsumerConfig utilizando la sección "Kafka:ConsumerConfig" del archivo de configuración de la aplicación, lo que permite que las opciones de configuración para el consumidor de Kafka se definan en el archivo de configuración y se inyecten en los servicios que las necesiten a través de la inyección de dependencias.
builder.Services.AddScoped<IEventConsumer, EventConsumer>();     // Agrega el servicio de consumidor de eventos al contenedor de servicios con un tiempo de vida scoped, lo que significa que se creará una nueva instancia del consumidor para cada solicitud HTTP, y se compartirá dentro de esa solicitud, lo que es adecuado para manejar operaciones relacionadas con el consumo de eventos en el contexto de una aplicación web.

///////////////////////////////////////////////////////////////////////////

///////////////// Create Database tables from Code (SQL) //////////////////////
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();     // Construye un proveedor de servicios a partir del contenedor de servicios configurado y luego obtiene una instancia de DatabaseContext utilizando el método GetRequiredService. Esto se hace para asegurarse de que la base de datos esté creada y las tablas estén configuradas correctamente antes de que la aplicación comience a manejar solicitudes.
dataContext.Database.EnsureCreated();                                                                // El método EnsureCreated() se utiliza para crear la base de datos y las tablas correspondientes si aún no existen.

//////////////////////////////////////////////////////////////////////////

// Agrego la registración del servicio del ConsumerHostedService para que se ejecute en segundo plano y mantenga el consumidor de eventos en ejecución, lo que permite procesar los eventos a medida que llegan y mantener la base de datos de consulta actualizada con los cambios realizados en la base de datos de escritura a través de los eventos consumidos.
builder.Services.AddHostedService<ConsumerHostedService>();     // Agrega el servicio hospedado de consumo de eventos al contenedor de servicios, lo que permite que el ConsumerHostedService se ejecute en segundo plano para mantener el consumidor de eventos en ejecución y procesar los eventos a medida que llegan, asegurando que el consumidor de eventos esté siempre activo mientras la aplicación esté en ejecución.

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
