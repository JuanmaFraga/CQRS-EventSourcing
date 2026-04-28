using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Post.Query.Domain.Entities
{
    [Table("Post")]                         // Linkeamos la entidad con la tabla Post
    public class PostEntity
    {
        [Key]
        public Guid PostId { get; set; }

        public string Author { get; set; }
        public DateTime DatePosted { get; set; }
        public string Message { get; set; }
        public int Likes { get; set; }
        //Pedimos una ICollection así luego Entity Framework se encarga de manejarlo como una List, un HashSet, etc. dependiendo de la implementación que usemos. Lo importante es que sea una colección para poder agregar y eliminar comentarios fácilmente.
        public virtual ICollection<CommentEntity> Comments { get; set; }    // Para agregar comentarios necesitamos usar Composición, y Navigation properties de Entity Framework. La entidad Comment va a ser una Navigation property de la entidad Post, y para usar Navigation Properties la propiedad tiene que ser virtual.
        // virtual se usa para utilizar Lazy Loading, lo que significa que los comentarios asociados a un post no se cargarán automáticamente cuando se recupere el post desde la base de datos, sino que se cargarán de manera diferida sólo cuando se acceda a la propiedad Comments. Esto puede mejorar el rendimiento al evitar la carga innecesaria de datos relacionados hasta que realmente se necesiten.

    }
}
