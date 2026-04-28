using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.Events
{
    public class CommentRemovedEvent : BaseEvent            // Evento que se dispara cuando se crea el RemoveCommentCommand, es decir, cuando se elimina un comentario de un post.
    {
        public CommentRemovedEvent() : base(nameof(CommentRemovedEvent))     // El constructor de la clase RemovedCommentEvent, que llama al constructor de la clase base BaseEvent y le pasa el nombre de la clase como string, para que se asigne a la propiedad Type de la clase BaseEvent.
        {
        }
        public Guid CommentId { get; set; }
    }
}
