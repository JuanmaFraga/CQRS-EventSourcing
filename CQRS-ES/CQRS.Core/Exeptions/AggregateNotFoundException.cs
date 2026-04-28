using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Core.Exeptions
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(string message ) : base( message ) { }
    }
}
