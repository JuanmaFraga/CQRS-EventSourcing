using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace Post.Query.Domain.Entities
{
    [Table("Comment")]
    public class CommentEntity
    {
        [Key]
        public Guid CommentId { get; set; }
        public string Username { get; set; }
        public DateTime CommentDate { get; set; }
        public string Comment { get; set; }
        public bool Edited { get; set; }
        public Guid PostId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]     // Evita que la propiedad Post se serialice al convertir el objeto CommentEntity a JSON, lo que puede ser útil para evitar problemas de referencia circular o para reducir la cantidad de datos enviados en una respuesta JSON.
        public virtual PostEntity Post { get; set; }     // Navigation property para establecer la relación entre CommentEntity y PostEntity. Esto permite que cada comentario esté asociado con un post específico, y facilita la navegación entre ambas entidades en el contexto de Entity Framework.
                                                         // La propiedad virtual permite el uso de Lazy Loading, lo que significa que los datos del post asociado se cargarán de manera diferida solo cuando se acceda a esta propiedad.
    }
}