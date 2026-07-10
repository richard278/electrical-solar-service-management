# Contrato API REST

Este documento define el contrato público de la API REST para el proyecto **Electrical Solar Service Management / EcoInstalacion**. Las interfaces aquí expuestas sirven de base para la integración con clientes API-first y para la futura migración hacia un frontend moderno con **React + Vite**.

## 1. Propósito del documento

Establecer una referencia clara y estructurada de los endpoints disponibles en el backend de ASP.NET Core, detallando sus rutas, métodos HTTP, códigos de estado y contratos de datos (DTOs).

## 2. Base URL (Entorno Local)

* **URL base**: `http://localhost:5240`

## 3. Convenciones Generales

* **Formato de datos**: Todas las solicitudes y respuestas utilizan el formato `application/json`.
* **Casing**: Se utiliza `camelCase` para los nombres de propiedad en los payloads JSON de respuesta y solicitud (configuración predeterminada de serialización en ASP.NET Core).
* **Transferencia de Datos (DTOs)**: No se exponen entidades de persistencia (`Entity`) directamente como respuesta pública de la API. Se emplean objetos de transferencia de datos separados por cada operación (`ListDto`, `DetailDto`, `CreateDto`, `UpdateDto`).
* **Códigos de Estado HTTP**: Se utilizan de manera semántica para indicar el éxito o tipo de fallo de la operación.
* **Seguridad y Antiforgery**: Los controladores de la API REST emplean el atributo `[IgnoreAntiforgeryToken]` para facilitar el consumo externo y del futuro SPA, delegando la seguridad de sesión al esquema que corresponda.

---

## 4. Módulos Disponibles y Endpoints

### 4.1 Clientes (`api/clientes`)

Módulo encargado de la gestión de clientes residenciales y comerciales.

| Método | Endpoint | Descripción | Payload Entrada | Payload Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/clientes` | Obtiene la lista de todos los clientes. | Ninguno | `List<ClienteListDto>` |
| **GET** | `/api/clientes/{id}` | Obtiene el detalle de un cliente específico. | Ninguno | `ClienteDetailDto` (o `404`) |
| **POST** | `/api/clientes` | Registra un nuevo cliente. | `ClienteCreateDto` | `ClienteDetailDto` |
| **PUT** | `/api/clientes/{id}` | Actualiza un cliente existente. | `ClienteUpdateDto` | `ClienteDetailDto` |
| **DELETE** | `/api/clientes/{id}` | Desactiva o elimina un cliente. | Ninguno | `204 No Content` |

### 4.2 Técnicos (`api/tecnicos`)

Módulo para administrar al personal técnico de instalaciones y mantenimiento.

| Método | Endpoint | Descripción | Payload Entrada | Payload Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/tecnicos` | Obtiene el listado de técnicos. | Ninguno | `List<TecnicoListDto>` |
| **GET** | `/api/tecnicos/{id}` | Obtiene el detalle de un técnico. | Ninguno | `TecnicoDetailDto` (o `404`) |
| **POST** | `/api/tecnicos` | Registra un nuevo técnico. | `TecnicoCreateDto` | `TecnicoDetailDto` |
| **PUT** | `/api/tecnicos/{id}` | Actualiza un técnico existente. | `TecnicoUpdateDto` | `TecnicoDetailDto` |
| **DELETE** | `/api/tecnicos/{id}` | Elimina un técnico del sistema. | Ninguno | `204 No Content` |

### 4.3 Roles (`api/roles`)

Gestión de los roles y permisos del sistema.

| Método | Endpoint | Descripción | Payload Entrada | Payload Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/roles` | Obtiene la lista de roles registrados. | Ninguno | `List<RolListDto>` |
| **GET** | `/api/roles/{id}` | Obtiene el detalle de un rol específico. | Ninguno | `RolDetailDto` (o `404`) |
| **POST** | `/api/roles` | Registra un nuevo rol administrativo o técnico. | `RolCreateDto` | `RolDetailDto` |
| **PUT** | `/api/roles/{id}` | Edita la descripción u otros campos de un rol. | `RolUpdateDto` | `RolDetailDto` |
| **PATCH**| `/api/roles/{id}/estado` | Activa o desactiva un rol mediante query params. | Query: `?esActivo=true\|false` | `RolDetailDto` |
| **DELETE**| `/api/roles/{id}` | Elimina un rol del sistema. | Ninguno | `204 No Content` |

### 4.4 Solicitudes de Instalación (`api/solicitudes`)

Módulo para administrar las solicitudes de instalaciones eléctricas o solares.

| Método | Endpoint | Descripción | Payload Entrada | Payload Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/solicitudes` | Obtiene la lista general de solicitudes. | Ninguno | `List<SolicitudListDto>` |
| **GET** | `/api/solicitudes/{id}` | Obtiene el detalle completo de una solicitud. | Ninguno | `SolicitudDetailDto` (o `404`) |
| **POST** | `/api/solicitudes` | Registra una nueva solicitud de instalación. | `SolicitudCreateDto` | `SolicitudDetailDto` |
| **PUT** | `/api/solicitudes/{id}` | Edita campos y reasigna técnicos en solicitudes. | `SolicitudUpdateDto` | `SolicitudDetailDto` |
| **DELETE**| `/api/solicitudes/{id}` | Elimina una solicitud del sistema. | Ninguno | `204 No Content` |

### 4.5 Visitas Técnicas (`api/visitas-tecnicas`)

Módulo encargado del control y registro de visitas técnicas de campo para validación de instalaciones.

| Método | Endpoint | Descripción | Payload Entrada | Payload Respuesta |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/visitas-tecnicas` | Obtiene la lista general de visitas. | Ninguno | `List<VisitaTecnicaListDto>` |
| **GET** | `/api/visitas-tecnicas?solicitudId={id}` | Filtra visitas por ID de solicitud. | Query: `solicitudId` | `List<VisitaTecnicaListDto>` |
| **GET** | `/api/visitas-tecnicas/{id}` | Obtiene el detalle de una visita. | Ninguno | `VisitaTecnicaDetailDto` (o `404`) |
| **POST** | `/api/visitas-tecnicas` | Crea una nueva visita técnica. | `VisitaTecnicaCreateDto` | `VisitaTecnicaDetailDto` |
| **PUT** | `/api/visitas-tecnicas/{id}` | Modifica una visita técnica existente. | `VisitaTecnicaUpdateDto` | `VisitaTecnicaDetailDto` |
| **DELETE**| `/api/visitas-tecnicas/{id}` | Elimina una visita técnica del sistema. | Ninguno | `204 No Content` |

---

## 5. Códigos de Respuesta HTTP

El sistema de APIs REST implementa el siguiente estándar de respuestas:

* **`200 OK`**: La solicitud se completó correctamente y devuelve los datos correspondientes en el cuerpo de la respuesta.
* **`201 Created`**: La solicitud de creación (`POST`) finalizó con éxito. Devuelve la cabecera `Location` apuntando al recurso y el cuerpo con el DTO detallado del objeto creado.
* **`204 No Content`**: La solicitud de eliminación (`DELETE`) finalizó correctamente, y no se devuelve contenido en el cuerpo.
* **`400 Bad Request`**: Errores de validación de modelo (`ModelState`), argumentos inválidos u operaciones canceladas por reglas de negocio en la capa `BLL`. Devuelve un objeto JSON detallando el error.
* **`404 Not Found`**: El recurso especificado mediante ID no existe en el sistema.

---

## 6. Observaciones de Seguridad y Autenticación

> [!IMPORTANT]
> - **Autenticación MVC/Razor Intacta**: La seguridad del sistema actual basada en roles y cookies continúa operando de forma aislada para las vistas Razor tradicionales.
> - **Exposición de APIs**: Las APIs REST actuales se exponen de manera pública e independiente de las cookies del navegador (a través de `[IgnoreAntiforgeryToken]`) para servir de base inicial de integración.
> - **Estrategia para React + Vite**: En fases posteriores, se diseñará la estrategia de autenticación específica de la API (por ejemplo, JWT, cookies seguras HTTP-Only, o tokens de portador) que proteja estos endpoints de manera unificada para el frontend SPA.
> - **Usuarios y Roles**: La información sensible de credenciales (`Usuario`) no se expone a nivel de API pública en este sprint para prevenir vulnerabilidades de filtración.

---

## 7. Compatibilidad con clientes web y móviles

Los contratos REST definidos en esta API están diseñados bajo especificaciones estándar independientes de la plataforma. De esta manera, cualquier cliente que respete la serialización JSON, el uso de DTOs para estructurar payloads, el manejo semántico de códigos de estado HTTP y los futuros esquemas de autenticación centralizada (JWT/Tokens) podrá consumir estos servicios sin cambios en el backend. Esto garantiza la total compatibilidad tanto con el cliente de administración web (React + Vite) como con la aplicación móvil de campo (React Native / Expo).
