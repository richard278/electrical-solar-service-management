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

## Nota del Sprint

> [!NOTE]
> La integración con los endpoints REST del backend está programada y pendiente para el **Sprint 5C**. La configuración de la URL base del backend se gestiona mediante la variable de entorno `VITE_API_BASE_URL` en el archivo `.env`.
