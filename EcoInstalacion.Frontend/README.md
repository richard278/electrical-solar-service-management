# EcoInstalacion Frontend

Cliente web administrativo para la gestión de servicios técnicos e instalaciones eléctricas residenciales y solares del sistema **EcoInstalacion / Electrical Solar Service Management**.

## Stack Tecnológico

- **Framework**: React 18
- **Lenguaje**: TypeScript
- **Herramienta de Compilación**: Vite

---

## Comandos de Desarrollo

* **Instalación de dependencias**:
  ```bash
  npm install
  ```
* **Ejecutar en modo de desarrollo**:
  ```bash
  npm run dev
  ```
* **Compilar para producción**:
  ```bash
  npm run build
  ```

---

## Entornos y URLs de Integración

- **URL de desarrollo local**: `http://localhost:5173`
- **URL esperada del Backend**: `http://localhost:5240`

---

## Sprint 5C — API Client

- Se consume `GET /api/clientes` de forma asíncrona.
- Se consume `GET /api/tecnicos` de forma asíncrona.
- Se utiliza `fetch` nativo para todas las operaciones (sin Axios ni librerías adicionales).
- Requiere el backend ejecutándose en `http://localhost:5240` (con CORS habilitado).
- Requiere el frontend ejecutándose en `http://localhost:5173`.
- No incluye autenticación todavía.
- No incluye operaciones de escritura (POST, PUT, DELETE) todavía.

---

## Sprint 5D — Operational Dashboard

- Se consume `GET /api/solicitudes` de forma asíncrona.
- Se consume `GET /api/visitas-tecnicas` de forma asíncrona.
- Se consume `GET /api/visitas-tecnicas?solicitudId={id}` de forma asíncrona.
- Se utiliza `fetch` nativo para todas las operaciones (sin Axios ni librerías adicionales).
- Requiere el backend ejecutándose en `http://localhost:5240` (con CORS habilitado).
- Requiere el frontend ejecutándose en `http://localhost:5173`.
- No incluye autenticación todavía.
- No incluye operaciones de escritura (POST, PUT, DELETE) ni formularios todavía.

---

## Sprint 5F — React Navigation and Read-Only Entity Panels

- **Navegación interna**: Implementada mediante estado de React (`useState`) en `AppShell.tsx` (sin React Router).
- **Vistas por entidad**: Paneles de solo lectura detallados para cada entidad principal del negocio.
- **Endpoints consumidos**:
  - `GET /api/clientes` (Métricas de clientes y catálogo tabular).
  - `GET /api/tecnicos` (Métricas de técnicos y personal calificado).
  - `GET /api/solicitudes` (Métricas de solicitudes y control de estado).
  - `GET /api/visitas-tecnicas` (Registro detallado e inspecciones aprobadas/pendientes).
- **Restricciones respetadas**:
  - No incluye autenticación de usuario (JWT o Cookies).
  - No incluye operaciones de escritura (POST, PUT, DELETE).
  - No incluye formularios de entrada.
  - No se instalaron dependencias ni librerías adicionales.
