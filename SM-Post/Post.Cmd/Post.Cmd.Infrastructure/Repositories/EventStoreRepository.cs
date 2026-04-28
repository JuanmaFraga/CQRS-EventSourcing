using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Cmd.Infrastructure.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly IMongoCollection<EventModel> _eventStoreCollection;         // El repositorio de eventos se encarga de interactuar con la base de datos de eventos para persistir los eventos y reconstruir el estado del Aggregate a partir de su historial de eventos. En este caso, utilizamos MongoDB como base de datos para almacenar los eventos.

        public EventStoreRepository(IOptions<MongoDBConfig> config)                 // Constructor público que recibe una instancia de IOptions<MongoDBConfig> para obtener la configuración de MongoDB, como la cadena de conexión y el nombre de la base de datos. Luego, se utiliza esta configuración para obtener una referencia a la colección de eventos en MongoDB, que se utilizará para guardar y recuperar los eventos.
        {                                                                           // IOptions nos permite inyectar la configuración de MongoDB por Dependency Injection en el repositorio de eventos, lo que facilita la gestión de la configuración y la separación de preocupaciones en la aplicación.
            var mongoClient = new MongoClient(config.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(config.Value.Database);

            _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Value.Collection);   // Obtenemos una referencia a la colección de eventos en MongoDB utilizando el nombre de la colección especificado en la configuración. Esta colección se utilizará para guardar y recuperar los eventos relacionados con los Aggregates en el sistema.
        }

        public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
        {
            return await _eventStoreCollection.Find(x => x.AggregateIdentifier == aggregateId).ToListAsync().ConfigureAwait(false);     // ConfigureAwait(false) se usa para evitar forzar que el callback se ejecute en el mismo contexto de sincronización, lo que puede mejorar el rendimiento y evitar deadlocks en aplicaciones ASP.NET Core.
        }                                                                                                                               // ConfigureAwait(false) es una indicación para el await que dice: “cuando termine esta operación async, no vuelvas al contexto original; continuá en cualquier hilo disponible”

        public async Task SaveAsync(EventModel @event)
        {
            await _eventStoreCollection.InsertOneAsync(@event).ConfigureAwait(false);     // Guarda un evento en la colección de eventos de MongoDB utilizando el método InsertOneAsync, que inserta un nuevo documento en la colección.
        }
    }
}
