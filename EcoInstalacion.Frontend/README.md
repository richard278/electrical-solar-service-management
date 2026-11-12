# EcoInstalacion Frontend

SPA administrativa para la gestión de servicios técnicos e instalaciones eléctricas residenciales y solares de **EcoInstalacion / Electrical Solar Service Management**.

El frontend consume la API ASP.NET Core mediante autenticación JWT y presenta capacidades de consulta o gestión según el rol autenticado.

## Stack tecnológico

- React 19.
- TypeScript 6.
- Vite 8.
- `fetch` nativo mediante un cliente HTTP centralizado.
- `lucide-react` para iconografía.
- Oxlint para análisis estático.

## Capacidades actuales

- Login mediante `POST /api/auth/login`.
- Access token JWT conservado únicamente en memoria (`AuthContext`).
- Inclusión automática de `Authorization: Bearer <token>` en las solicitudes protegidas (`httpClient.ts`).
- Cierre de sesión manual y cierre automático al alcanzar `expiresAtUtc`.
- Panel con cinco secciones: Inicio (`OverviewPage`), Clientes (`ClientesPage`), Técnicos (`TecnicosPage`), Solicitudes (`SolicitudesPage`) y Visitas Técnicas (`VisitasTecnicasPage`).
- **Paginación y búsqueda en servidor (`DEFAULT_PAGE_SIZE = 25`, debounce `300 ms`)**:
  - `Clientes`: búsqueda por nombre o DUI (`q`) y filtro por estado (`activos`, `inactivos`, `todos` para `Administrador` mediante `/api/clientes/administracion`; solo activos para `Técnico` mediante `/api/clientes/paginados`).
  - `Técnicos`: búsqueda por nombre o especialidad (`q`) en `/api/tecnicos/paginados`.
  - `Solicitudes`: búsqueda por nombre de cliente (`q`) y filtro por estado (`Todas`, `Pendiente`, `Programada`, `En proceso`, `Cerrada`, `Rechazada`) en `/api/solicitudes/paginadas`.
  - `Visitas Técnicas`: búsqueda por nombre de cliente (`q`) y filtro de aprobación (`Todas`, `Pendientes`, `Aprobadas`) en `/api/visitas-tecnicas/paginadas`.
- Operaciones `GET`, `POST`, `PUT` y `DELETE` habilitadas conforme al rol y a las reglas de cada módulo.
- Estados de carga, error, confirmación y validación en los flujos de gestión.
- Navegación interna mediante estado de React en `AppShell.tsx`, sin React Router.

## Matriz funcional por rol

| Módulo | Administrador | Técnico |
| :--- | :--- | :--- |
| Clientes | Consultar activos, inactivos o todos; crear, editar y eliminar | Consultar clientes activos y crear |
| Técnicos | Consultar, crear, editar y eliminar según reglas de negocio | Consultar |
| Solicitudes | Consultar, crear, editar y eliminar según reglas de negocio | Consultar |
| Visitas Técnicas | Consultar, crear, editar, aprobar y eliminar según reglas de negocio | Consultar |

La API valida los permisos en el servidor. La interfaz también oculta o deshabilita las acciones que el rol no puede ejecutar.

## Requisitos locales

- Node.js y npm compatibles con las versiones declaradas en `package.json`.
- Backend ASP.NET Core en ejecución.
- Origen del frontend permitido por la política CORS del backend.

## Configuración

La URL base se obtiene de `VITE_API_BASE_URL` y usa `http://localhost:5240` como valor predeterminado.

Para crear la configuración local:

```bash
cp .env.example .env.local
```

Ejemplo:

```env
VITE_API_BASE_URL=http://localhost:5240
```

## Comandos de desarrollo

```bash
npm install
npm run dev
```

El servidor de Vite utiliza normalmente `http://localhost:5173`.

## Validación

```bash
npm run lint
npm run build
```

## Límites conocidos

- La sesión no se persiste: recargar la página requiere iniciar sesión nuevamente.
- No existen refresh tokens ni renovación silenciosa de sesión.
- La navegación por estado vuelve a Inicio al recargar y no ofrece enlaces profundos (`react-router` pendiente).
- La vista de resumen (`OverviewPage`) calcula contadores generales consultando los endpoints de listado completo (`GET /api/clientes`, `GET /api/tecnicos`, `GET /api/solicitudes`, `GET /api/visitas-tecnicas`) y todavía no integra la política de clasificación de alertas operativas de `BLL` (`OperationalAlertPolicy`).
- La administración de cuentas internas (`Usuarios`, `Roles`) y la exportación de reportes PDF (`QuestPDF`) operan en la interfaz heredada MVC/Razor y aún no están expuestas en la SPA React.
- Los listados de solicitudes y visitas para el rol `Técnico` son de solo lectura global y no filtran todavía registros por técnico asignado (`TecnicoAsignadoId`).
- No existe todavía una suite automatizada de pruebas del frontend (las pruebas unitarias existentes en `tests/EcoInstalacion.BLL.Tests` cubren la capa de dominio `.NET`).
- El despliegue unificado del SPA y ASP.NET Core continúa pendiente.

Para el contrato y la matriz de seguridad vigentes, consulte:

- [Contrato API REST](../docs/08-api-contract.md)
- [Diseño de autenticación y autorización API](../docs/10-auth-api-design.md)
- [Registro de deuda técnica](../docs/11-technical-debt-register.md)
