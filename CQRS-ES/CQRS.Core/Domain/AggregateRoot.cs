using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot                 // Una clase abtracta no puede instanciarse directamente si no que sirve como base para otras clases. No se puede hacer un new AggregateRoot() porque es abstracta, pero se puede hacer un new PostAggregate() que herede de AggregateRoot.
    {                                                   // Encargado de mantener la consistencia, es decir, elevar eventos del aggregate, aplicar cambios de estado del aggregate, gestionar los cambios no aplicados, poder hacer un replay del último estado del aggregate y determinar qué métodos invocar, etc.
        protected Guid _Id;
        private readonly List<BaseEvent> _changes = new List<BaseEvent>();      // Los Eventos se usan para registrar cambios de estados en el Aggregate.

        public Guid Id                                  // Al escribirse así con el _Id como campo privado y el Id como propiedad pública de solo lectura, se garantiza que el Id solo se pueda asignar dentro de la clase o en las clases que hereden de AggregateRoot, pero no desde fuera de la clase. Esto es importante para mantener la integridad del Id, ya que el Id es un identificador único que no debe cambiar una vez asignado.
        {
            get { return _Id; }                         // Alternativamente se podría escribir como public Guid Id { get; protected set; } y eliminar el campo _Id, pero se ha optado por esta forma para tener un control más explícito sobre la asignación del Id.
        }

        public int Version { get; set; } = -1;          // Control de la versión del Aggregate, es decir, cuántos eventos se han aplicado al Aggregate. Se inicializa en -1 porque cuando se crea un nuevo Aggregate no se ha aplicado ningún evento, por lo que la versión es -1. Cuando se aplica el primer evento, la versión pasa a ser 0, luego 1, y así sucesivamente.

        public IEnumerable<BaseEvent> GetUncommittedChanges()      // Método para obtener los eventos que se han registrado pero que aún no se han guardado en la base de datos. Esto es útil para saber qué cambios se han hecho en el Aggregate antes de persistirlos.
        {                                                          // IEnumerable representa una colección que podés recorrer. Podes hacer un FOREACH sobre el resultado de este método para obtener cada evento registrado.
            return _changes;
        }

        public void MarkChangesAsCommitted()                      // Método para marcar los cambios como comprometidos, es decir, para limpiar la lista de eventos registrados después de que se hayan guardado en la base de datos. Esto es importante para evitar que los mismos eventos se vuelvan a guardar o se vuelvan a aplicar al Aggregate.
        {
            _changes.Clear();
        }


        private void ApplyChange(BaseEvent @event, bool isNew)     // Método para aplicar un cambio al Aggregate, es decir, para ejecutar la lógica de negocio que corresponde a cada evento. El parámetro isNew indica si el evento es nuevo (es decir, si se ha registrado recientemente) o si es un evento antiguo que se está aplicando al cargar el Aggregate desde la base de datos.
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });     // GetMethod(..) está buscando un método público llamado "Apply" y que tenga como parámetro el tipo del evento que se está aplicando. Este método debe estar definido en la clase que hereda de AggregateRoot y debe contener la lógica de negocio para manejar ese evento específico.
                                                                                                 //@event: Se usa @ porque event es palabra reservada en C#
                                                                                                 //this.GetType() Devuelve el tipo real del objeto en tiempo de ejecución
                                                                                                 //Apply actualiza el estado del Aggregate en base a un evento específico, por ejemplo, si el evento es PostCreatedEvent, el método Apply(PostCreatedEvent @event) podría asignar el título y el contenido del post al Aggregate. Si el evento es CommentAddedEvent, el método Apply(CommentAddedEvent @event) podría agregar un nuevo comentario a la lista de comentarios del post. Cada evento tiene su propio método Apply que se encarga de actualizar el estado del Aggregate de acuerdo a la información contenida en el evento.
                                                                                                 //Reflexion es la capacidad de un programa de inspeccionar y manipular su propio código en tiempo de ejecución

            if (method == null)
            {
                throw new ArgumentException(nameof(method), $"The Apply method not found in the Aggregate for {@event.GetType().Name}!");
            }
            else
            {
                method.Invoke(this, new object[] { @event });     // Invoke(..) ejecuta el método encontrado en la línea anterior, pasando el evento como argumento. Esto permite que se ejecute la lógica de negocio correspondiente al evento y se actualice el estado del Aggregate en consecuencia.
            }

            if (isNew)
            {
                _changes.Add(@event);                           // Si es un evento nuevo, se agrega a la lista de cambios no commited para que luego pueda ser persistido en la base de datos.
            }
        }

        protected void RaiseEvent(BaseEvent @event)           // Método protegido para registrar un nuevo evento, es decir, para aplicar un cambio al Aggregate y marcarlo como un cambio nuevo que debe ser guardado en la base de datos.
        {
            ApplyChange(@event, true);                      // Se llama al método ApplyChange con el evento y el parámetro isNew en true, lo que indica que es un evento nuevo que se debe agregar a la lista de cambios no commited.
        }

        public void ReplayEvents(IEnumerable<BaseEvent> events)       // Método público para reproducir una serie de eventos, es decir, para aplicar una secuencia de eventos al Aggregate. Esto es útil para reconstruir el estado del Aggregate a partir de su historial de eventos almacenados en la base de datos.
        {
            foreach (var @event in events)
            {
                ApplyChange(@event, false);                 // Se llama al método ApplyChange con cada evento y el parámetro isNew en false, lo que indica que son eventos antiguos que se están aplicando al cargar el Aggregate desde la base de datos, por lo que no se deben agregar a la lista de cambios no commited.
            }
        }
    }
}
