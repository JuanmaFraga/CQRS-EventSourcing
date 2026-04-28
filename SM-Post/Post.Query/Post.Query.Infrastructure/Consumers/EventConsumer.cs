using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Post.Query.Infrastructure.Consumers
{
    public class EventConsumer : IEventConsumer           // Implementación abstracta que contiene la logica de consumo de eventos, que se encargará de consumir los eventos que llegan a través de Kafka y procesarlos para actualizar la base de datos de consulta en consecuencia. Esta clase se encargará de recibir los eventos, deserializarlos utilizando el EventJsonConverter, y luego llamar a los métodos correspondientes para actualizar la base de datos de consulta.
    {
        private readonly ConsumerConfig _config;
        private readonly IEventHandler _eventHandler;     // Interfaz que define el contrato para manejar los eventos consumidos. El EventConsumer utilizará esta interfaz para delegar el procesamiento de los eventos a una implementación concreta del manejador de eventos, lo que permite una separación de responsabilidades y facilita la mantenibilidad del código.

        public EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler)                              // Constructor para inyectar la configuracióndel consumidor de Kafka y el manejador de eventos. El constructor recibe un objeto IOptions<ConsumerConfig> que contiene la configuración del consumidor de Kafka, y un objeto IEventHandler que es la implementación concreta del manejador de eventos que se utilizará para procesar los eventos consumidos.
        {
            _config = config.Value;
            _eventHandler = eventHandler;
        }

        public void Consume(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)  // Crea una instancia del consumidor de Kafka utilizando la configuración proporcionada. Se crea con una Key y un Value tipo string de la misma manera que se maneja el Producer.
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();     // Crea una instancia del consumidor de Kafka utilizando la configuración proporcionada. Se crea con una Key y un Value tipo string de la misma manera que se maneja el Producer.
            {
                consumer.Subscribe(topic);     // Suscribe el consumidor al tema especificado, lo que le permitirá recibir los mensajes publicados en ese tema.

                while (true)     // Bucle infinito para mantener el consumidor en ejecución y procesar los mensajes a medida que llegan.
                {

                    var consumeResult = consumer.Consume();     // Consume un mensaje del tema al que está suscrito. Hace un "Poll" de kafka, o sea que va a consultar (preguntar) al broker si hay mensajes nuevos para leer

                    if (consumeResult?.Message == null)             // consumeResult?.Message => Si consumeResult NO es null → accede a .Message; Si consumeResult ES null → devuelve null (sin lanzar excepción). Si el resultado de consumir un mensaje es null o si el mensaje consumido es null, se omite el procesamiento y se continúa con la siguiente iteración del bucle para esperar el próximo mensaje.
                    {
                        continue;
                    }

                    var options = new JsonSerializerOptions             // Registramos nuestro Converter personalizado para que JsonSerializer sepa cómo deserializar correctamente eventos polimórficos (derivados de BaseEvent) usando el campo Type.
                    { 
                        Converters = { new EventJsonConverter() } 
                    };  

                    var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);     // Deserializa el valor del mensaje consumido (que es un string JSON) en un objeto de tipo BaseEvent pasandole en las OPCIONES nuestro converter EventJsonConverter que mapea al derivado de Base event correspondiente.
                
                    var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() });     // Utiliza reflexión para obtener el método "On" porque todos nuestros métodos del manejador de eventos (EventHandler) siguen la convención de nombrado "On" seguido del tipo de evento que manejan (por ejemplo, On(PostCreatedEvent)). La reflexión permite inspeccionar el tipo del evento en tiempo de ejecución y encontrar el método correspondiente en el manejador de eventos que puede procesar ese tipo específico de evento.
                                                                                                                      // reflexión: es una característica de C# que permite inspeccionar y manipular el código en tiempo de ejecución.
                    if (handlerMethod == null)
                        throw new ArgumentException(nameof(handlerMethod), "Could not find event handler method!");
                    
                    handlerMethod.Invoke(_eventHandler, new object[] { @event });     // Invoca el método "On" utilizando reflexión, pasando el evento deserializado como argumento.
                                                                                      // Invoke ejecuta el método devuelto por el EventHandler dinámicamente usando reflection, permitiendo llamar al handler correcto según el tipo real del evento en runtime. 

                    consumer.Commit(consumeResult);                                   // Le decimos a Kafka que hemos procesado el mensaje correctamente y que puede marcarlo como "consumido" para evitar procesarlo nuevamente en el futuro, y que puede aumentar el offset del consumidor para avanzar al siguiente mensaje.
                }
            }
        }
    }
}
