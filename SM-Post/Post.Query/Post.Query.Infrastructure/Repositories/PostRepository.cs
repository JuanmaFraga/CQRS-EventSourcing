using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository                         // Encargada de manejar las operaciones de acceso a datos relacionadas con las entidades de publicación. Esta clase implementa la interfaz IPostRepository, lo que garantiza que se proporcionen los métodos necesarios para crear, leer, actualizar y eliminar publicaciones, así como para listar publicaciones según diferentes criterios.
    {
        private readonly DatabaseContextFactory _contextFactory;

        public PostRepository(DatabaseContextFactory contextFactory)        // Constructor que inyecta una instancia de DatabaseContextFactory, lo que permite crear instancias de DatabaseContext para interactuar con la base de datos. Esto sigue el patrón de diseño Factory, que ayuda a gestionar la creación de objetos y a mantener el código más limpio y modular.
        {
            _contextFactory = contextFactory;
        }

        public async Task CreateAsync(PostEntity post)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                context.Posts.Add(post);
                _ = await context.SaveChangesAsync();                           // _ = es una forma de indicar que no se utilizará el resultado de la operación SaveChangesAsync(), lo que puede ser útil para evitar advertencias del compilador sobre variables no utilizadas.
            }
        }

        public async Task DeleteAsync(PostEntity post)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                context.Posts.Remove(post);
                _ = await context.SaveChangesAsync();
            }
        }

        public async Task<PostEntity> GetByIdAsync(Guid postId)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                return await context.Posts
                    .Include(p => p.Comments)                   // Incluimos los COMENTARIOS del Post a la hora de devolverlo ya que al usar lazy loading, sin el Include los comentarios se cargarán recién cuando se acceda a ellos y no al traer el Post.
                    .FirstOrDefaultAsync(x => x.PostId == postId);      // Necesitamos incluirlos para que al hacer el DELETE, Entity Frameworks haga el DELETE del Comentario también
            }
        }

        public async Task<List<PostEntity>> ListAllAsync()
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                return await context.Posts.AsNoTracking()       // AsNoTracking() se utiliza para mejorar el rendimiento al indicar a Entity Framework que no realice un seguimiento de los cambios en las entidades recuperadas, lo que es útil cuando solo se necesitan leer datos sin modificarlos.
                    .Include(p => p.Comments)                   // Incluimos los comentarios del Post a la hora de devolverlo ya que al usar lazy loading, sin el Include los comentarios se cargarán recién cuando se acceda a ellos y no al traer el Post.
                    .ToListAsync();
            }
        }

        public async Task<List<PostEntity>> ListByAuthorAsync(string author)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                return await context.Posts.AsNoTracking()
                    .Include(p => p.Comments)                   // Incluimos los comentarios del Post a la hora de devolverlo ya que al usar lazy loading, sin el Include los comentarios se cargarán recién cuando se acceda a ellos y no al traer el Post.
                    .Where(x => x.Author.Contains(author))      // Usamos contains o sea que podemos pasar parte del nombre del autor
                    .ToListAsync();
            }
        }

        public async Task<List<PostEntity>> ListWithCommentsAsync()
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                return await context.Posts.AsNoTracking()
                    .Include(p => p.Comments)                   // Incluimos los comentarios del Post a la hora de devolverlo ya que al usar lazy loading, sin el Include los comentarios se cargarán recién cuando se acceda a ellos y no al traer el Post.
                    .Where(x => x.Comments != null && x.Comments.Any())                    // Filtramos los posts para incluir solo aquellos que tienen al menos un comentario, utilizando el método Any() para verificar si la colección de comentarios no está vacía.
                    .ToListAsync();
            }
        }

        public async Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                return await context.Posts.AsNoTracking()
                    .Include(p => p.Comments)                   // Incluimos los comentarios del Post a la hora de devolverlo ya que al usar lazy loading, sin el Include los comentarios se cargarán recién cuando se acceda a ellos y no al traer el Post.
                    .Where(x => x.Likes >= numberOfLikes)
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(PostEntity post)
        {
            using (DatabaseContext context = _contextFactory.CreateDbContext())
            {
                context.Posts.Update(post);
                await context.SaveChangesAsync();
            }
        }
    }
}
