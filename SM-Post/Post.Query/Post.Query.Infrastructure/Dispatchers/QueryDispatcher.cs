using CQRS.Core.Infrastructure;
using CQRS.Core.Queries;
using Post.Query.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.Dispatchers
{                                                         
    public class QueryDispatcher : IQueryDispatcher<PostEntity>       // Al implementar el patron Mediator, este es el Concrete Mediator, que implementa la interfaz IQueryDispatcher. Es responsable de registrar handlers para comandos específicos y enviar comandos para su procesamiento.
    {                                                                   // Nuestro tipo TEntity es PostEntity
        private readonly Dictionary<Type, Func<BaseQuery, Task<List<PostEntity>>>> _handlers = new();   // Para el retorno, siempre queremos que el Handler que se haga cargo, retorne una lista de elementos del tipo  PostEntity

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<PostEntity>>> handler) where TQuery : BaseQuery      // Lo llamamos en el Program.cs de la Api para registrar los Handlers al iniciar.
        {
            if(_handlers.ContainsKey(typeof(TQuery)))       // Si nuestro handler ya contiene el tipo especifico de Query Type
            {
                throw new IndexOutOfRangeException("You cannot register the same query handler twice!");
            }
            // else, agrego al Diccionario 
            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));         // x => handler((TQuery)x) => Recibo un object (x), lo convierto a TQuery, y ejecuto el handler original
                                                                            // RegisterHandler<FindAllPostQuery>(handler) => query entra como object (x) se castea a el tipo de query (ej FindAllPostQuery), se ejecuta el handler correcto
        }

        public async Task<List<PostEntity>> SendAsync(BaseQuery query)        // Metodo que despacha el query object
        {
            if(_handlers.TryGetValue(query.GetType(), out Func<BaseQuery, Task<List<PostEntity>>> handler))      // Si tenemos registrado el handler para el query específico lo ejecutamos
            {
                return await handler(query);
            }
            // Si no
            throw new ArgumentNullException(nameof(handler), "No query handler was registered!");
        }
    }
}
