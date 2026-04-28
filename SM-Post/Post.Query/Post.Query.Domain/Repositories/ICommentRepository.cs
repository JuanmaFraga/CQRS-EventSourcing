using Post.Query.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Domain.Repositories
{
    public interface ICommentRepository             // Interfaz abstracta que representa las interacciones pon la tabla Comment en la base de datos de lectura (SQL)
    {
        Task CreateAsync(CommentEntity comment);          // Método para crear una nueva entidad de comentario en la base de datos de lectura. Recibe un objeto CommentEntity que contiene los datos del comentario a crear, y devuelve una tarea asíncrona que representa la operación de creación.

        Task UpdateAsync(CommentEntity comment);

        Task<CommentEntity> GetByIdAsync(Guid commentId);
        Task DeleteAsync(Guid commentId);
    }
}
