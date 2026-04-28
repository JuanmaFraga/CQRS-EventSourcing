using CQRS.Core.Events;
using Post.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Post.Query.Infrastructure.Converters
{
    public class EventJsonConverter : JsonConverter<BaseEvent>                       // Convertidor del evento JSON polimorfico que llega, que determina en cual de las formas concretas que derivan de Base Event se debe deserializar el evento en forma de JSON
    {
        public override bool CanConvert(Type typeToConvert)                             // Sobrescribe el método CanConvert para determinar si el tipo a convertir es un tipo que se puede convertir utilizando este convertidor. En este caso, verifica si el tipo a convertir es un tipo que deriva de BaseEvent.
        {
            return typeToConvert.IsAssignableFrom(typeof(BaseEvent));
        }

        public override BaseEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!JsonDocument.TryParseValue(ref reader, out var doc))                 // si el valor no puede ser parseado como un documento JSON, devuelve null
                throw new JsonException($"Failed to parse {nameof(JsonDocument)}!");
            
            if (!doc.RootElement.TryGetProperty("Type", out var type))           // si el documento JSON no tiene una propiedad "Type", lanza una excepción
                throw new JsonException($"Could not detect the Type discriminator property!");      // En el campo "Type" de BaseEvent se espera que se encuentre el nombre del tipo concreto del evento, ej: "PostCreatedEvent", "PostUpdatedEvent", etc. para poder determinar a qué tipo concreto de evento se debe deserializar el JSON.
        
            var typeDiscriminator = type.GetString();          // obtiene el valor de la propiedad "Type" del documento JSON, que se espera que sea el nombre del tipo concreto del evento
            var json = doc.RootElement.GetRawText();              // obtiene el texto JSON sin procesar del documento JSON

            return typeDiscriminator switch
            {
                nameof(PostCreatedEvent) => JsonSerializer.Deserialize<PostCreatedEvent>(json, options),        // options es el JsonSerializerOptions que nos van a pasar a través del método Read
                nameof(MessageUpdatedEvent) => JsonSerializer.Deserialize<MessageUpdatedEvent>(json, options),        // si el valor de "Type" es "PostUpdatedEvent", deserializa el JSON en un objeto de tipo PostUpdatedEvent
                nameof(PostLikedEvent) => JsonSerializer.Deserialize<PostLikedEvent>(json, options),        // si el valor de "Type" es "PostLikedEvent", deserializa el JSON en un objeto de tipo PostLikedEvent
                nameof(CommentAddedEvent) => JsonSerializer.Deserialize<CommentAddedEvent>(json, options),        // si el valor de "Type" es "CommentAddedEvent", deserializa el JSON en un objeto de tipo CommentAddedEvent
                nameof(CommentUpdatedEvent) => JsonSerializer.Deserialize<CommentUpdatedEvent>(json, options),        // si el valor de "Type" es "CommentUpdatedEvent", deserializa el JSON en un objeto de tipo CommentUpdatedEvent
                nameof(CommentRemovedEvent) => JsonSerializer.Deserialize<CommentRemovedEvent>(json, options),        // si el valor de "Type" es "CommentRemovedEvent", deserializa el JSON en un objeto de tipo CommentRemovedEvent
                nameof(PostRemovedEvent) => JsonSerializer.Deserialize<PostRemovedEvent>(json, options),        // si el valor de "Type" es "PostRemovedEvent", deserializa el JSON en un objeto de tipo PostRemovedEvent
                _ => throw new JsonException($"{typeDiscriminator} is not supporter yet!")     // si el valor de "Type" no coincide con ningún tipo conocido, lanza una excepción
            };
        }

        public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        {
            // NO VAMOS A USAR EL CONVERTER PARA SERIALIZAR LOS EVENTOS, SOLO PARA DESERIALIZARLOS, POR LO QUE ESTE MÉTODO NO SE IMPLEMENTA
            throw new NotImplementedException();
        }
    }
}
