using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.DAL.Interfaces;
using EcoInstalacion.Entity;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Auth.Providers;

namespace EcoInstalacion.BLL.Implementacion
{
    public class FirebaseService : IFirebaseService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;
        public FirebaseService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<string> SubirStorage(Stream archivo, string carpetaDestino, string nombreArchivo)
        {
            string urlImagen = string.Empty;
            try
            {
                List<Configuracion> query = await _repositorio.Consultar(c => c.Recurso == "FireBase_Storage");
                // Conversión explícita con filtrado de nulos
                Dictionary<string, string> config = query
                    .Where(c => !string.IsNullOrEmpty(c.Propiedad) && !string.IsNullOrEmpty(c.Valor))
                    .Select(c => new { Key = c.Propiedad!, Value = c.Valor! })
                    .ToDictionary(c => c.Key, c => c.Value);

                FirebaseAuthConfig ConfigFB = new FirebaseAuthConfig();
                ConfigFB.ApiKey = config["api_key"];
                ConfigFB.AuthDomain = config["ruta"];
                ConfigFB.Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                };
                Firebase.Auth.IFirebaseAuthClient auth = new FirebaseAuthClient(ConfigFB);

                var authLink = await auth.SignInWithEmailAndPasswordAsync(config["email"], config["clave"]);
                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    config["ruta"],
                    new FirebaseStorageOptions
                    {
                        //AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child(carpetaDestino)
                    .Child(nombreArchivo)
                    .PutAsync(archivo, cancellation.Token);

                urlImagen = await task;
            }
            catch (Exception)
            {
                urlImagen = string.Empty;
            }

            return urlImagen;
        }

        public async Task<bool> EliminarStorage(string rutaArchivo, string nombreArchivo)
        {
            try
            {
                List<Configuracion> query = await _repositorio.Consultar(c => c.Recurso == "FireBase_Storage");
                // Conversión explícita con filtrado de nulos
                Dictionary<string, string> config = query
                    .Where(c => !string.IsNullOrEmpty(c.Propiedad) && !string.IsNullOrEmpty(c.Valor))
                    .Select(c => new { Key = c.Propiedad!, Value = c.Valor! })
                    .ToDictionary(c => c.Key, c => c.Value);

                FirebaseAuthConfig ConfigFB = new FirebaseAuthConfig();
                ConfigFB.ApiKey = config["api_key"];
                ConfigFB.AuthDomain = config["ruta"];
                ConfigFB.Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                };
                Firebase.Auth.IFirebaseAuthClient auth = new FirebaseAuthClient(ConfigFB);

                var authLink = await auth.SignInWithEmailAndPasswordAsync(config["email"], config["clave"]);
                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    config["ruta"],
                    new FirebaseStorageOptions
                    {
                        //AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child(rutaArchivo)
                    .Child(nombreArchivo)
                    .DeleteAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
