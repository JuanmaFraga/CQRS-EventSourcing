using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exeptions;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;
using Post.Cmd.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;

        private readonly IEventProducer _eventProducer;

        public EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer)     // Constructor público que recibe una instancia de IEventStoreRepository para interactuar con la base de datos de eventos. Esta instancia se inyecta a través de Dependency Injection, lo que permite una mayor flexibilidad y separación de preocupaciones en la aplicación.
        {
            _eventStoreRepository = eventStoreRepository;
            _eventProducer = eventProducer;
        }

        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);     // Llama al método GetEventStream del repositorio para obtener la secuencia de eventos asociados con el aggregateId proporcionado. Este método devuelve una lista de eventos que representan el historial de cambios para ese agregado.

            if (eventStream == null || !eventStream.Any())
            {
                throw new AggregateNotFoundException("Incorrect post ID provided.");
            }

            return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();   // Retorna todos los eventos asociados al aggregateId ordenados por versión.
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion)         // Responsable de guardar una lista de eventos para un agregado específico, y de invocar el productor del evento que producirá los eventos para Kafka.
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);     // Obtiene la secuencia de eventos actual para el aggregateId proporcionado.

            if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)            // eventStream[^1] equivale a eventStream[eventStream.Count - 1], es decir, el último evento de la secuencia. Compara la versión del último evento con la versión esperada proporcionada. Si no coinciden, se lanza una excepción de concurrencia, lo que indica que otro proceso ha modificado el agregado desde la última vez que se leyó.
            {                                                                                   // Es nuestra versión de concurrencia optimista.
                throw new ConcurrencyException();

            }

            var version = expectedVersion;

            foreach (var @event in events)          // Persistimos Todos los eventos.
            {
                version++;
                @event.Version = version;     // Asigna la versión incremental a cada evento en la lista de eventos que se va a guardar. Esto asegura que cada evento tenga una versión única y secuencial.
                var eventType = @event.GetType().Name;     // Obtiene el nombre del tipo de evento, lo que puede ser útil para la serialización y el almacenamiento en la base de datos.
                var eventModel = new EventModel             // Modelo que representa el schema del evento que estamos almacenando.
                {           
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventStoreRepository.SaveAsync(eventModel);     // Guarda el evento en la base de datos a través del repositorio. Este método se encarga de persistir el evento y de manejar cualquier lógica adicional relacionada con el almacenamiento, como la publicación del evento en Kafka.

                // Producimos el EVENTO para Kafka DESPUÉS de GUARDAR todos los eventos en la base de datos. Esto asegura que solo se produzcan eventos que han sido exitosamente persistidos, lo que ayuda a mantener la consistencia entre la base de datos y el sistema de mensajería.
                var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");     // Obtiene el nombre del topic de Kafka desde una variable de entorno, o utiliza un valor predeterminado si la variable no está configurada.
                await _eventProducer.ProduceAsync(topic, @event);                                   // Envía el evento a Kafka utilizando el productor de eventos.

                // Se podría hacer una TRANSACCION entre el SaveAsync que guarda en MongoDB y el ProduceAsync que produce el evento para Kafka, pero para hacer una transacción en MongoDB se necesita correr la DB como parte de un REPLICA SET
            }


        }
    }
}
