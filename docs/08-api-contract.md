# Contrato API REST

Este documento describe el contrato HTTP vigente de la API de **EcoInstalacion / Electrical Solar Service Management** en el baseline público (`a1cc4ef95dc1a91198c1ed2db571e9dfbec4be38`), consumida actualmente por la SPA administrativa React + Vite (`EcoInstalacion.Frontend`) y preparada como base para futuros clientes móviles.

## 1. Base URL local

```text
http://localhost:5240
```

## 2. Convenciones generales y paginación

- Las solicitudes y respuestas utilizan `application/json`.
- Las propiedades JSON se serializan en `camelCase`.
- La API publica DTOs específicos y no expone directamente las entidades de persistencia (`EcoInstalacion.Entity`).
- Salvo `POST /api/auth/login`, todos los endpoints requieren un access token JWT válido enviado mediante `Authorization: Bearer <token>`.
- Los controladores API utilizan `[IgnoreAntiforgeryToken]` porque la autenticación REST se basa exclusivamente en JWT Bearer y no en la cookie de sesión MVC.

### Estructura estándar de paginación (`PagedResponse<T>`)

Los endpoints paginados (`/paginados`, `/paginadas` y `/api/clientes/administracion`) devuelven el contenedor genérico [`PagedResponse<T>`](../EcoInstalacion.AppWeb/DTOs/Shared/PagedResponse.cs):

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalCount": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

**Reglas de normalización en servidor (`DAL`):**
- `page`: valor predeterminado `1`; si se recibe un valor `< 1`, se normaliza a `1`.
- `pageSize`: valor predeterminado `25`; si se recibe `< 1` se normaliza a `25`, y si supera `100` se acota a `100`.
- `q`: término opcional de búsqueda libre (se aplica `.Trim()` e ignora cadenas vacías o de solo espacios).

## 3. Políticas de acceso

| Política | Esquema | Roles admitidos (`RolesSistema`) |
| :--- | :--- | :--- |
| `ApiAuthenticated` | JWT Bearer | `Administrador` (`IdRol = 1`), `Técnico` (`IdRol = 2`) |
| `ApiAdminOnly` | JWT Bearer | `Administrador` (`IdRol = 1`) |

Una solicitud sin token válido obtiene `401 Unauthorized`. Un usuario autenticado cuyo rol no cumple la política del endpoint obtiene `403 Forbidden`.

## 4. Autenticación (`/api/auth`)

### Login

| Método | Endpoint | Política | Entrada | Respuesta (`200 OK`) |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/login` | `AllowAnonymous` | `AuthLoginRequestDto` | `AuthLoginResponseDto` |

Payload de entrada (`AuthLoginRequestDto`):

```json
{
  "correo": "usuario@ejemplo.com",
  "clave": "contraseña"
}
```

Respuesta exitosa (`AuthLoginResponseDto`):

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

- Retorna `400 Bad Request` si el modelo de entrada no cumple las anotaciones de validación (`correo` con formato de email válido y `clave` obligatoria).
- Retorna `401 Unauthorized` (`{ "mensaje": "..." }`) ante credenciales incorrectas.
- Retorna `403 Forbidden` (`{ "mensaje": "..." }`) si el usuario está inactivo, su rol está inactivo o la asignación no pertenece al catálogo canónico (`Administrador`, `Técnico`).
- No existe endpoint de refresh token ni revocación en el baseline actual.

## 5. Endpoints por módulo

### 5.1 Clientes (`/api/clientes`)

Controlador: [`ClientesApiController`](../EcoInstalacion.AppWeb/Controllers/Api/ClientesApiController.cs) (política base de controlador: `ApiAuthenticated`).

| Método | Endpoint | Descripción y parámetros | Respuesta exitosa | Acceso |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/clientes` | Lista completa de clientes activos ordenados por `NombreCompleto`. | `200 OK` (`List<ClienteListDto>`) | Administrador, Técnico |
| **GET** | `/api/clientes/paginados` | Listado paginado con búsqueda por `NombreCompleto` o `DUI` (`?q={texto}&estado={activos\|inactivos\|todos}&page=1&pageSize=25`). Para `Administrador`, filtra según `estado` (default `activos`; valor inválido retorna `400`). Para `Técnico`, ignora `estado` y fuerza `esActivo = true`. | `200 OK` (`PagedResponse<ClienteListDto>`) | Administrador, Técnico |
| **GET** | `/api/clientes/administracion` | Listado administrativo paginado con filtro de estado y búsqueda (`?q={texto}&estado=activos\|inactivos\|todos&page=1&pageSize=25`). Retorna `400` si `estado` no es `activos`, `inactivos` o `todos`. | `200 OK` (`PagedResponse<ClienteListDto>`) | Administrador (`ApiAdminOnly`) |
| **GET** | `/api/clientes/{id:int}` | Obtiene el detalle de un cliente. Un `Técnico` recibe `404 Not Found` si el cliente está inactivo (`!EsActivo`). | `200 OK` (`ClienteDetailDto`) | Administrador, Técnico |
| **POST** | `/api/clientes` | Crea un cliente siempre con `EsActivo = true`. | `201 Created` (`ClienteDetailDto`) | Administrador, Técnico |
| **PUT** | `/api/clientes/{id:int}` | Actualiza datos y estado (`EsActivo`) de un cliente. | `200 OK` (`ClienteDetailDto`) | Administrador (`ApiAdminOnly`) |
| **DELETE** | `/api/clientes/{id:int}` | Elimina físicamente un cliente si no tiene solicitudes asociadas. | `204 No Content` | Administrador (`ApiAdminOnly`) |

**DTOs y reglas de validación (`Clientes`):**
- `ClienteListDto` / `ClienteDetailDto`: `id`, `nombreCompleto`, `dui`, `telefono`, `direccion`, `correo`, `esActivo`.
- `ClienteCreateDto`: `nombreCompleto` (requerido, máx. 100), `dui` (requerido, máx. 10), `telefono` (opcional, máx. 15), `direccion` (opcional, máx. 150), `correo` (opcional, formato email, máx. 100).
- `ClienteUpdateDto`: mismos campos que `ClienteCreateDto` más `esActivo` (`bool`, por defecto `true`).
- **Reglas de negocio en `BLL` (`400 Bad Request`)**: unicidad de `DUI` y de `Correo` en creación y edición; bloqueo de eliminación (`DELETE`) cuando el cliente registra solicitudes de instalación (`TieneSolicitudes`).

---

### 5.2 Técnicos (`/api/tecnicos`)

Controlador: [`TecnicosApiController`](../EcoInstalacion.AppWeb/Controllers/Api/TecnicosApiController.cs) (política base de controlador: `ApiAuthenticated`).

| Método | Endpoint | Descripción y parámetros | Respuesta exitosa | Acceso |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/tecnicos` | Lista completa de técnicos ordenados por `Nombre`. | `200 OK` (`List<TecnicoListDto>`) | Administrador, Técnico |
| **GET** | `/api/tecnicos/paginados` | Listado paginado con búsqueda por `Nombre` o `Especialidad` (`?q={texto}&page=1&pageSize=25`), ordenado por `Nombre` e `Id`. | `200 OK` (`PagedResponse<TecnicoListDto>`) | Administrador, Técnico |
| **GET** | `/api/tecnicos/{id:int}` | Obtiene el detalle de un técnico por identificador. | `200 OK` (`TecnicoDetailDto`) | Administrador, Técnico |
| **POST** | `/api/tecnicos` | Crea un registro de técnico. | `201 Created` (`TecnicoDetailDto`) | Administrador (`ApiAdminOnly`) |
| **PUT** | `/api/tecnicos/{id:int}` | Actualiza los datos de un técnico existente. | `200 OK` (`TecnicoDetailDto`) | Administrador (`ApiAdminOnly`) |
| **DELETE** | `/api/tecnicos/{id:int}` | Elimina un técnico si no está asignado a solicitudes. | `204 No Content` | Administrador (`ApiAdminOnly`) |

**DTOs y reglas de validación (`Técnicos`):**
- `TecnicoListDto` / `TecnicoDetailDto`: `id`, `nombre`, `especialidad`, `telefono`, `correo`.
- `TecnicoCreateDto` / `TecnicoUpdateDto`: `nombre` (requerido, máx. 100), `especialidad` (opcional, máx. 50), `telefono` (opcional, máx. 15), `correo` (opcional, formato email, máx. 100).
- **Reglas de negocio en `BLL` (`400 Bad Request`)**: unicidad de `Correo` (si se proporciona); bloqueo de eliminación (`DELETE`) si el técnico está asignado a alguna solicitud (`EstaAsignadoASolicitudes`).

---

### 5.3 Roles (`/api/roles`)

Controlador: [`RolesApiController`](../EcoInstalacion.AppWeb/Controllers/Api/RolesApiController.cs). Todo el controlador exige la política `ApiAdminOnly` (`Administrador`).

| Método | Endpoint | Descripción y parámetros | Respuesta exitosa |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/roles?filtro={texto}&soloActivos={bool}` | Lista roles con filtros opcionales de texto y estado activo. | `200 OK` (`List<RolListDto>`) |
| **GET** | `/api/roles/{id:int}` | Obtiene el detalle de un rol. | `200 OK` (`RolDetailDto`) |
| **POST** | `/api/roles` | Crea un rol conforme al catálogo canónico permitido. | `201 Created` (`RolDetailDto`) |
| **PUT** | `/api/roles/{id:int}` | Actualiza descripción y estado de un rol. | `200 OK` (`RolDetailDto`) |
| **PATCH** | `/api/roles/{id:int}/estado?esActivo={bool}` | Activa o desactiva un rol. | `200 OK` (`RolDetailDto`) |
| **DELETE** | `/api/roles/{id:int}` | Elimina un rol si las reglas de negocio lo permiten. | `204 No Content` |

DTOs principales: `RolListDto` (`idRol`, `descripcion`, `esActivo`), `RolDetailDto` (añade `fechaRegistro`), `RolCreateDto` y `RolUpdateDto`.

---

### 5.4 Solicitudes de Instalación (`/api/solicitudes`)

Controlador: [`SolicitudesApiController`](../EcoInstalacion.AppWeb/Controllers/Api/SolicitudesApiController.cs) (política base de controlador: `ApiAuthenticated`).

| Método | Endpoint | Descripción y parámetros | Respuesta exitosa | Acceso |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/solicitudes` | Lista completa de solicitudes con cliente y técnico asignado, ordenadas por `FechaSolicitud` descendente e `Id` descendente. | `200 OK` (`List<SolicitudListDto>`) | Administrador, Técnico |
| **GET** | `/api/solicitudes/paginadas` | Listado paginado con búsqueda por nombre de cliente (`Cliente.NombreCompleto`) y filtro por estado (`?q={texto}&estado={estado}&page=1&pageSize=25`). Si `estado` se omite o es `"Todas"`, no filtra por estado. | `200 OK` (`PagedResponse<SolicitudListDto>`) | Administrador, Técnico |
| **GET** | `/api/solicitudes/{id:int}` | Obtiene el detalle de una solicitud. | `200 OK` (`SolicitudDetailDto`) | Administrador, Técnico |
| **POST** | `/api/solicitudes` | Crea una solicitud (nace siempre en estado `"Pendiente"` con `FechaSolicitud = DateTime.UtcNow`). | `201 Created` (`SolicitudDetailDto`) | Administrador (`ApiAdminOnly`) |
| **PUT** | `/api/solicitudes/{id:int}` | Actualiza cliente, tipo, descripción, estado y técnico asignado. | `200 OK` (`SolicitudDetailDto`) | Administrador (`ApiAdminOnly`) |
| **DELETE** | `/api/solicitudes/{id:int}` | Elimina una solicitud si no tiene visitas técnicas asociadas. | `204 No Content` | Administrador (`ApiAdminOnly`) |

**DTOs y reglas de validación (`Solicitudes`):**
- `SolicitudListDto`: `id`, `fechaSolicitud`, `clienteId`, `clienteNombre`, `tipoInstalacion`, `estado`, `tecnicoAsignadoId`, `tecnicoNombre`.
- `SolicitudDetailDto`: mismos campos que `SolicitudListDto` más `descripcion`.
- `SolicitudCreateDto`: `clienteId` (`>= 1`), `tipoInstalacion` (requerido, 3 a 50 caracteres; valores de dominio: `"Solar"` o `"Convencional"`), `descripcion` (opcional, máx. 500 caracteres), `tecnicoAsignadoId` (opcional, `>= 1`).
- `SolicitudUpdateDto`: mismos campos que `SolicitudCreateDto` más `estado` (requerido, máx. 20 caracteres; estados canónicos en [`EstadosSolicitudSistema`](../EcoInstalacion.Entity/EstadosSolicitudSistema.cs): `"Pendiente"`, `"Programada"`, `"En proceso"`, `"Cerrada"`, `"Rechazada"`).
- **Reglas de negocio en `BLL` (`400 Bad Request`)**:
  - El cliente debe existir; para crear una solicitud o reasignarla a otro cliente, el cliente destino debe estar activo (`EsActivo == true`).
  - Si se indica `tecnicoAsignadoId`, el técnico debe existir.
  - No se permite más de una solicitud abierta (estado distinto de `"Cerrada"` y `"Rechazada"`) con la misma descripción para el mismo cliente.
  - No se puede eliminar (`DELETE`) una solicitud que tenga visitas técnicas registradas (`VisitasTecnicas.Any()`).

---

### 5.5 Visitas Técnicas (`/api/visitas-tecnicas`)

Controlador: [`VisitasTecnicasApiController`](../EcoInstalacion.AppWeb/Controllers/Api/VisitasTecnicasApiController.cs) (política base de controlador: `ApiAuthenticated`).

| Método | Endpoint | Descripción y parámetros | Respuesta exitosa | Acceso |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/visitas-tecnicas` | Lista visitas técnicas ordenadas por `FechaVisita` descendente. Acepta filtro opcional `?solicitudId={id}` (si `solicitudId <= 0`, retorna `400 Bad Request`). | `200 OK` (`List<VisitaTecnicaListDto>`) | Administrador, Técnico |
| **GET** | `/api/visitas-tecnicas/paginadas` | Listado paginado con búsqueda por nombre de cliente (`Solicitud.Cliente.NombreCompleto`) y filtro booleano de aprobación (`?q={texto}&aprobado={true\|false}&page=1&pageSize=25`), ordenado por `FechaVisita` descendente e `Id` descendente. | `200 OK` (`PagedResponse<VisitaTecnicaListDto>`) | Administrador, Técnico |
| **GET** | `/api/visitas-tecnicas/{id:int}` | Obtiene el detalle de una visita técnica. | `200 OK` (`VisitaTecnicaDetailDto`) | Administrador, Técnico |
| **POST** | `/api/visitas-tecnicas` | Registra una visita técnica asociada a una solicitud activa. | `201 Created` (`VisitaTecnicaDetailDto`) | Administrador (`ApiAdminOnly`) |
| **PUT** | `/api/visitas-tecnicas/{id:int}` | Actualiza fecha, observaciones y estado de aprobación (`Aprobado`) de la visita. | `200 OK` (`VisitaTecnicaDetailDto`) | Administrador (`ApiAdminOnly`) |
| **DELETE** | `/api/visitas-tecnicas/{id:int}` | Elimina una visita técnica siempre que esté pendiente (`Aprobado != true`). | `204 No Content` | Administrador (`ApiAdminOnly`) |

**DTOs y reglas de validación (`Visitas Técnicas`):**
- `VisitaTecnicaListDto` / `VisitaTecnicaDetailDto`: `id`, `solicitudId`, `fechaVisita`, `observaciones`, `aprobado`, `clienteNombre`, `tipoInstalacion`, `estadoSolicitud`, `tecnicoNombre`.
- `VisitaTecnicaCreateDto`: `solicitudId` (`>= 1`), `fechaVisita` (requerido, `>= 1753-01-01`), `observaciones` (opcional, máx. 500 caracteres), `aprobado` (`bool` requerido).
- `VisitaTecnicaUpdateDto`: `fechaVisita` (requerido, `>= 1753-01-01`), `observaciones` (opcional, máx. 500 caracteres), `aprobado` (`bool` requerido). No permite modificar `solicitudId`.
- **Reglas de negocio en `BLL` (`400 Bad Request`)**:
  - La solicitud asociada debe existir y no encontrarse en estado terminal (`"Cerrada"` o `"Rechazada"`) al registrar o aprobar una visita.
  - La aprobación es **irreversible**: una visita con `Aprobado = true` no puede volver a `false` ni puede ser eliminada (`DELETE`).
  - Al aprobar una visita pendiente en edición (`EditarConTransicionAprobacion`) o al insertarla aprobada (mediante el trigger `dbo.tr_ActualizarEstadoSolicitud` en SQL Server), la solicitud asociada pasa a `"En proceso"`.

## 6. Códigos de respuesta y formato de errores

| Código | Uso | Estructura de cuerpo habitual |
| :--- | :--- | :--- |
| `200 OK` | Consulta o actualización exitosa. | DTO de detalle, `List<TDto>` o `PagedResponse<TDto>`. |
| `201 Created` | Creación exitosa, con encabezado `Location`. | DTO de detalle del recurso creado. |
| `204 No Content` | Eliminación exitosa. | Sin cuerpo. |
| `400 Bad Request` | Error de validación de modelo (`DataAnnotations`) o incumplimiento de regla de negocio (`TaskCanceledException`, `InvalidOperationException`, `ArgumentException`). | `ValidationProblemDetails` en fallos de `ModelState` de módulos operativos, o `{ "message": "..." }` en excepciones de negocio. |
| `401 Unauthorized` | Token JWT ausente, inválido o expirado; o credenciales inválidas en `/api/auth/login`. | Sin cuerpo en desafío Bearer, o `{ "mensaje": "..." }` en `/api/auth/login`. |
| `403 Forbidden` | Usuario autenticado sin el rol exigido por la política (`ApiAdminOnly` / `ApiAuthenticated`), o cuenta/rol inactivo en login. | Sin cuerpo en rechazo de política, o `{ "mensaje": "..." }` en `/api/auth/login`. |
| `404 Not Found` | Recurso inexistente o cliente inactivo consultado por el rol `Técnico`. | `{ "message": "..." }`. |
| `500 Internal Server Error` | Fallo inesperado del servidor. | `{ "mensaje": "..." }` en login o respuesta estándar del host. |

> **Nota sobre uniformidad de errores (`DT-07`)**: La estandarización global de errores bajo un único middleware (`ProblemDetails` / JSON uniforme entre `/api/auth` y los controladores operativos) permanece registrada como pendiente en [11. Registro de Deuda Técnica](11-technical-debt-register.md).

## 7. Seguridad y convivencia con MVC/Razor

- Las vistas heredadas MVC/Razor (`EcoInstalacion.AppWeb/Controllers`) conservan autenticación por cookie (`/Home/Index`) y validación antiforgery (`RequestVerificationToken`).
- Las políticas de la API (`ApiAuthenticated` y `ApiAdminOnly`) exigen explícitamente `JwtBearerDefaults.AuthenticationScheme`, por lo que una cookie MVC nunca autoriza solicitudes hacia `/api/*`.
- Los módulos administrativos de gestión de cuentas internas (`Usuarios`) y exportación de reportes PDF (`ReportesController` con QuestPDF) operan actualmente en la superficie MVC/Razor y no exponen endpoints en la API REST.
- La política de clasificación de alertas operativas ([`OperationalAlertPolicy`](../EcoInstalacion.BLL/Implementacion/OperationalAlertPolicy.cs)) está implementada y probada en `EcoInstalacion.BLL`, pero aún no se expone mediante un endpoint REST dedicado ni se consume en el dashboard de React.

## 8. Compatibilidad con frontend SPA y preparación móvil

- **Consumo en `EcoInstalacion.Frontend` (React 19 + Vite 8 + TypeScript 6)**:
  - Las cuatro vistas de gestión (`ClientesPage`, `TecnicosPage`, `SolicitudesPage`, `VisitasTecnicasPage`) consumen los endpoints paginados (`/paginados` y `/paginadas`) con `pageSize = 25`, búsqueda con debounce de `300 ms` y filtros por estado o aprobación.
  - La vista de resumen (`OverviewPage`) y los selectores de formularios modales consumen los endpoints de listado completo (`GET /api/clientes`, `GET /api/tecnicos`, `GET /api/solicitudes`, `GET /api/visitas-tecnicas`).
- **Consideraciones para futuro cliente móvil (React Native / Expo)**:
  - Los endpoints de lectura y paginación ya aceptan al rol `Técnico` (`ApiAuthenticated`).
  - No obstante, en el baseline actual (`a1cc4ef`) las operaciones de escritura sobre visitas técnicas (`POST` y `PUT /api/visitas-tecnicas`) requieren `ApiAdminOnly` (`Administrador`), y los listados no filtran aún registros por `TecnicoAsignadoId`. Habilitar el registro o edición de visitas en campo por parte de un `Técnico` requerirá ajustar dichas políticas y el filtrado por técnico asignado en una iteración posterior (véase [09. Preparación para Cliente Móvil](09-mobile-readiness.md)).
