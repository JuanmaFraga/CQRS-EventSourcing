using System;
using System.Collections.Generic;
using System.Text;

namespace Post.Common.DTOs
{
    public class BaseResponse           // DTOs son objetos simples de C# (clases) que se utilizan para transferir datos entre diferentes capas o componentes de una aplicación. 
    {                                   // En este caso, BaseResponse es una clase base que puede ser utilizada como punto de partida para crear respuestas específicas para diferentes tipos de solicitudes en la API de consulta (Query API). Al heredar de BaseResponse, las clases de respuesta específicas pueden incluir propiedades adicionales según sea necesario, mientras que BaseResponse puede contener propiedades comunes a todas las respuestas, como un mensaje de estado o un código de error.
        public string Message { get; set; }     // Propiedad común que puede ser utilizada para incluir un mensaje de estado o información adicional en la respuesta. Las clases de respuesta específicas pueden heredar esta propiedad y agregar otras propiedades según sea necesario para proporcionar información más detallada sobre la respuesta a una solicitud específica.
    }
}
