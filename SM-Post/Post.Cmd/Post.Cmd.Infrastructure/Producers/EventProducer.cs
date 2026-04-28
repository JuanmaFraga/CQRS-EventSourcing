using Confluent.Kafka;
using CQRS.Core.Events;
using CQRS.Core.Producers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Post.Cmd.Infrastructure.Producers
{
    public class EventProducer : IEventProducer
    {
        private readonly ProducerConfig _config;                    // Campo privado para almacenar la configuración del productor de Kafka, que se inyecta a través del constructor. Esta configuración incluirá detalles como la dirección del servidor Kafka, las opciones de seguridad y otras configuraciones necesarias para establecer la conexión con Kafka y enviar eventos de manera eficiente.

        // Construnctor para inyectar la configuración del productor de Kafka a través de IOptions<ProducerConfig>, lo que permite que la configuración se gestione de manera centralizada y se pueda modificar fácilmente sin necesidad de cambiar el código del productor. Esto facilita la flexibilidad y la mantenibilidad del código al permitir que la configuración se ajuste según las necesidades del entorno de ejecución o los requisitos específicos de la aplicación.
        public EventProducer(IOptions<ProducerConfig> config)               // En .NET la configuración no se inyecta directamente como objetos, sino a través del patrón Options. Usmos IOptions porque integra tu clase de configuración con el sistema de configuración y DI de .NET.
        {                                                                   // Para este caso vamos a usar el campo BoostrapServers del IOptions para configurar la dirección del servidor Kafka al que se conectará el productor para enviar los eventos. 
            _config = config.Value;

        }

        public async Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent
        {
            using var producer = new ProducerBuilder<string, string>(_config)       // Crea una instancia del productor de Kafka utilizando la configuración inyectada. El productor se construye con una clave de tipo string y un valor de tipo T, que es el tipo del evento que se va a enviar.
                .SetKeySerializer(Serializers.Utf8)                                 // El uso de "using" asegura que el productor se cierre correctamente después de su uso, liberando los recursos asociados.
                .SetValueSerializer(Serializers.Utf8)                               // Los SetKeySerializer y SetValueSerializer configuran los serializadores para la clave y el valor del mensaje, respectivamente. En este caso, ambos se configuran para usar UTF-8, lo que significa que tanto la clave como el valor del mensaje se serializarán como cadenas de texto en formato UTF-8 antes de ser enviados a Kafka.
                .Build();                                                           // .Build() finaliza la construcción del productor y devuelve una instancia lista para usar.

            // Nuevo mensaje de Kafka que se va a enviar tras el evento.
            var eventMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),                                    // Generamos un nuevo Guid cada vez
                Value = JsonSerializer.Serialize(@event, @event.GetType())          // Serializamos el evento a una cadena JSON para que pueda ser enviado como valor del mensaje a Kafka. Esto permite que el evento se transmita de manera estructurada y sea fácilmente deserializable por los consumidores que lo reciban.
            };

            var deliveryResult = await producer.ProduceAsync(topic, eventMessage);  // Envía el mensaje al tema especificado de Kafka de manera asincrónica.

            if (deliveryResult.Status == PersistenceStatus.NotPersisted)                 // Verifica el resultado de la entrega del mensaje. Si el estado de persistencia es "NotPersisted", significa que el mensaje no se ha guardado correctamente en Kafka, lo que podría indicar un problema de conexión o configuración. En este caso, se imprime un mensaje de error en la consola indicando que el evento no se pudo enviar al tema especificado.
            {
                throw new Exception($"Could not produce {@event.GetType().Name} message to topic - {topic} due to the following reason: {deliveryResult.Message}.");  // Lanza una excepción indicando que el evento no se pudo enviar al tema especificado, junto con la clave del mensaje.
            }
        }
    }
}
