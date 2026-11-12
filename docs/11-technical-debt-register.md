# Registro de Deuda Técnica

Este documento mantiene el estado de la deuda técnica durante la modernización de **EcoInstalacion / Electrical Solar Service Management**. Los ítems resueltos permanecen registrados para conservar trazabilidad.

## 1. Criterios de estado

| Estado | Definición |
| :--- | :--- |
| **Resuelta** | La capacidad base está implementada y validada dentro del alcance actual. |
| **Parcial** | Existe una solución funcional, pero falta endurecimiento o cobertura relevante. |
| **Pendiente** | La capacidad todavía no está implementada. |

La prioridad y el riesgo se clasifican como **Alta**, **Media** o **Baja**.

## 2. Situación actual por categoría

- **Seguridad**: JWT Bearer, login, protección de endpoints y controles por rol están activos. Continúan pendientes refresh tokens, HTTPS productivo y pruebas automáticas de seguridad.
- **API**: El contrato CRUD y los endpoints de búsqueda y paginación en servidor (`/paginados` y `/paginadas` con `PagedResponse<T>`) están operativos en los cuatro módulos principales. Falta el manejo global uniforme de excepciones (`ProblemDetails`).
- **Frontend**: Existe un panel modular con cinco secciones (`Inicio`, `Clientes`, `Técnicos`, `Solicitudes`, `Visitas Técnicas`) con paginación server-side y búsqueda con debounce (`300 ms`). La navegación usa estado de React y todavía no admite rutas profundas (`react-router`), y `OverviewPage` (`Inicio`) consulta aún los endpoints de listado completo para calcular totales sin exponer las alertas de `OperationalAlertPolicy`.
- **UI/UX**: El layout administrativo, los modales de gestión, la paginación, los filtros por estado/aprobación y la iconografía formal están implementados.
- **Datos**: El frontend realiza mutaciones protegidas en los cuatro módulos operativos. Los módulos de `Usuarios` y exportación de reportes PDF (`QuestPDF`) permanecen en la superficie MVC/Razor heredada. La sincronización offline y los permisos de escritura en campo para un futuro cliente móvil permanecen fuera del alcance actual.
- **Testing**: En el backend existe la suite unitaria xUnit [`tests/EcoInstalacion.BLL.Tests`](../tests/EcoInstalacion.BLL.Tests) (`26` métodos declarados / `49` casos ejecutables para `OperationalAlertPolicy` y `ElSalvadorOperationalClock`). Continúan pendientes las suites automatizadas del frontend React y de los controladores HTTP/API.
- **Deployment**: No existe todavía un empaquetado y despliegue unificado del SPA con ASP.NET Core, ni scripts públicos de inicialización SQL/migraciones, Docker o GitHub Actions.

## 3. Matriz de deuda técnica

| ID | Categoría | Descripción vigente | Riesgo | Prioridad | Estado | Siguiente acción |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **DT-01** | Seguridad | Proteger las APIs REST mediante autenticación y autorización. | **Alta** | **Alta** | **Resuelta** | Mantener pruebas de regresión sobre `401` y `403`. |
| **DT-02** | Seguridad | Proteger el acceso al panel y reflejar permisos por rol en React. | **Alta** | **Alta** | **Resuelta** | Cubrir el flujo con pruebas automáticas. |
| **DT-03** | Seguridad | Definir almacenamiento y ciclo de vida seguro de tokens. El access token ya vive en memoria; faltan refresh, rotación y revocación. | **Alta** | **Alta** | **Parcial** | Diseñar la estrategia productiva de renovación de sesión. |
| **DT-04** | Frontend | Incorporar enrutamiento SPA con URLs navegables, recarga estable y enlaces profundos. | **Media** | **Media** | **Pendiente** | Evaluar React Router en un incremento independiente. |
| **DT-05** | UI/UX | Contar con Header, Sidebar y paneles administrativos consistentes. | **Baja** | **Media** | **Resuelta** | Mantener accesibilidad y respuesta visual. |
| **DT-06** | Datos | Habilitar operaciones `POST`, `PUT` y `DELETE` desde React conforme a la matriz de roles. | **Alta** | **Alta** | **Resuelta** | Añadir cobertura automatizada de los flujos CRUD. |
| **DT-07** | API | Implementar manejo global y uniforme de excepciones y errores REST (`ValidationProblem` vs `{ "message" }` vs `{ "mensaje" }`). | **Media** | **Media** | **Pendiente** | Diseñar middleware y formato de error común. |
| **DT-08** | API / Frontend | Incorporar paginación server-side (`PagedResponse<T>`) para listados operativos en API y React SPA. | **Media** | **Media** | **Resuelta** | Evaluar en el futuro un endpoint de métricas para `OverviewPage` sin descargar listados completos. |
| **DT-09** | Frontend / API | Añadir búsqueda server-side (`q` con debounce `300 ms`) y filtros por módulo (`estado` en Clientes y Solicitudes, `aprobado` en Visitas Técnicas, `Nombre`/`Especialidad` en Técnicos). | **Baja** | **Media** | **Resuelta** | Evaluar filtros multicriterio adicionales solo si la operación lo requiere. |
| **DT-10** | UI/UX | Separar controles y acciones visibles según Administrador o Técnico. | **Media** | **Media** | **Resuelta** | Mantener sincronía con las políticas del backend. |
| **DT-11** | Testing | Crear suites automatizadas para la capa de dominio, API y React. Existe cobertura xUnit en `BLL` (`OperationalAlertPolicy` y reloj operativo); faltan pruebas de API, autenticación y frontend. | **Media** | **Media** | **Parcial** | Cubrir controladores API, autenticación y componentes React. |
| **DT-12** | Deployment | Estructurar el despliegue unificado del frontend estático y ASP.NET Core, junto con scripts SQL reproducibles y CI/CD. | **Media** | **Baja** | **Pendiente** | Definir estrategia de build, migraciones/bootstrap SQL y hosting. |

## 4. Deuda resuelta en la consolidación actual

- Implementación de paginación y búsqueda server-side (`PagedResponse<T>`) en `Clientes`, `Técnicos`, `Solicitudes` y `Visitas Técnicas`, integrada en la SPA React (`DT-08`, `DT-09`).
- Cobertura de pruebas unitarias xUnit en `tests/EcoInstalacion.BLL.Tests` para `OperationalAlertPolicy` y `ElSalvadorOperationalClock` (`DT-11` parcial).
- Retiro de páginas React huérfanas y estilos sin consumidores activos.
- Alineación de la terminología del panel con las capacidades actuales y sustitución de emojis del `AppShell` por iconografía formal.
- Alineación del README principal y de los documentos técnicos de contrato API, autenticación, modelo de datos y preparación móvil (`ECO-PORT-01` y `ECO-PORT-02`).

Estas tareas mejoran mantenibilidad y presentación sin modificar navegación, permisos, llamadas API ni reglas de negocio.

## 5. Próximos frentes recomendados

### Frente A: dashboard operativo y métricas

Exponer las métricas agregadas y la clasificación de alertas de `OperationalAlertPolicy` hacia el dashboard de React (`OverviewPage`), evitando consultar los cuatro listados completos en el arranque.

### Frente B: navegación estable

Evaluar React Router para conservar la sección activa al recargar, habilitar enlaces profundos y preparar rutas protegidas sin alterar la matriz de autorización.

### Frente C: robustez, seguridad y alcance por técnico

Incorporar manejo global uniforme de errores (`DT-07`), refresh tokens (`DT-03`), HTTPS productivo, pruebas automáticas de API/frontend (`DT-11`) y filtrado/permisos por técnico asignado para un futuro cliente móvil.

### Frente D: reproducibilidad de base de datos y despliegue

Versionar el script de inicialización SQL (tablas, trigger `dbo.tr_ActualizarEstadoSolicitud` y procedimiento almacenado de reportes) y definir la estrategia de publicación del SPA junto con ASP.NET Core (`DT-12`).
