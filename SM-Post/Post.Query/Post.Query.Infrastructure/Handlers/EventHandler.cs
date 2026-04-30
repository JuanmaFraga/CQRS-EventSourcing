using Post.Common.Events;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IPostRepository _postRepository;                   // Dependency injection of the post repository
        private readonly ICommentRepository _commentRepository;             // Dependency injection of the comment repository

        public EventHandler(IPostRepository postRepository, ICommentRepository commentRepository)       // Constructor para inyectar las dependencias de los repositorios
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        public async Task On(PostCreatedEvent @event)
        {
            var post = new PostEntity
            {
                PostId = @event.Id,
                Author = @event.Author,
                DatePosted = @event.DatePosted,
                Message = @event.Message
            };

            await _postRepository.CreateAsync(post);
        }

        public async Task On(MessageUpdatedEvent @event)
        {
            var post = await _postRepository.GetByIdAsync(@event.Id);

            if (post == null) return; // Si el post no existe, no hacemos nada

            post.Message = @event.Message;

            await _postRepository.UpdateAsync(post);
        }

        public async Task On(PostLikedEvent @event)
        {
            //var post = await _postRepository.GetByIdAsync(@event.Id);

            //if (post == null) return; // Si el post no existe, no hacemos nada

            //post.Likes++;

            //await _postRepository.UpdateAsync(post);

            await _postRepository.LikePostAsync(@event.Id);     // Pasamos la lógica del like a una operación de repo en particular que trae y suma en un sólo paso de db para evitar race conditions entre get y update para varios like seguidos
        }

        public async Task On(CommentAddedEvent @event)
        {
            var comment = new CommentEntity
            {
                PostId = @event.Id,             // Asumimos que el Id del evento es el Id del post al que se le añade el comentario
                CommentId = @event.CommentId,
                CommentDate = @event.CommentDate,
                Comment = @event.Comment,
                Username = @event.Username,
                Edited = false      // Porque es un comentario nuevo
            };

            await _commentRepository.CreateAsync(comment);
        }

        public async Task On(CommentUpdatedEvent @event)
        {
            var comment = await _commentRepository.GetByIdAsync(@event.CommentId);

            if (comment == null) return; // Si el comentario no existe, no hacemos nada

            comment.Comment = @event.Comment;
            comment.Edited = true;
            comment.CommentDate = @event.EditDate; // Actualizamos la fecha del comentario a la fecha de edición

            await _commentRepository.UpdateAsync(comment);
        }

        public async Task On(CommentRemovedEvent @event)
        {
            await _commentRepository.DeleteAsync(@event.CommentId);     // Si el comentario no existe, el repositorio se encargará de manejarlo (puede lanzar una excepción o simplemente no hacer nada)
        }

        public async Task On(PostRemovedEvent @event)
        {
            await _postRepository.DeleteAsync(@event.Id);        // Borramos el Post CON los comentarios (porque los trajimos en el GetByIdAsync (con incluide))
        }
    }
}
