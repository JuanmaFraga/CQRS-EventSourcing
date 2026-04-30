using CQRS.Core.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Events
{
    public abstract class BaseEvent : Message
    {
        protected BaseEvent(string type)        // El CONSTRUCTOR de la clase base. Nos aseguramos que recibe el TIPO del evento, que es un string que representa el nombre de la clase del evento, por ejemplo "PostCreatedEvent", "CommentAddedEvent", etc.
        {                                       // y luego asignamos ese TIPO a la propiedad Type de la clase BaseEvent, para que cuando deserializamos el evento podamos saber que tipo de evento es y deserializarlo correctamente.
            Type = type;
        }

        public string Type { get; set; }        //Discriminator para saber que tipo de evento es, ya que el aggregate va a tener una lista de eventos y cada uno va a ser de un tipo diferente
                                                // Cuando deserializamos el evento hacemos databinding polimorfico, es decir, si el tipo del evento es "PostCreatedEvent" entonces se va a deserializar como un objeto de la clase PostCreatedEvent, si el tipo del evento es "CommentAddedEvent" entonces se va a deserializar como un objeto de la clase CommentAddedEvent, etc.
        public int Version { get; set; }        //La usamos cuando hacemos replay del último estado del aggregate
    }
}
