using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Query.Infrastructure.DataAccess
{                                                                   // Factory es un patrón de diseño que delega la creación de objetos a una clase especializada para no exponer la creación.
                                                                    // var context = factory.CreateDbContext(); en vezd de var context = new DatabaseContext(options);
    public class DatabaseContextFactory                             // Clase encargada de crear instancias de DatabaseContext ya configuradas.
    {
        // Action es un delegado que representa un método que no devuelve un valor y puede aceptar uno o más parámetros.
        // Un delegado es un un tipo que representa una función (método). Te permite guardar una función en una variable, pasarla como parámetro o ejecutarla después.
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;                 // Guarda una función de configuración para el DbContext, que se inyecta a través del constructor.

        public DatabaseContextFactory(Action<DbContextOptionsBuilder> configureDbContext)     // Constructor que recibe una acción de configuración para el DbContext, lo que permite personalizar la configuración de la base de datos según las necesidades específicas de la aplicación. Esta acción se inyecta a través del constructor, lo que facilita la flexibilidad y la reutilización del código al permitir que diferentes configuraciones de base de datos se apliquen sin necesidad de modificar el código del factory.
        {
            _configureDbContext = configureDbContext;
        }

        public DatabaseContext CreateDbContext()     // Método que crea y devuelve una nueva instancia de DatabaseContext utilizando la configuración proporcionada a través del constructor. Este método se encarga de construir las opciones del DbContext utilizando la acción de configuración inyectada, lo que permite que la instancia de DatabaseContext se configure correctamente antes de ser utilizada para interactuar con la base de datos.
        {
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new();     // Crea una nueva instancia de DbContextOptionsBuilder para configurar las opciones del DbContext específico (DatabaseContext en este caso).
            
            _configureDbContext(optionsBuilder);       // Llama a la acción de configuración inyectada para configurar las opciones del DbContext utilizando el optionsBuilder.
            
            return new DatabaseContext(optionsBuilder.Options);     // Devuelve una nueva instancia de DatabaseContext utilizando las opciones configuradas, lo que permite que el DbContext esté listo para ser utilizado en la aplicación para realizar operaciones de consulta y guardado en la base de datos.
        }
    }
}
