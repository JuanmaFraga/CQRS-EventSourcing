using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Producers
{
    public interface IEventProducer                                             // Interfaz abstracta que representa el productor de eventos en el patrón CQRS, encargada de definir la funcionalidad para producir eventos de dominio. Este método se encargará de enviar el evento a Kafka para su posterior procesamiento por parte de los consumidores de eventos.
    {
        Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent;
    }
}
