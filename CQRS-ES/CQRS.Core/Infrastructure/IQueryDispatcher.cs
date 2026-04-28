using CQRS.Core.Commands;
using CQRS.Core.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace CQRS.Core.Infrastructure
{                                                               // Utilizada en el patron Mediator. Interfaz que define el contrato para un despachador de comandos, que es responsable de registrar handlers para comandos específicos y enviar comandos para su procesamiento.
    public interface IQueryDispatcher<TEntity>                // TEntity es un Tipo genérico (como T) que sea Entity
    {
        void RegisterHandler<TQuery>(Func<TQuery, Task<List<TEntity>>> handler) where TQuery : BaseQuery;       // Registranda una función (handler) que procesa una query y devuelve una lista de resultados del tipo TEntity, donde TQuery tiene que heredar de BaseQuery
                                                                                                                // La Func delegate recibe una función que: toma un TQuery y devuelve un Task<List<TEntity>>, porque podemos devolver varios posts dependiendo la operación.
                                                                                                                // Defino un método para registrar un handler que: maneja un tipo de query específico (TQuery), recibe esa query y devuelve una lista de entidades (de un tipo TEntity) de forma asincrónica, siempre que esa query herede de BaseQuery.
        Task<List<TEntity>> SendAsync(BaseQuery query);                                        // Envía un comando para su procesamiento.
                                                                                // Toma un objeto de tipo BaseQuery como parámetro, que representa el comando que se desea enviar.
    }
}
