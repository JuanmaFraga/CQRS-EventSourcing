using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.Events
{
    public class PostCreatedEvent : BaseEvent
    {
        public PostCreatedEvent() : base(nameof(PostCreatedEvent))      // El constructor de la clase PostCreatedEvent, que llama al constructor de la clase base BaseEvent y le pasa el nombre de la clase como string, para que se asigne a la propiedad Type de la clase BaseEvent.
        {

        }

        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
