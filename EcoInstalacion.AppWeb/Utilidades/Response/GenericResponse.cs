namespace EcoInstalacion.AppWeb.Utilidades.Response
{
    public class GenericResponse<TObject> 
    { 
        public bool Estado { get; set; }
        public string? Mensaje { get; set; }
        public TObject? Objeto { get; set; }
        public List<TObject>? Lista { get; set; }

    }
}

/*

🧩 Propósito

Estado: indica si la operación fue exitosa (true) o falló (false).
Mensaje: sirve para mensajes de error o confirmación.
Objeto: retorna una única entidad (por ejemplo, al crear o editar).
Lista: se usa cuando retornas colecciones (como Lista() en tus servicios).*/