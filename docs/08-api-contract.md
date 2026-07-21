# Contrato API REST

Este documento describe el contrato HTTP vigente de la API de **EcoInstalacion / Electrical Solar Service Management**, consumida actualmente por el frontend React + Vite y preparada para futuros clientes móviles.

## 1. Base URL local

```text
http://localhost:5240
```

## 2. Convenciones generales

- Las solicitudes y respuestas utilizan `application/json`.
- Las propiedades JSON se serializan en `camelCase`.
- La API publica DTOs específicos y no expone directamente las entidades de persistencia.
- Salvo `POST /api/auth/login`, los endpoints requieren un access token JWT válido.
- El token se envía mediante `Authorization: Bearer <token>`.
- Los controladores API utilizan `[IgnoreAntiforgeryToken]` porque la autenticación REST se basa en JWT Bearer y no en la cookie MVC.

## 3. Políticas de acceso

| Política | Esquema | Roles admitidos |
| :--- | :--- | :--- |
| `ApiAuthenticated` | JWT Bearer | `Administrador`, `Técnico` |
| `ApiAdminOnly` | JWT Bearer | `Administrador` |

Una solicitud sin token válido obtiene `401 Unauthorized`. Un usuario autenticado cuyo rol no cumple la política obtiene `403 Forbidden`.

## 4. Autenticación

### Login

| Método | Endpoint | Acceso | Entrada | Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/login` | Anónimo | `AuthLoginRequestDto` | `AuthLoginResponseDto` |

Payload de entrada:

```json
{
  "correo": "usuario@ejemplo.com",
  "clave": "contraseña"
}
```

Respuesta exitosa:

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

El login retorna `401` para credenciales incorrectas y `403` si el usuario, su rol o el rol asignado no están habilitados para la aplicación.

## 5. Endpoints por módulo

### 5.1 Clientes (`/api/clientes`)

| Método | Endpoint | Descripción | Acceso |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/clientes` | Lista clientes activos. | Administrador, Técnico |
| **GET** | `/api/clientes/administracion?estado=activos\|inactivos\|todos` | Lista clientes según estado. | Administrador |
| **GET** | `/api/clientes/{id}` | Obtiene el detalle. Un Técnico recibe `404` ante un cliente inactivo. | Administrador, Técnico |
| **POST** | `/api/clientes` | Crea un cliente activo. | Administrador, Técnico |
| **PUT** | `/api/clientes/{id}` | Actualiza un cliente. | Administrador |
| **DELETE** | `/api/clientes/{id}` | Elimina o desactiva según las reglas de negocio. | Administrador |

DTOs principales: `ClienteListDto`, `ClienteDetailDto`, `ClienteCreateDto` y `ClienteUpdateDto`.

### 5.2 Técnicos (`/api/tecnicos`)

| Método | Endpoint | Descripción | Acceso |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/tecnicos` | Lista técnicos. | Administrador, Técnico |
| **GET** | `/api/tecnicos/{id}` | Obtiene el detalle. | Administrador, Técnico |
| **POST** | `/api/tecnicos` | Crea un técnico. | Administrador |
| **PUT** | `/api/tecnicos/{id}` | Actualiza un técnico. | Administrador |
| **DELETE** | `/api/tecnicos/{id}` | Elimina un técnico si las reglas de negocio lo permiten. | Administrador |

DTOs principales: `TecnicoListDto`, `TecnicoDetailDto`, `TecnicoCreateDto` y `TecnicoUpdateDto`.

### 5.3 Roles (`/api/roles`)

Todo el módulo requiere `ApiAdminOnly`.

| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| **GET** | `/api/roles?filtro={texto}&soloActivos={bool}` | Lista roles con filtros opcionales. |
| **GET** | `/api/roles/{id}` | Obtiene el detalle de un rol. |
| **POST** | `/api/roles` | Crea un rol conforme al catálogo permitido. |
| **PUT** | `/api/roles/{id}` | Actualiza descripción y estado. |
| **PATCH** | `/api/roles/{id}/estado?esActivo={bool}` | Activa o desactiva un rol. |
| **DELETE** | `/api/roles/{id}` | Elimina un rol si las reglas de negocio lo permiten. |

DTOs principales: `RolListDto`, `RolDetailDto`, `RolCreateDto` y `RolUpdateDto`.

### 5.4 Solicitudes (`/api/solicitudes`)

| Método | Endpoint | Descripción | Acceso |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/solicitudes` | Lista solicitudes. | Administrador, Técnico |
| **GET** | `/api/solicitudes/{id}` | Obtiene el detalle. | Administrador, Técnico |
| **POST** | `/api/solicitudes` | Crea una solicitud. | Administrador |
| **PUT** | `/api/solicitudes/{id}` | Actualiza una solicitud. | Administrador |
| **DELETE** | `/api/solicitudes/{id}` | Elimina una solicitud si las reglas de negocio lo permiten. | Administrador |

DTOs principales: `SolicitudListDto`, `SolicitudDetailDto`, `SolicitudCreateDto` y `SolicitudUpdateDto`.

### 5.5 Visitas Técnicas (`/api/visitas-tecnicas`)

| Método | Endpoint | Descripción | Acceso |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/visitas-tecnicas` | Lista visitas técnicas. | Administrador, Técnico |
| **GET** | `/api/visitas-tecnicas?solicitudId={id}` | Filtra visitas por solicitud. | Administrador, Técnico |
| **GET** | `/api/visitas-tecnicas/{id}` | Obtiene el detalle. | Administrador, Técnico |
| **POST** | `/api/visitas-tecnicas` | Crea una visita. | Administrador |
| **PUT** | `/api/visitas-tecnicas/{id}` | Actualiza una visita. | Administrador |
| **DELETE** | `/api/visitas-tecnicas/{id}` | Elimina una visita si las reglas de negocio lo permiten. | Administrador |

DTOs principales: `VisitaTecnicaListDto`, `VisitaTecnicaDetailDto`, `VisitaTecnicaCreateDto` y `VisitaTecnicaUpdateDto`.

## 6. Códigos de respuesta

| Código | Uso |
| :--- | :--- |
| `200 OK` | Consulta o actualización exitosa. |
| `201 Created` | Creación exitosa, con `Location` y DTO de detalle. |
| `204 No Content` | Eliminación exitosa. |
| `400 Bad Request` | Validación, argumentos o reglas de negocio incumplidas. |
| `401 Unauthorized` | Token ausente, inválido o expirado; también credenciales de login incorrectas. |
| `403 Forbidden` | Usuario autenticado sin el rol requerido o cuenta/rol no habilitado durante el login. |
| `404 Not Found` | Recurso inexistente o no visible para el rol autenticado. |
| `500 Internal Server Error` | Fallo inesperado del servidor. |

## 7. Seguridad y convivencia con MVC

- Las vistas MVC/Razor conservan autenticación por cookie.
- Las políticas API seleccionan explícitamente JWT Bearer, por lo que no aceptan la cookie MVC como sustituto del token.
- Solo el endpoint de login permite acceso anónimo.
- Las credenciales y entidades de usuario no se exponen en los módulos operativos.
- El secreto JWT debe configurarse mediante user-secrets o variables de entorno y no debe persistirse en archivos públicos.

## 8. Compatibilidad de clientes

El contrato utiliza JSON, DTOs y semántica HTTP independiente de la plataforma. El frontend React + Vite ya consume estos endpoints; un cliente React Native / Expo podrá reutilizarlos incorporando el mismo flujo Bearer y un mecanismo de almacenamiento seguro apropiado para el dispositivo.
