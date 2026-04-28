using CQRS.Core.Commands;
using CQRS.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Cmd.Infrastructure.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher                     // Al implementar el patron Mediator, este es el Concrete Mediator, que implementa la interfaz ICommandDispatcher. Es responsable de registrar handlers para comandos específicos y enviar comandos para su procesamiento.
    {
        private readonly Dictionary<Type, Func<BaseCommand, Task>> _handlers = new();   // new Dictionary<Type, Func<BaseCommand, Task>>();  <- Desde c#11 es equivalente a esto
                                                                                        // Este diccionario almacena los handlers registrados para cada tipo de comando, como funciones delegadas. La clave es el tipo del comando y el valor es una función que toma un comando y devuelve una tarea.
                                                                                        // el "_" antes del nombre de la variable es una convención común en C# para indicar que es un campo PRIVADO, sólo una convención.
                                                                                        //  Ejemplo de funcion delegada Func<int, int> duplicar = x => x * 2; Esta función delegada toma un entero y devuelve el resultado de multiplicarlo por 2. En este caso, el tipo de la función delegada es Func<int, int>, lo que significa que toma un entero como entrada y devuelve un entero como salida.
        public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand         // Lo llamamos en el Program.cs de la Api para registrar los Handlers al iniciar.
        {
            if (_handlers.ContainsKey(typeof(T)))           // Si el handler ya está en nuestro diccionario de handlers
            {
                throw new IndexOutOfRangeException($"You cannot register the same command handler {typeof(T).Name} twice!");
            }
            else 
            { 
                _handlers.Add (typeof(T), (command) => handler((T)command));   // Agrega el handler al diccionario, asociándolo con el tipo de comando correspondiente.
                                                                               // La función delegada se encarga de convertir el comando genérico a su tipo específico antes de llamar al handler.
            }
        }

        public async Task SendAsync(BaseCommand command)                             // Método que hace el dispatch del comando, es decir, envía el comando para su procesamiento. Toma un comando como parámetro y busca el handler registrado para ese tipo de comando en el diccionario.
        {
            if(_handlers.TryGetValue(command.GetType(), out Func<BaseCommand, Task> handler))       // TryGetValue busca en el Diccionario si existe una clave, y existe devuelve el valor asociado.
                                                                                                    // Si encuentra un handler para el tipo de comando, lo ejecuta, pasando el comando como argumento.
            {
                await handler(command);
            }
            else
            {
                throw new ArgumentNullException(nameof(handler), "No command handler was registered!");  // Si no encuentra un handler para el tipo de comando, lanza una excepción indicando que no se registró un handler para ese tipo de comando.
            }
        }
    }
}
