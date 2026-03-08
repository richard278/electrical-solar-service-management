using Microsoft.Extensions.Configuration;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using System.Net;
using System.Net.Mail;

namespace EcoInstalacion.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<bool> EnviarCorreo(string? correoDestino, string? asunto, string? mensaje)
        {
            try
            {
                List<Configuracion> query = await _repositorio.Consultar(
                c => !string.IsNullOrEmpty(c.Recurso) &&
                c.Recurso.Equals("Servicio_Correo", StringComparison.OrdinalIgnoreCase));

                Dictionary<string, string> Config = query
                .Where(c => c.Propiedad != null && c.Valor != null)
                .ToDictionary(c => c.Propiedad!, c => c.Valor!);


                var credenciales = new NetworkCredential(Config["correo"], Config["clave"]);

                var correo = new MailMessage()
                {
                    From = new MailAddress(Config["correo"], Config["alias"]),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(new MailAddress(correoDestino));

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = Config["host"];
                    smtp.Port = int.Parse(Config["puerto"]);
                    smtp.Credentials = credenciales;
                    smtp.EnableSsl = bool.Parse(Config["ssl"]);

                    await smtp.SendMailAsync(correo);
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                return false;
            }
        }
    }
}