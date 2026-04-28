using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Messages
{
    public abstract class Message       // Al ser abstracta, impedís que alguien haga new Message(), lo cual no tendría ninguna utilidad porque un mensaje vacío no representa ninguna acción.
    {                                   // Obligamos a instanciar para poder crear una nueva entidad
        public Guid Id { get; set; }        // abstract Te permite escribir código que acepte un Message genérico (por ejemplo, en un Dispatcher o un Logger), pero que en realidad esté procesando un NewPostCommand. Podés tratar a todos los hijos por igual basándote en la base.
    }
}
