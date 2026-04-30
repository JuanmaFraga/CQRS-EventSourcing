using CQRS.Core.Handlers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Api.Commands
{
    public class CommandHandler : ICommandHandler           // Representa el Concrete Colleague en el patrón Mediator, implementando la interfaz ICommandHandler para manejar la lógica de procesamiento de comandos específicos relacionados con las operaciones de publicación, edición, me gusta, comentarios y eliminación de publicaciones. Cada método HandleAsync se encargará de procesar un comando específico.
    {
        private readonly IEventSourcingHandler<PostAggregate> _eventSourcingHandler;            // Para manejar los objetos de command

        public CommandHandler(IEventSourcingHandler<PostAggregate> eventSourcingHandler)        // Constructor que inyecta la dependencia del Event Sourcing Handler, permitiendo que el Command Handler interactúe con el Event Store para recuperar y persistir el estado del agregado PostAggregate a través de eventos.
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task HandleAsync(NewPostCommand command)                                       // Crea una nueva instancia del Aggregate
        {
            var aggregate = new PostAggregate(command.Id, command.Author, command.Message);        // Crea una nueva instancia del agregado PostAggregate utilizando los datos proporcionados en el comando NewPostCommand, lo que representa la creación de una nuevo post en el sistema.
            
            await _eventSourcingHandler.SaveAsync(aggregate);                                       // Guarda el estado del agregado en el Event Store utilizando el Event Sourcing Handler.
        }

        public async Task HandleAsync(EditMessageCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando EditMessageCommand, lo que permite obtener la información actual del post antes de realizar la edición.
            // No chequeamos si encontramos el aggregate porque el método GetByIdAsync lanza una excepción si no encuentra el aggregate, lo que indica que el post que se intenta editar no existe en el sistema.
            
            aggregate.EditMessage(command.Message);                                                        // Llama al método EditMessage del agregado para actualizar el mensaje del post con el nuevo contenido proporcionado en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste los cambios realizados en el mensaje del post.
        }

        public async Task HandleAsync(LikePostCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando LikePostCommand, lo que permite obtener la información actual del post antes de realizar la acción de "me gusta".
            
            aggregate.LikePost();                                                        // Llama al método LikePost del agregado para registrar que un usuario ha dado "me gusta" al post, utilizando el nombre de usuario proporcionado en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste la acción de "me gusta" realizada por el usuario en el post.
        }

        public async Task HandleAsync(AddCommentCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando AddCommentCommand, lo que permite obtener la información actual del post antes de agregar un nuevo comentario.

            aggregate.AddComment(command.Comment, command.Username);                                                        // Llama al método AddComment del agregado para agregar un nuevo comentario al post, utilizando el contenido del comentario y el nombre de usuario proporcionados en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste la adición del nuevo comentario al post.
        }

        public async Task HandleAsync(EditCommentCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando EditCommentCommand, lo que permite obtener la información actual del post antes de editar un comentario existente.

            // No chequeamos si el username del comando es el mismo que el del comentario porque el método EditComment lanza una excepción si el username no coincide, lo que indica que el usuario que intenta editar el comentario no es el autor original del comentario.
            aggregate.EditComment(command.CommentId, command.Comment, command.Username);                                                        // Llama al método EditComment del agregado para editar un comentario existente en el post, utilizando el ID del comentario, el nuevo contenido del comentario y el nombre de usuario proporcionados en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste los cambios realizados en el comentario editado del post.
        }

        public async Task HandleAsync(RemoveCommentCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando RemoveCommentCommand, lo que permite obtener la información actual del post antes de eliminar un comentario existente.

            // No chequeamos si el username del comando es el mismo que el del comentario porque el método RemoveComment lanza una excepción si el username no coincide, lo que indica que el usuario que intenta eliminar el comentario no es el autor original del comentario.
            aggregate.RemoveComment(command.CommentId, command.Username);                                                        // Llama al método RemoveComment del agregado para eliminar un comentario existente en el post, utilizando el ID del comentario y el nombre de usuario proporcionados en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste la eliminación del comentario del post.
        }

        public async Task HandleAsync(DeletePostCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);                      // Recupera el estado actual del agregado PostAggregate desde el Event Store utilizando el ID proporcionado en el comando DeletePostCommand, lo que permite obtener la información actual del post antes de eliminarlo.

            // No chequeamos si el username del comando es el mismo que el del post porque el método DeletePost lanza una excepción si el username no coincide, lo que indica que el usuario que intenta eliminar el post no es el autor original del post.
            aggregate.DeletePost(command.Username);                                                        // Llama al método DeletePost del agregado para eliminar el post, utilizando el nombre de usuario proporcionado en el comando.
            await _eventSourcingHandler.SaveAsync(aggregate);                                                   // Guarda el estado actualizado del agregado en el Event Store, lo que persiste la eliminación del post.
        }

        public async Task HandleAsync(RestoreReadDbCommand command)                  // Manejamos el comando para Republicar a KAFKA todos los eventos del EventStore
        {
            await _eventSourcingHandler.RepublishEventsAsync();                    // Le pasamos la tarea al Event Sorucing Handler donde se envía a Kafka
        }
    }
}
