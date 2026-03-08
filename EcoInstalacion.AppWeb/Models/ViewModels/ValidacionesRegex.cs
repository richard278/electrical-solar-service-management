namespace EcoInstalacion.AppWeb.Models.ViewModels
{
    public static class ValidacionesRegex
    {
        // ------------------------------
        // Documento de Identidad (El Salvador)
        // ------------------------------

        // DUI salvadoreño: ########-#
        public const string DUI = @"^\d{8}-\d{1}$";

        // NIT salvadoreño (más flexible): ####-######-###-#
        public const string NIT = @"^\d{4}-\d{6}-\d{3}-\d$";

        // ------------------------------
        // Teléfonos
        // ------------------------------

        // Teléfono de 8 dígitos o formato ####-####
        public const string Telefono = @"^(\d{4}-\d{4}|\d{8})$";

        // Celular internacional opcional (+503 1234-5678 o 1234-5678)
        public const string TelefonoInternacional =
            @"^(?:\+?\d{1,3}\s?)?(?:\d{4}-\d{4}|\d{8})$";

        // ------------------------------
        // Cadenas de Texto
        // ------------------------------

        //Acepta solamente letras, espacios y tildes. Ideal para nombres.
        public const string SoloLetras =
            @"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$";

        // Letras, números y espacios (para descripciones simples)
        public const string AlfaNumericoBasico =
            @"^[A-Za-z0-9ÁÉÍÓÚáéíóúÑñ ]+$";

        /// Validación para direcciones flexibles
        public const string Direccion =
            @"^[A-Za-z0-9ÁÉÍÓÚáéíóúÑñ#,\-\. ]+$";

        // ------------------------------
        // Correo estándar
        // ------------------------------

        // Email estándar RFC (bastante completo)
        public const string Correo =
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$";


        /*-------------------------------------------------------------
        Fechas (cuando se quieran validar formato)
        NO siempre se recomienda usar regex en fechas,
        pero se incluye para formularios que no usen input type="date"
        -------------------------------------------------------------*/

        // Fecha en formato dd/MM/yyyy
        public const string Fecha_DDMMYYYY =
            @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}$";

        /// Fecha y hora dd/MM/yyyy HH:mm
        public const string FechaHora_DDMMYYYY_HHMM =
            @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}\s(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$";
    }
}

/*
 * Validaciones para nombres, apellidos, textos generales, direcciones.
 * Validaciones para fechas (cuando apliquen).
 * Validaciones para correos, DUI, NIT salvadoreño, teléfonos, cadenas alfanuméricas,
 */