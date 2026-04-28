using Post.Common.Events;
using Post.Query.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.Handlers
{
    public interface IEventHandler                      // Interfaz abstracta que maneja los eventos luego de ser consumidos desde Kafka y luego invoca los métodos correspondientes en los repositorios.
    {
        Task On(PostCreatedEvent @event);          // On significa: “cuando ocurra este evento, ejecutá esta lógica”. Ej: public void On(PostCreatedEvent e) {..}, lo que se traduce a: “cuando ocurra un evento de tipo PostCreatedEvent, ejecutá esta lógica”.
        Task On(MessageUpdatedEvent @event);
        Task On(PostLikedEvent @event);
        Task On(PostRemovedEvent @event);
        Task On(CommentAddedEvent @event);
        Task On(CommentUpdatedEvent @event);
        Task On(CommentRemovedEvent @event);
    }
}
