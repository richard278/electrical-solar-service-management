# Diseño de Autenticación y Autorización API

Este documento describe la seguridad actualmente implementada para la API REST de **EcoInstalacion** y las medidas que continúan pendientes antes de un despliegue productivo.

## 1. Propósito

Mantener una referencia verificable del login, la emisión y validación del JWT, las políticas por rol y el manejo de sesión del frontend React, preservando la convivencia con la autenticación MVC/Razor.

## 2. Arquitectura vigente

| Superficie | Esquema | Estado |
| :--- | :--- | :--- |
| MVC/Razor | Cookie de ASP.NET Core | Activo para las vistas heredadas |
| API REST | JWT Bearer | Activo en Clientes, Técnicos, Roles, Solicitudes y Visitas Técnicas |
| React + Vite | Access token en memoria | Activo; el token se adjunta mediante el cliente HTTP centralizado |

El esquema predeterminado de la aplicación continúa siendo la cookie MVC. Las políticas de la API declaran explícitamente `JwtBearerDefaults.AuthenticationScheme`, evitando que una cookie MVC autorice por accidente una solicitud REST.

Los únicos roles activos para el acceso API son:

- `Administrador`.
- `Técnico`.

## 3. Login API implementado

### Endpoint

```http
POST /api/auth/login
Content-Type: application/json
```

Solicitud:

```json
{
  "correo": "usuario@ejemplo.com",
  "clave": "contraseña"
}
```

Respuesta `200 OK`:

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-07-21T20:00:00Z",
  "user": {
    "id": 1,
    "nombre": "Usuario",
    "rol": "Administrador"
  }
}
```

### Validaciones del servidor

El endpoint permite acceso anónimo únicamente para recibir credenciales y:

1. Valida el modelo de entrada.
2. Delega la comprobación de correo y clave al servicio de usuarios.
3. Rechaza credenciales incorrectas con `401 Unauthorized`.
4. Rechaza con `403 Forbidden` un usuario inactivo, un rol inactivo o una asignación ajena al catálogo canónico.
5. Emite el JWT y `expiresAtUtc` cuando el usuario está habilitado.

No existe un endpoint de refresh token en el estado actual.

## 4. Flujo de sesión en React

1. `AuthContext` envía las credenciales a `/api/auth/login`.
2. El access token se conserva en memoria mediante estado y una referencia de React.
3. `httpClient` obtiene el token e incorpora `Authorization: Bearer <token>` a las solicitudes.
4. `AppContent` muestra `LoginPage` si no existe una sesión autenticada y `AppShell` cuando la sesión está activa.
5. Un temporizador compara `expiresAtUtc` con la hora actual y cierra la sesión al vencer el token.
6. El usuario puede cerrar la sesión manualmente desde el panel.

Este diseño evita persistir el token en `localStorage` o `sessionStorage`. Como consecuencia deliberada, recargar la página elimina la sesión y exige un nuevo login.

## 5. Políticas de autorización

| Política | Requisito |
| :--- | :--- |
| `ApiAuthenticated` | JWT válido y rol `Administrador` o `Técnico` |
| `ApiAdminOnly` | JWT válido y rol `Administrador` |

La protección se aplica en el servidor. Las utilidades de roles del frontend reflejan la misma matriz para mostrar solo las acciones permitidas, pero no sustituyen la autorización de la API.

## 6. Matriz vigente de endpoints

| Módulo | Operación | Administrador | Técnico |
| :--- | :--- | :---: | :---: |
| Auth | `POST /api/auth/login` | Anónimo | Anónimo |
| Clientes | `GET /api/clientes` | Sí | Sí |
| Clientes | `GET /api/clientes/administracion` | Sí | No |
| Clientes | `GET /api/clientes/{id}` | Sí | Sí, solo cliente activo |
| Clientes | `POST /api/clientes` | Sí | Sí |
| Clientes | `PUT`, `DELETE /api/clientes/{id}` | Sí | No |
| Técnicos | `GET` listado y detalle | Sí | Sí |
| Técnicos | `POST`, `PUT`, `DELETE` | Sí | No |
| Roles | Todas las operaciones | Sí | No |
| Solicitudes | `GET` listado y detalle | Sí | Sí |
| Solicitudes | `POST`, `PUT`, `DELETE` | Sí | No |
| Visitas Técnicas | `GET` listado, filtro y detalle | Sí | Sí |
| Visitas Técnicas | `POST`, `PUT`, `DELETE` | Sí | No |

La excepción de negocio vigente permite que el rol `Técnico` cree clientes, pero no los edite ni elimine. En los demás módulos operativos sus permisos actuales son de consulta.

La autorización presente trabaja a nivel de endpoint. Salvo la ocultación de clientes inactivos para el rol Técnico, todavía no existe filtrado de registros por técnico asignado.

## 7. Validación y configuración del JWT

La configuración de JWT valida:

- Emisor.
- Audiencia.
- Tiempo de vida.
- Firma y clave de firma.

La clave debe tener al menos 32 caracteres y se obtiene de `Jwt__SecretKey` o de user-secrets. Los archivos públicos solo deben contener marcadores seguros.

`RequireHttpsMetadata` permanece deshabilitado para el entorno local y la redirección HTTPS no está activa en la configuración actual. Esto es aceptable únicamente para desarrollo local controlado; un despliegue externo debe habilitar HTTPS de extremo a extremo.

## 8. CORS y antiforgery

La política CORS local admite explícitamente:

- `http://localhost:5173`.
- `https://localhost:5173`.

Permite encabezados y métodos HTTP, pero no habilita credenciales de cookies entre orígenes. Esto es coherente con el access token enviado en el encabezado Bearer.

Los controladores API usan `[IgnoreAntiforgeryToken]`. Esta decisión no vuelve públicos los endpoints: la protección efectiva corresponde a JWT y a las políticas de autorización. Los formularios MVC continúan bajo el esquema antiforgery y la autenticación por cookie.

## 9. Respuestas de seguridad esperadas

| Código | Significado |
| :--- | :--- |
| `400 Bad Request` | Payload inválido o regla de negocio incumplida. |
| `401 Unauthorized` | Credenciales incorrectas en login o token ausente, inválido o expirado. |
| `403 Forbidden` | Cuenta o rol inactivo durante el login, o rol sin permiso para el endpoint. |
| `404 Not Found` | Recurso inexistente o cliente inactivo no visible para un Técnico. |

## 10. Estado de implementación

| Capacidad | Estado |
| :--- | :--- |
| Login API y emisión JWT | Implementado |
| Validación JWT Bearer | Implementado |
| Políticas `ApiAuthenticated` y `ApiAdminOnly` | Implementado |
| Contexto de autenticación React | Implementado |
| Token en memoria y cabecera Bearer | Implementado |
| Controles de UI por rol | Implementado |
| Operaciones protegidas de Clientes, Técnicos, Solicitudes y Visitas | Implementado |
| Refresh token y renovación silenciosa | Pendiente |
| Revocación de tokens y cierre global de sesiones | Pendiente |
| HTTPS productivo | Pendiente |
| Filtrado de datos por técnico asignado | Pendiente |
| Pruebas automáticas de autenticación y autorización | Pendiente |

## 11. Criterios antes de producción

Antes de publicar la API fuera de un entorno local se debe:

1. Servir frontend y backend exclusivamente mediante HTTPS.
2. Diseñar refresh tokens con rotación, expiración, revocación y almacenamiento seguro.
3. Centralizar el tratamiento de sesiones expiradas y respuestas `401` en el cliente.
4. Incorporar pruebas automáticas para credenciales, roles, expiración y matriz de permisos.
5. Definir el alcance de datos visible para cada Técnico.
6. Revisar CORS, secretos, telemetría y manejo global de errores según el entorno de despliegue.
