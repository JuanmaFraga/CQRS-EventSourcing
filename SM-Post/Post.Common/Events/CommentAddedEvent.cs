using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.Events
{
    public class CommentAddedEvent : BaseEvent      // Evento que se dispara cuando se crea el AddCommentCommand, es decir, cuando se agrega un comentario a un post.
    {
        public CommentAddedEvent() : base(nameof(CommentAddedEvent))      // El constructor de la clase CommentAddedEvent, que llama al constructor de la clase base BaseEvent y le pasa el nombre de la clase como string, para que se asigne a la propiedad Type de la clase BaseEvent.
        {

        }

        public Guid CommentId { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public DateTime CommentDate { get; set; }
    }
}
