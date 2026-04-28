using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Queries
{
    public abstract class BaseQuery         // No se puede crear una instancia de un objeto abstracto (no se puede hacer new BaseQuery ())
    {
        // No especificamos por ej el Id, porque por ej FindAllPostQuery no necesita ni siquiera el Id al traer todos los post.
        // La hacemos abstract también para que podamos usar polimorfismo
    }
}
