using CQRS.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Infrastructure
{
    public interface ICommandDispatcher                                             // Utilizada en el patron Mediator. Interfaz que define el contrato para un despachador de comandos, que es responsable de registrar handlers para comandos específicos y enviar comandos para su procesamiento.
    {
        void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand;       // Registra un handler para un tipo específico de comando.
                                                                                    // Toma una Func delegate Puede tomar una serie de parametros de entrada, pero el último parámetro llamado handler, siempre va a ser una parámetro de salida, del tipo Task (asincrona).
                                                                                    // El tipo de comando que maneja el handler se especifica mediante el parámetro de tipo genérico T, que debe ser una clase que herede de BaseCommand.
        Task SendAsync(BaseCommand command);                                        // Envía un comando para su procesamiento.
                                                                                    // Toma un objeto de tipo BaseCommand como parámetro, que representa el comando que se desea enviar.
    }
}
