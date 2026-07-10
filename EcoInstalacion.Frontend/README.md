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
