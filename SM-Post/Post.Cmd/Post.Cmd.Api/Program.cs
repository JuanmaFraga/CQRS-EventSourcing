using Confluent.Kafka;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Post.Cmd.Api.Commands;
using Post.Cmd.Domain.Aggregates;
using Post.Cmd.Infrastructure.Config;
using Post.Cmd.Infrastructure.Dispatchers;
using Post.Cmd.Infrastructure.Handlers;
using Post.Cmd.Infrastructure.Producers;
using Post.Cmd.Infrastructure.Repositories;
using Post.Cmd.Infrastructure.Stores;
using Post.Common.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// CONFIGURACIÓN DE MONGODB PARA GUIDS
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Registramos las el classmap porque MongoDB por defecto no soporta Polimorfismo
BsonClassMap.RegisterClassMap<BaseEvent>();
BsonClassMap.RegisterClassMap<PostCreatedEvent>();
BsonClassMap.RegisterClassMap<MessageUpdatedEvent>();
BsonClassMap.RegisterClassMap<PostLikedEvent>();
BsonClassMap.RegisterClassMap<CommentAddedEvent>();
BsonClassMap.RegisterClassMap<CommentUpdatedEvent>();
BsonClassMap.RegisterClassMap<CommentRemovedEvent>();
BsonClassMap.RegisterClassMap<PostRemovedEvent>();

/////////////////////////////////////// ZONA DE REGISTRAR SERVICIOS DE USUARIO ///////////////////////////////////////

//El orden de registro de los servicios es importante para garantizar que las dependencias se resuelvan correctamente. En este caso, primero registramos la configuración de MongoDB, luego el repositorio de eventos, seguido del servicio de Event Store, el Event Sourcing Handler y finalmente el Command Handler. Esto asegura que cada servicio tenga acceso a sus dependencias necesarias cuando se inyecten en los controladores o endpoints que los utilicen.
builder.Services.Configure<MongoDBConfig>(builder.Configuration.GetSection(nameof(MongoDBConfig)));       // Tomamos la sección de "MongoDBConfig" del archivo appsettings.development.json (appsettings.json) para que se pueda inyectar en IOptions<MongoDBConfig> config la configuración de MongoDB por Dependency Injection en el repositorio de eventos.
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));       // Tomamos la sección de "ProducerConfig" del archivo appsettings.development.json (appsettings.json) para que se pueda inyectar en IOptions<ProducerConfig> config la configuración del productor de Kafka por Dependency Injection en el Event Producer.

builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();                     // Registramos el repositorio de eventos en el contenedor de servicios de ASP.NET Core utilizando AddScoped.
                                                                                               // Un servicio Scoped creará una nueva instancia del repositorio para cada solicitud HTTP. Esto es importante para garantizar que cada solicitud tenga su propia instancia del repositorio y evitar problemas de concurrencia al acceder a la base de datos de eventos.
builder.Services.AddScoped<IEventProducer, EventProducer>();                                   // Registramos el productor de eventos en el contenedor de servicios de ASP.NET Core utilizando AddScoped. Esto permitirá que el productor de eventos se inyecte en el servicio de Event Store para enviar los eventos a Kafka después de guardarlos en la base de datos de eventos.
builder.Services.AddScoped<IEventStore, EventStore>();                                         // Registramos el servicio de Event Store en el contenedor de servicios de ASP.NET Core utilizando AddScoped.
builder.Services.AddScoped<IEventSourcingHandler<PostAggregate>, EventSourcingHandler>();       // Registramos el Event Sourcing Handler para el agregado PostAggregate en el contenedor de servicios de ASP.NET Core utilizando AddScoped.
builder.Services.AddScoped<ICommandHandler, CommandHandler>();                                 // Registramos el Command Handler en el contenedor de servicios de ASP.NET Core utilizando AddScoped. Esto permitirá que el Command Handler se inyecte en los controladores o endpoints que lo necesiten para procesar los comandos relacionados con las operaciones de publicación, edición, me gusta, comentarios y eliminación de publicaciones.

///////////////////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////// ZONA DE REGISTRAR METODOS DEL COMMAND HANDLER  ///////////////////////////////////////

var commandHandler = builder.Services.BuildServiceProvider().GetRequiredService<ICommandHandler>();     // Creamos una instancia del Command Handler utilizando el contenedor de servicios de ASP.NET Core para resolver sus dependencias. Esto nos permitirá llamar a los métodos del Command Handler para procesar los comandos relacionados con las operaciones de publicación, edición, me gusta, comentarios y eliminación de publicaciones.
var dispatcher = new CommandDispatcher();                                                               // Creamos una instancia del Command Dispatcher, que es responsable de enviar los comandos para su procesamiento. El Command Dispatcher se encargará de enrutar los comandos al Command Handler correspondiente.

// Rellenamos el diccionario de handlers del Command Dispatcher con los métodos del Command Handler para cada tipo de comando específico. Esto permitirá que cuando se envíe un comando de un tipo específico, el Command Dispatcher lo enrute al método HandleAsync correspondiente del Command Handler para su procesamiento.
dispatcher.RegisterHandler<NewPostCommand>(commandHandler.HandleAsync);                             // Registramos el método HandleAsync del Command Handler para manejar el comando NewPostCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo NewPostCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<EditMessageCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando EditMessageCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo EditMessageCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<LikePostCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando LikePostCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo LikePostCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<AddCommentCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando AddCommentCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo AddCommentCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<EditCommentCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando EditCommentCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo EditCommentCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<RemoveCommentCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando RemoveCommentCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo RemoveCommentCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.
dispatcher.RegisterHandler<DeletePostCommand>(commandHandler.HandleAsync);                         // Registramos el método HandleAsync del Command Handler para manejar el comando DeletePostCommand en el Command Dispatcher. Esto permitirá que cuando se envíe un comando de tipo DeletePostCommand, el Command Dispatcher lo enrute al método HandleAsync del Command Handler para su procesamiento.

// Ahora que ya registramos todos los Handlers, podemos registrar el Command Dispatcher en el contenedor de servicios de ASP.NET Core utilizando AddSingleton, lo que permitirá que el Command Dispatcher se inyecte en los controladores o endpoints que lo necesiten para enviar los comandos relacionados con las operaciones de publicación, edición, me gusta, comentarios y eliminación de publicaciones.
// Utilizamos AddSingleton para registrar el Command Dispatcher, lo que significa que se creará una única instancia del Command Dispatcher durante toda la vida útil de la aplicación. Esto es apropiado para el Command Dispatcher, ya que no tiene estado y puede ser compartido de manera segura entre múltiples solicitudes. Al registrar el Command Dispatcher como un servicio singleton, garantizamos que todas las partes de la aplicación que dependan del Command Dispatcher utilicen la misma instancia, lo que facilita la gestión de los comandos y su enrutamiento a los handlers correspondientes.
builder.Services.AddSingleton<ICommandDispatcher>(_ => dispatcher);                             // _ => dispatcher significa “una función que recibe un parámetro (que no uso) y devuelve dispatcher”
//////////////////////////////////////////////////////////////////////////////////////////////////////////////


builder.Services.AddOpenApi();
// Inicializamos los Controllers
builder.Services.AddControllers();

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
    var forecast = Enumerable.Range(1, 5).Select(index =>
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

// Mapeamos los Controllers
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
