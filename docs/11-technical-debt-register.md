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

- **Seguridad**: JWT Bearer, login, protección de endpoints y controles por rol están activos. Continúan pendientes refresh tokens, HTTPS productivo y pruebas automáticas.
- **API**: El contrato CRUD está operativo; faltan paginación, búsqueda avanzada en servidor y manejo global uniforme de excepciones.
- **Frontend**: Existe un panel modular con cinco secciones. La navegación usa estado de React y todavía no admite rutas profundas.
- **UI/UX**: El layout administrativo, los modales de gestión y la iconografía formal están implementados. Faltan búsquedas avanzadas y refinamientos responsivos.
- **Datos**: El frontend realiza mutaciones protegidas en los cuatro módulos operativos. La sincronización offline para un futuro cliente móvil permanece fuera del alcance actual.
- **Testing**: Lint y builds forman parte de la validación manual, pero no hay suites automáticas del frontend.
- **Deployment**: No existe todavía un empaquetado y despliegue unificado del SPA con ASP.NET Core.

## 3. Matriz de deuda técnica

| ID | Categoría | Descripción vigente | Riesgo | Prioridad | Estado | Siguiente acción |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **DT-01** | Seguridad | Proteger las APIs REST mediante autenticación y autorización. | **Alta** | **Alta** | **Resuelta** | Mantener pruebas de regresión sobre `401` y `403`. |
| **DT-02** | Seguridad | Proteger el acceso al panel y reflejar permisos por rol en React. | **Alta** | **Alta** | **Resuelta** | Cubrir el flujo con pruebas automáticas. |
| **DT-03** | Seguridad | Definir almacenamiento y ciclo de vida seguro de tokens. El access token ya vive en memoria; faltan refresh, rotación y revocación. | **Alta** | **Alta** | **Parcial** | Diseñar la estrategia productiva de renovación de sesión. |
| **DT-04** | Frontend | Incorporar enrutamiento SPA con URLs navegables, recarga estable y enlaces profundos. | **Media** | **Media** | **Pendiente** | Evaluar React Router en un incremento independiente. |
| **DT-05** | UI/UX | Contar con Header, Sidebar y paneles administrativos consistentes. | **Baja** | **Media** | **Resuelta** | Mantener accesibilidad y respuesta visual. |
| **DT-06** | Datos | Habilitar operaciones `POST`, `PUT` y `DELETE` desde React conforme a la matriz de roles. | **Alta** | **Alta** | **Resuelta** | Añadir cobertura automatizada de los flujos CRUD. |
| **DT-07** | API | Implementar manejo global y uniforme de excepciones y errores REST. | **Media** | **Media** | **Pendiente** | Diseñar middleware y formato de error común. |
| **DT-08** | API | Incorporar paginación para listados que puedan crecer. | **Media** | **Media** | **Pendiente** | Definir contrato paginado antes de cambiar consumidores. |
| **DT-09** | Frontend/API | Añadir filtros y búsqueda por nombre, DUI y criterios propios de cada módulo. Existe filtro de estado en Clientes. | **Baja** | **Media** | **Parcial** | Diseñar filtros coordinados entre API y frontend. |
| **DT-10** | UI/UX | Separar controles y acciones visibles según Administrador o Técnico. | **Media** | **Media** | **Resuelta** | Mantener sincronía con las políticas del backend. |
| **DT-11** | Testing | Crear suites unitarias y de integración para React y los flujos de autenticación. | **Media** | **Media** | **Pendiente** | Seleccionar herramientas y cubrir primero auth y permisos. |
| **DT-12** | Deployment | Estructurar el despliegue unificado del frontend estático y ASP.NET Core. | **Media** | **Baja** | **Pendiente** | Definir estrategia de build, configuración y hosting. |

## 4. Deuda resuelta en la consolidación actual

- Retiro de páginas React huérfanas y estilos sin consumidores activos.
- Alineación de la terminología del panel con las capacidades actuales.
- Sustitución de emojis del `AppShell` por iconografía formal.
- Corrección del README y de la documentación de contrato y autenticación.

Estas tareas mejoran mantenibilidad y presentación sin modificar navegación, permisos, llamadas API ni reglas de negocio.

## 5. Próximos frentes recomendados

### Frente A: búsqueda operativa

Diseñar de forma conjunta filtros por nombre, DUI, estado y criterios específicos de cada módulo, evitando filtrar grandes volúmenes únicamente en el navegador.

### Frente B: navegación estable

Evaluar React Router para conservar la sección activa al recargar, habilitar enlaces profundos y preparar rutas protegidas sin alterar la matriz de autorización.

### Frente C: robustez y seguridad

Incorporar manejo global de errores, refresh tokens, HTTPS productivo, pruebas automáticas y alcance de datos por Técnico.

### Frente D: escalabilidad y despliegue

Agregar paginación y definir la estrategia de publicación del SPA junto con ASP.NET Core.
