using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Events
{
    public class EventModel                     // Entidad de MongoDB para modelar los eventos que guardamos en la BD
    {
        [BsonId]        // Marca esta propiedad como el identificador único del documento en MongoDB
        [BsonRepresentation(BsonType.ObjectId)]    // Indica que el Id se representa como un ObjectId en MongoDB, lo que permite que MongoDB genere automáticamente un Id único para cada evento almacenado.
        public string Id { get; set; }                          // MongoDB object Id
        public DateTime TimeStamp { get; set; }                 // Fecha y hora de cuando un evento es persistido en la base de datos.
        public Guid AggregateIdentifier { get; set; }           // El Id del Aggregate al que pertenece el evento, lo que nos permite agrupar eventos por Aggregate y reconstruir su estado a partir de su historial de eventos.
        public string AggregateType { get; set; }
        public int Version { get; set; }        
        public string EventType { get; set; }                 // El tipo de evento, que se utiliza para identificar el tipo específico de evento que se está almacenando. Esto es útil para la serialización y deserialización de eventos, así como para la lógica de manejo de eventos en el sistema.
        public BaseEvent EventData { get; set; }                            // El evento en sí mismo, que contiene la información específica del cambio que ocurrió en el Aggregate. Este campo es de tipo BaseEvent, lo que permite almacenar cualquier tipo de evento que herede de BaseEvent, lo que nos da flexibilidad para manejar diferentes tipos de eventos en el sistema.

    }
}
