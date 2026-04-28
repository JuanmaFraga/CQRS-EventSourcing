using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.DataAccess
{
    public class DatabaseContext : DbContext                // DBContex representa una sesión con la base de datos y se utiliza para realizar operaciones de consulta y guardado en la base de datos a través de Entity Framework Core.
    {
        public DatabaseContext(DbContextOptions options) : base(options)       // Llama al constructor de la clase base (DbContext) y pasale options
        {

        }

        //Los DbSet se usan para representar las tablas de la base de datos y permiten realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) y guardar instancias de las entidades en la base de datos.
        //Cada DbSet corresponde a una tabla específica en la base de datos, y cada instancia de la entidad representa una fila en esa tabla.
        public DbSet<PostEntity> Posts { get; set; }     // Representa la tabla "Posts" en la base de datos, donde cada instancia de PostEntity corresponde a una fila en esa tabla. DbSet es una colección que permite realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre los datos de la tabla "Posts" utilizando Entity Framework Core.
        public DbSet<CommentEntity> Comments { get; set; }     // Representa la tabla "Comments" en la base de datos, donde cada instancia de CommentEntity corresponde a una fila en esa tabla. DbSet es una colección que permite realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre los datos de la tabla "Comments" utilizando Entity Framework Core.

    }
}
