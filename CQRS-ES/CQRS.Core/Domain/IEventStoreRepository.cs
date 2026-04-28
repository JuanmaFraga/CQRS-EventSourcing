using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Domain                              // Se define en Domain y no en Infrastructure porque es una abstracción del negocio, no una implementación técnica. Domain → define qué necesita el negocio, Infrastructure → define cómo se implementa eso
{                                                       // Las interfaces de repositorio definen los métodos que se deben implementar para interactuar con la base de datos.
    public interface IEventStoreRepository              // Interfaz para el repositorio de eventos. Este repositorio es responsable de interactuar con la base de datos de Eventos para persistir los eventos y reconstruir el estado del Aggregate a partir de su historial de eventos.
    {                                                   // Sólo deberíamos poder crear y leer data porque los eventos son inmutables, o sea que no se pueden cambiar
        Task SaveAsync(EventModel @event);                              // Método para guardar un evento en la base de datos. Este método se utiliza para persistir los eventos generados por los Aggregates.
        Task<List<EventModel>> FindByAggregateId(Guid aggregateId);     // Método para obtener la lista de eventos asociados a un Aggregate específico, identificado por su Id. Este método se utiliza para reconstruir el estado del Aggregate a partir de su historial de eventos.
    }
}
