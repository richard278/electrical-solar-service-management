# Diseño de Autenticación y Autorización API

Este documento detalla la planificación y el diseño de la seguridad para la API REST del proyecto **EcoInstalacion**, asegurando una transición controlada desde la arquitectura de vistas Razor hacia la arquitectura SPA multi-cliente.

## 1. Propósito del documento

Definir de manera formal el esquema de autenticación, autorización, protección de endpoints y el flujo de sesión requerido para que las aplicaciones cliente (React + Vite y futuras apps móviles) interactúen de forma segura con el backend ASP.NET Core, manteniendo al mismo tiempo la convivencia armónica con el sistema heredado.

## 2. Estado Actual de Autenticación

Actualmente coexisten dos enfoques de presentación en el backend:

* **Presentación MVC/Razor (Heredada)**: Emplea autenticación basada en cookies tradicionales (`CookieAuthenticationDefaults.AuthenticationScheme`). Los roles (`Administrador` y `Técnico`) están mapeados y aplicados mediante atributos `[Authorize]` tradicionales en los controladores MVC.
* **Presentación API REST (Modernización)**: Las APIs de Clientes, Técnicos, Roles, Solicitudes y Visitas Técnicas están expuestas mediante controladores API (`[ApiController]`). Al nivel de clase, se aplica el atributo `[IgnoreAntiforgeryToken]` para facilitar el consumo local asíncrono desde el frontend React (puerto `5173`). No obstante, en esta fase preliminar de integración local, **los endpoints de la API carecen de validación de credenciales y no están protegidos**.
* **Usuarios/Auth**: Las operaciones sobre usuarios y el proceso de autenticación en sí no han sido expuestos como servicios API públicos en este sprint para proteger la integridad de las credenciales de prueba.

---

## 3. Riesgos Controlados y Declaración de Estado

> [!WARNING]
> Las APIs REST en el estado actual de integración local están diseñadas exclusivamente para desarrollo y pruebas. No deben considerarse bajo ningún escenario como listas para producción.
>
> Antes de su despliegue a entornos externos o productivos, se debe implementar de forma mandatoria la seguridad JWT o de Cookies HttpOnly según lo estipulado en este diseño.

---

## 4. Objetivo de Seguridad API-first

La meta principal es proveer un mecanismo de autenticación para servicios REST que cumpla con los siguientes principios:

1. **Desacoplamiento**: El cliente React o móvil no debe requerir redireccionamientos a páginas de login en servidor. La sesión debe orquestarse mediante API REST asíncrona.
2. **Modularidad**: El servidor debe validar las credenciales y emitir una prueba digital de sesión estructurada y portable.
3. **Privacidad de datos**: Las contraseñas se transmiten cifradas por canal HTTPS y no se exponen credenciales de base de datos en las respuestas públicas de la API.

---

## 5. Estrategias de Autenticación API Candidatas

### Opción A: JSON Web Token (JWT) Bearer Authentication (Recomendada)
Consiste en la emisión de un token firmado digitalmente por el servidor que almacena de manera segura la identidad del usuario y sus claims. El cliente almacena este token en memoria o almacenamiento local y lo adjunta en la cabecera `Authorization: Bearer <Token>` en cada petición.

* **Ventajas**:
  * Altamente portable y agnóstico al lenguaje (excelente compatibilidad con el futuro cliente móvil React Native / Expo).
  * Desacoplamiento total de cookies y sesiones de servidor.
  * Facilita el escalado horizontal del backend.
* **Desventajas/Riesgos**:
  * Sensible a ataques XSS (Cross-Site Scripting) si se almacena incorrectamente en el almacenamiento del navegador (`localStorage`).
  * Requiere lógica adicional en el cliente para gestionar la renovación del token (refresh tokens).

### Opción B: Cookies HTTP-Only SameSite/Secure (Híbrido)
Reutilizar la infraestructura de autenticación por cookies de ASP.NET Core existente, exponiendo un endpoint de login asíncrono que emita la cookie de sesión cifrada directamente al navegador.

* **Ventajas**:
  * Inmunidad natural frente a ataques XSS si se configura la cabecera `HttpOnly`.
  * Integración simple con los flujos de autorización y cookies de Razor existentes.
* **Desventajas/Riesgos**:
  * Poca o nula compatibilidad nativa con clientes móviles (los cuales no gestionan cookies de forma automática del mismo modo que un navegador).
  * Exposición a ataques CSRF (Cross-Site Request Forgery), requiriendo validación rigurosa de Anti-Forgery Tokens.

---

## 6. Recomendación y Decisión de Diseño

Para EcoInstalacion se recomienda un enfoque híbrido transicional de **JWT Bearer Token** como la estrategia de autenticación predilecta para la API REST.

### Arquitectura de Tokens Propuesta
1. **Emisión de Access Token**: JWT de corta duración (ej. 15 minutos) que viaja en memoria en la aplicación React.
2. **Emisión de Refresh Token**: Almacenado de forma segura en una cookie en el navegador con directivas `HttpOnly`, `Secure` y `SameSite` según el modelo de despliegue:
   - `SameSite=Lax` o `SameSite=Strict` si frontend y backend comparten el mismo sitio.
   - `SameSite=None; Secure` si React y la API operan como orígenes cruzados y se usan cookies con credenciales.
3. **Dispositivos Móviles**: Para el cliente móvil, tanto el Access Token como el Refresh Token pueden almacenarse de manera cifrada mediante el almacenamiento seguro en el dispositivo mediante mecanismos como Expo SecureStore, Keychain o Keystore.

---

## 7. Roles Previstos y Modelo de Autorización

El sistema mantendrá el mapeo de roles existente en la base de datos:

* **Administrador (`Administrador`)**: Acceso total al panel web administrativo. Permiso de lectura y escritura en clientes, técnicos, roles, solicitudes y visitas técnicas.
* **Técnico (`Técnico`)**: Acceso restringido (principalmente desde la app móvil). Permiso de lectura en solicitudes asignadas y lectura/escritura en visitas técnicas.

---

## 8. Matriz Inicial de Protección de Endpoints

Una vez implementada la seguridad, los endpoints responderán según la siguiente matriz de políticas:

| Módulo | Endpoint | Método | Requerimiento de Roles |
| :--- | :--- | :--- | :--- |
| **Auth** | `/api/auth/login` | POST | Anónimo / Público |
| | `/api/auth/refresh` | POST | Anónimo / Envía Refresh Token |
| **Clientes**| `/api/clientes` | GET | `Administrador` |
| | `/api/clientes/{id}` | GET | `Administrador` |
| | `/api/clientes` | POST/PUT/DELETE | `Administrador` |
| **Técnicos**| `/api/tecnicos` | GET | `Administrador` |
| | `/api/tecnicos/{id}` | GET | `Administrador` |
| | `/api/tecnicos` | POST/PUT/DELETE | `Administrador` |
| **Solicitudes**| `/api/solicitudes` | GET | `Administrador`, `Técnico` *(filtrado por asignación)* |
| | `/api/solicitudes/{id}` | GET | `Administrador`, `Técnico` *(filtrado por asignación)* |
| | `/api/solicitudes` | POST/PUT/DELETE | `Administrador` |
| **Visitas** | `/api/visitas-tecnicas` | GET | `Administrador` |
| | `/api/visitas-tecnicas?solicitudId={id}` | GET | `Administrador`, `Técnico` |
| | `/api/visitas-tecnicas` | POST/PUT/DELETE | `Administrador`, `Técnico` |

---

## 9. Flujo Futuro de Login y Orquestación

```
React Client                 Kestrel Server (ASP.NET Core)
    |                                   |
    |---- 1. POST /api/auth/login ----->|  (Valida credenciales en BLL/BD)
    |      (correo, clave)              |
    |                                   |
    |<--- 2. Retorna JWT (body) --------|  (Genera y firma el JWT)
    |     y Refresh Token (Cookie)      |
    |                                   |
    |---- 3. Petición con Cabecera ---->|  (Valida firma de JWT y Claims)
    |     Authorization: Bearer <Token> |
    |                                   |
```

### Orquestación en el Cliente (React)
* El Access Token se almacena en memoria de la aplicación (dentro de una variable del estado de React).
* El componente de enrutamiento del frontend evalúa la presencia del token y los roles para permitir el acceso a las vistas.
* Si el Access Token expira, el cliente HTTP base realiza automáticamente una llamada pre-intercepción a `/api/auth/refresh` para renovar la sesión en segundo plano.

---

## 10. Consideraciones CORS y Seguridad Adicional

* **SameSite y Secure**: Las cookies emitidas por el backend destinadas a refresh tokens deben configurarse con las directivas `HttpOnly`, `Secure` y `SameSite` según el modelo de despliegue:
  - `SameSite=Lax` o `SameSite=Strict` si frontend y backend comparten el mismo sitio.
  - `SameSite=None; Secure` si React y la API operan como orígenes cruzados y se usan cookies con credenciales.
* **AllowCredentials**: La política CORS actual `FrontendDevelopment` en `Program.cs` no permite el uso de credenciales (`.AllowCredentials()`). Si se implementa refresh token basado en cookies, la política CORS del backend debe ser actualizada de forma controlada a `.AllowCredentials()` especificando explícitamente los orígenes permitidos.

---

## 11. Roadmap de Implementación de Seguridad API

* **Sprint 5F: Diseño e implementación mínima de Auth API**
  * Creación del endpoint `/api/auth/login` y generación de tokens JWT.
  * Habilitación del servicio de validación JWT Bearer en el arranque de ASP.NET Core.
* **Sprint 5G: Protección de rutas en React**
  * Creación de contextos de seguridad (`AuthContext`) en el frontend.
  * Componentes de rutas protegidas y manejo de estados de sesión.
* **Sprint 5H: Navegación de Entidades**
  * Pantalla de login e integración del flujo completo en la interfaz.
* **Sprint 5I: Operaciones de escritura protegidas**
  * Aseguramiento del paso de cabeceras en llamadas REST de escritura.
