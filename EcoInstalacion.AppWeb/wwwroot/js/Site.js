// Helper GLOBAL para obtener el token CSRF
window.getCsrfToken = function () {
    // Busca el formulario oculto del layout
    const form = document.getElementById('__AjaxAntiForgeryForm');
    if (!form) return '';

    const input = form.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
};

// Configuración global para llamadas jQuery AJAX
// (se ejecuta en TODAS las peticiones $.ajax, DataTables, etc.)
if (window.$) {
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            // Solo enviamos token en métodos que modifican (no GET)
            const isSafeMethod = /^(GET|HEAD|OPTIONS|TRACE)$/i.test(settings.type);
            if (isSafeMethod) return;

            const token = window.getCsrfToken && window.getCsrfToken();
            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }
        }
    });
}
