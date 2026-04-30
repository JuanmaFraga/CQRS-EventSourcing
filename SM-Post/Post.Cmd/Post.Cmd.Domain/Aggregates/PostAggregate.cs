using CQRS.Core.Domain;
using Post.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate : AggregateRoot                  // Cada PostAggregate representa UN post individual en el sistema. Usamos eventos para alterar el estado del Aggregate, lo que nos permite mantener un historial completo de cambios y facilitar la reconstrucción del estado del Aggregate a partir de su historial de eventos.
    {
        private bool _active;
        private string _author;                                 // El autor del post
        private readonly Dictionary<Guid, Tuple<string, string>> _comments = new(); //Los comentarios tienen un Id único (Guid), el contenido del comentario (string) y el nombre de usuario del autor del comentario (string).


        public bool Active
        {
            get => _active;             // Propiedad pública para acceder al estado de actividad del post. Se puede leer y escribir desde fuera de la clase, lo que permite activar o desactivar el post según sea necesario.
            set => _active = value;     // Equivalente a public bool Active { get { return _active; } set { _active = value; }  
        }

        public PostAggregate()              // Constructor vacío necesario para la deserialización de eventos al cargar el Aggregate desde la base de datos. Cuando se reconstruye el estado del Aggregate a partir de su historial de eventos, se necesita un constructor sin parámetros para crear una instancia del Aggregate y luego aplicar los eventos uno por uno para actualizar su estado.
        {

        }

        public PostAggregate(Guid id, string author, string message)     // Constructor público para crear una nueva instancia del PostAggregate con un Id, un autor y un mensaje. Este constructor se utiliza para crear un nuevo post desde cero, asignando un Id único, el nombre del autor y el contenido del mensaje.
        {
            RaiseEvent(new PostCreatedEvent                              // Se llama al método RaiseEvent que registra un nuevo evento de tipo PostCreatedEvent, que contiene la información del nuevo post. Este evento se aplicará al Aggregate para actualizar su estado y se agregará a la lista de cambios no commited para que luego pueda ser persistido en la base de datos.
            {
                Id = id,
                Author = author,
                Message = message,
                DatePosted = DateTime.Now
            });
        }

        public void Apply(PostCreatedEvent @event)     // Método Apply para manejar el evento PostCreatedEvent
        {                                              // Es el método "Apply" que busca el GetMethod de ApplyChanges de AggregateRoot
            _id = @event.Id;
            _active = true;                // Se asume que un post recién creado está activo por defecto.
            _author = @event.Author;
        }

        public void EditMessage(string message)     // Método público para editar el mensaje del post. Este método se puede llamar desde fuera de la clase para actualizar el contenido del post, y registra un nuevo evento de tipo PostEditedEvent que contiene la nueva información del mensaje.
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot edit the message of an inactive post!");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException($"The value of {nameof(message)} cannot be null or empty. Please provide a valid {nameof(message)}!");
            }                                                       // Usamos el operador nameof por si llegamos a cambiar el nombre del parametro message por algo como "post message"

            RaiseEvent(new MessageUpdatedEvent
            {
                Id = _id,
                Message = message
            });
        }

        public void Apply(MessageUpdatedEvent @event)     // Método Apply para manejar el evento MessageUpdatedEvent
        {
            _id = @event.Id;
        }

        public void LikePost()                         // No necesitamos el id del post porque el Aggregate ya tiene su propio Id que se asigna al crearse el post
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot like an inactive post!");
            }

            RaiseEvent(new PostLikedEvent
            {
                Id = _id
            });
        }

        public void Apply(PostLikedEvent @event)     // Método Apply para manejar el evento PostLikedEvent
        {
            _id = @event.Id;
        }

        public void AddComment(string comment, string username)     // Método público para agregar un comentario al post. Este método registra un nuevo evento de tipo CommentAddedEvent que contiene la información del comentario y el autor del mismo.
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot add a comment to an inactive post!");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new InvalidOperationException($"The value of {nameof(comment)} cannot be null or empty. Please provide a valid {nameof(comment)}!");
            }

            RaiseEvent(new CommentAddedEvent
            {
                Id = _id,
                CommentId = Guid.NewGuid(),
                Comment = comment,
                Username = username,
                CommentDate = DateTime.Now
            });
        }

        public void Apply(CommentAddedEvent @event)     // Método Apply para manejar el evento CommentAddedEvent
        {
            _id = @event.Id;
            _comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }

        public void EditComment(Guid commentId, string comment, string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot edit a comment of an inactive post!");
            }

            if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))        // Verificamos que el usuario que intenta editar el comentario sea el mismo que lo creó, comparando el nombre de usuario almacenado en el diccionario de comentarios con el nombre de usuario proporcionado como parámetro.
            {                                                                                                   //Los comentarios tienen un Id único (Guid), el contenido del comentario (string) y el nombre de usuario del autor del comentario (string).
                throw new InvalidOperationException("You are not allowed to edit a comment that was made by another user!");
            }

            RaiseEvent(new CommentUpdatedEvent
            {
                Id = _id,
                CommentId = commentId,
                Comment = comment,
                Username = username,
                EditDate = DateTime.Now
            });
        }

        public void Apply(CommentUpdatedEvent @event)     // Método Apply para manejar el evento CommentUpdatedEvent
        {
            _id = @event.Id;
            _comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.Username);
        }

        public void RemoveComment(Guid commentId, string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("You cannot remove a comment of an inactive post!");
            }

            if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to remove a comment that was made by another user!");
            }

            RaiseEvent(new CommentRemovedEvent
            {
                Id = _id,
                CommentId = commentId
            });
        }

        public void Apply(CommentRemovedEvent @event)     // Método Apply para manejar el evento RemovedCommentEvent
        {
            _id = @event.Id;
            _comments.Remove(@event.CommentId);
        }

        public void DeletePost(string username) 
        {
            if (!_active)
            {
                throw new InvalidOperationException("The post has already been removed!");
            }

            if (!_author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to delete a post that was made by another user!");
            }

            RaiseEvent(new PostRemovedEvent
            {
                Id = _id
            });
        }

        public void Apply(PostRemovedEvent @event)     // Método Apply para manejar el evento PostRemovedEvent
        {
            _id = @event.Id;
            _active = false;                // Al eliminar un post, simplemente lo marcamos como inactivo en lugar de eliminarlo físicamente de la base de datos, lo que nos permite mantener un historial completo de eventos relacionados con ese post.
        }
    }
}
