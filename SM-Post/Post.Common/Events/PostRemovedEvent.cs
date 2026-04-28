using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.Events
{
    public class PostRemovedEvent : BaseEvent            // Evento que se dispara cuando se crea el DeletePostCommand, es decir, cuando se elimina un post.
    {
        public PostRemovedEvent() : base(nameof(PostRemovedEvent))     // El constructor de la clase PostRemovedEvent, que llama al constructor de la clase base BaseEvent y le pasa el nombre de la clase como string, para que se asigne a la propiedad Type de la clase BaseEvent.
        {

        }
    }
}
