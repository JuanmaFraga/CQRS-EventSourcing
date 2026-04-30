using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Infrastructure
{
    public interface IEventStore                                    // Interfaz abstracta para el EventStore, que define los métodos que deben implementarse para interactuar con el EventStore. Esta interfaz se utiliza para abstraer la implementación del EventStore y permitir la flexibilidad de cambiar la implementación sin afectar el resto del código que depende del EventStore.
    {
        Task SaveEventsAsync(Guid aggregateId, IEnumerable<BaseEvent> events, int expectedVersion); 

        Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId);

        Task<List<Guid>> GetAggregateIdsAsync();                // Retorna todos los Ids de todos los aggregates en el event Store
    }
}
