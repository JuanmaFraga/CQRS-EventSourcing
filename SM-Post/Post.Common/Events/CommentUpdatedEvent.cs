using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.Events
{
    public class CommentUpdatedEvent : BaseEvent        // Evento que se dispara cuando se crea el EditCommentCommand, es decir, cuando se actualiza un comentario de un post.
    {
        public CommentUpdatedEvent() : base(nameof(CommentUpdatedEvent))     // El constructor de la clase CommentUpdatedEvent, que llama al constructor de la clase base BaseEvent y le pasa el nombre de la clase como string, para que se asigne a la propiedad Type de la clase BaseEvent.
        {

        }

        public Guid CommentId { get; set; }     
        public string Comment { get; set; }
        public string Username { get; set; }
        public DateTime EditDate { get; set; }
    }
}
