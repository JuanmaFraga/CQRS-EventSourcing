using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>                 // Clase que implementará la interfaz IEventSourcingHandler para manejar la lógica de persistencia de eventos y recuperación del estado del agregado a través del Event Store.
    {
        private readonly IEventStore _eventStore;
        private readonly IEventProducer _eventProducer;                     // Sólo para hacer la republicación de todos los eventos de la read database

        public EventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer)                 // Aseguramos que la dependencia IEventStore se inyecte a través del constructor.
        {
            _eventStore = eventStore;
            _eventProducer = eventProducer;                                                                // también inyectamos el eventProducer para hacer la republicación de todos los eventos de la read database
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            // Creamos una nueva instancia del agregado PostAggregate.
            var aggregate = new PostAggregate();

            // Recuperamos los eventos asociados al ID del agregado desde el Event Store.
            var events = await _eventStore.GetEventsAsync(aggregateId);

            if (events == null || !events.Any())
                return aggregate;                 // Si no se encuentran eventos, devolvemos el agregado vacío.

            // Reproducimos los eventos para reconstruir el estado del agregado.
            aggregate.ReplayEvents(events);

            //Actualizamos la versión del agregado al número de versión más reciente de los eventos reproducidos.
            aggregate.Version = events.Max(x => x.Version);                 // O events.Select(e => e.Version).Max(); 

            return aggregate;            
        }

        public async Task RepublishEventsAsync()       // Volvemos a publicar TODOS los eventos HACIA KAFKA
        {
            var aggregateIds = await _eventStore.GetAggregateIdsAsync();        // Buscamos todos los Ids de los aggregates de la Event Store

            if (aggregateIds == null || !aggregateIds.Any())
            {
                return;
            }

            foreach (var aggregateId in aggregateIds)
            {
                var aggregate = await GetByIdAsync(aggregateId);                // Para cada Aggregate Id, buscamos la entidad y nos fijamos sólo las que estan activas
                
                if(aggregate == null || !aggregate.Active) continue;

                var events = await _eventStore.GetEventsAsync(aggregateId);     // Para cada aggregate activo buscamos todos sus eventos

                foreach(var @event in events )
                {
                    var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");          // Especificado en launchsettings.json en el apartado "env" de variables de entorno

                    await _eventProducer.ProduceAsync(topic, @event);                       // ReProducimos todos los eventos de cáda aggregate para Kafka
                }
            }
        }

        public async Task SaveAsync(AggregateRoot aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version);     // Llama al método SaveEventsAsync del Event Store para persistir los eventos no commitidos del agregado.
            
            aggregate.MarkChangesAsCommitted();     // Para que la próxima vez que invoquemos SaveAsync del Even Source no nos traiga cambios sin commitir que si hayan sigo persistidos
        }
    }
}
