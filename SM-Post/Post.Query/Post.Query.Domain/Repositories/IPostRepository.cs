using Post.Query.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Domain.Repositories
{
    public interface IPostRepository                // Interfaz abstracta que representa las interacciones pon la tabla Post en la base de datos de lectura (SQL)
    {
        Task CreateAsync(PostEntity post);          // Método para crear una nueva entidad de publicación en la base de datos de lectura. Recibe un objeto PostEntity que contiene los datos de la publicación a crear, y devuelve una tarea asíncrona que representa la operación de creación.

        Task UpdateAsync(PostEntity post);

        Task DeleteAsync(Guid postId);

        Task LikePostAsync(Guid postId);    // Pasamos la lógica del like a una operación de repo en particular que trae y suma en un sólo paso de db para evitar race conditions entre get y update para varios like seguidos

        Task<PostEntity> GetByIdAsync(Guid postId);     // Método para obtener una entidad de publicación por su ID desde la base de datos de lectura. Recibe un GUID que representa el ID de la publicación a buscar, y devuelve una tarea asíncrona que representa la operación de búsqueda, con un resultado de tipo PostEntity que contiene los datos de la publicación encontrada.

        Task<List<PostEntity>> ListAllAsync();

        Task<List<PostEntity>> ListByAuthorAsync(string author);

        Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes);

        Task<List<PostEntity>> ListWithCommentsAsync();
    }
}
