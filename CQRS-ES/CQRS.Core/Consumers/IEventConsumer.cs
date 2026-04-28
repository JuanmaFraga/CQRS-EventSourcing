using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Consumers
{
    public interface IEventConsumer                     // Interfaz abstracta que define el contrato para el EventConsumer de Kafka, estableciendo las operaciones que deben implementarse para manejar los eventos de manera efectiva.
    {
        void Consume(string topic);                   // Método que se encarga de consumir los eventos de un tema específico en Kafka, permitiendo que el EventConsumer escuche y procese los eventos a medida que llegan.
    }
}
