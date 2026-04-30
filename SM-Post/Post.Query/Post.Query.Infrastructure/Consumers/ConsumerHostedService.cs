using Castle.Core.Logging;
using CQRS.Core.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : IHostedService               // Consummer service que corre en segundo plano para mantener el consumidor de eventos en ejecución y procesar los eventos a medida que llegan. Esta clase implementa la interfaz IHostedService, lo que permite que se ejecute como un servicio hospedado en el contexto de una aplicación ASP.NET Core o cualquier aplicación que soporte servicios hospedados. El ConsumerHostedService se encargará de iniciar el proceso de consumo de eventos cuando se inicie la aplicación y detenerlo cuando la aplicación se cierre, asegurando que el consumidor de eventos esté siempre activo mientras la aplicación esté en ejecución.
    {                                                                   // Se usa para transformar la Query API de una Api Restful comun, TAMBIÉN a una aplicación que se ejecute en segundo plano y se encargue de consumir los eventos que llegan a través de Kafka, procesarlos y actualizar la base de datos de consulta en consecuencia.
        private readonly ILogger<ConsumerHostedService> _logger;        // Inyección de dependencias para el logger, lo que permite registrar información sobre el estado del servicio y cualquier error que pueda ocurrir durante el proceso de consumo de eventos.
        private readonly IServiceProvider _serviceProvider;             // Lo usamos para crear un Scope nuevo para recuperar el servicio del EventConsumer de los scoped services registrados en el contenedor de dependencias. Esto es necesario porque el EventConsumer puede tener dependencias que también son scoped, y al crear un nuevo scope dentro del método StartAsync, podemos asegurarnos de que se resuelvan correctamente las dependencias del EventConsumer.

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider)       // Constructor para inyectar las dependencias del logger y el servicio de proveedor. El constructor recibe un objeto ILogger<ConsumerHostedService> para registrar información sobre el estado del servicio, y un objeto IServiceProvider para resolver las dependencias necesarias para el proceso de consumo de eventos.
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // IServiceProvider es una interfaz que proporciona un mecanismo para obtener (crear) servicios registrados en el contenedor de dependencias. En este caso, se utiliza para resolver la instancia del EventConsumer que se encargará de consumir los eventos de Kafka.
        // Como el EventConsumer va a correr en background, NO está dentro de una request HTTP, entonces No hay scope automático como en un controller


        //public Task StartAsync(CancellationToken cancellationToken)     // Se invoca cuando el contenedor se inicia, y se encarga de iniciar el proceso de consumo de eventos. Aquí es donde se puede crear una instancia del EventConsumer y llamar a su método Consume para comenzar a escuchar los eventos en el tema de Kafka correspondiente.
        //{
        //    _logger.LogInformation("Event Consumer Service running.");     // Registra un mensaje de información indicando que el servicio de consumo de eventos se está iniciando.

        //    //Obtenemos el servicio del EventConsumer desde el servicio scoped 
        //    using (IServiceScope scope = _serviceProvider.CreateScope())     // Crea un nuevo scope para resolver las dependencias del EventConsumer. Esto es necesario porque el EventConsumer puede tener dependencias que también son scoped, y al crear un nuevo scope dentro del método StartAsync, podemos asegurarnos de que se resuelvan correctamente las dependencias del EventConsumer.
        //    {
        //        var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();     // Resuelve la instancia del IEventConsumer desde el contenedor de dependencias utilizando el nuevo scope creado. Esto garantiza que se obtenga una instancia fresca del EventConsumer con todas sus dependencias resueltas correctamente.

        //        var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");     // Obtiene el nombre del tema de Kafka desde una variable de entorno llamada "KAFKA_TOPIC". Esto permite que el nombre del tema de Kafka sea configurable sin necesidad de modificar el código, lo que facilita la flexibilidad y la adaptabilidad del servicio a diferentes entornos.

        //        Task.Run(() => eventConsumer.Consume(topic), cancellationToken);     // Llama al método Consume del EventConsumer para comenzar a escuchar los eventos en el tema "KAFKA_TOPIC" de Kafka. Esto iniciará el proceso de consumo de eventos y permitirá que el EventConsumer procese los eventos a medida que llegan.
        //                                                                              // Task.Run(...) Crea una tarea asíncrona y la manda al ThreadPool. () =>  es una expresión lambda, “una función que no recibe parámetros y ejecuta ese código”
        //                                                                              // cancellationToken Permite cancelar la tarea si el servicio se detiene antes de que la tarea haya terminado, lo que ayuda a garantizar una finalización ordenada del servicio y evita que queden tareas en ejecución después de que el servicio se haya detenido.
        //    }

        //    return Task.CompletedTask;
        //}

        public Task StartAsync(CancellationToken cancellationToken)         // Solución al problema de concurrencia al enviar varios mensjes seguidos como en el rebuild de la base (sobre todo con los likes que se pisaban)
        {
            _logger.LogInformation("Event Consumer Service running.");

            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");

            // Lanzamos la tarea de fondo, pero NO usamos un 'using' que la envuelva
            Task.Run(() =>
            {
                // El scope se crea DENTRO de la tarea de fondo para que viva lo mismo que el consumidor
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
                    eventConsumer.Consume(topic);
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)      // Se invoca cuando el contenedor se detiene, y se encarga de detener el proceso de consumo de eventos. Aquí es donde se puede implementar la lógica para limpiar los recursos utilizados por el EventConsumer, como cerrar la conexión con Kafka o liberar cualquier otro recurso que se haya utilizado durante el proceso de consumo.
        {
            _logger.LogInformation("Event Consumer Service Stopped");     // Registra un mensaje de información indicando que el servicio de consumo de eventos se está deteniendo.
            return Task.CompletedTask;
        }
    }
}
