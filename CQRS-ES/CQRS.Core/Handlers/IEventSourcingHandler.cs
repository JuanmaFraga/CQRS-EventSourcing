using CQRS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Handlers
{
    public interface IEventSourcingHandler<T>           // Interfaz abstracta para que el Handler de Command obtenga el último estado del Aggregate y persista los cambios no commitidos en el Event Store.
    {
        Task SaveAsync(AggregateRoot aggregate);     // Método para guardar un agregado, que se encargará de persistir los eventos asociados al agregado en el Event Store. Este método es asíncrono, lo que permite una mejor escalabilidad y rendimiento en la aplicación.
    
        Task<T> GetByIdAsync(Guid aggregateId);              // T va a ser la implementación concreta del Aggregate (PostAggregate)
    }
}
