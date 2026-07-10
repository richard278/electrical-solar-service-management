# Registro de Deuda Técnica

Este documento registra y categoriza de manera sistemática los ítems de deuda técnica acumulados durante el proceso de modernización progresiva del sistema **EcoInstalacion / Electrical Solar Service Management**.

## 1. Propósito del documento

Mantener la visibilidad, evaluar el riesgo y planificar la refactorización e implementación de características faltantes para asegurar la calidad de nivel de producción del sistema, evitando que la velocidad de desarrollo comprometa la arquitectura a largo plazo.

## 2. Deuda Técnica por Categoría

* **Seguridad**: Falta de mecanismos de protección y autorización en los endpoints de la API REST que sirven como base para la integración de la SPA.
* **API**: Limitación de características avanzadas de servicio (paginación, filtros avanzados en servidor y normalización global de errores).
* **Frontend**: Estructura actual limitada a una sola pantalla (falta de enrutamiento y modularización de layouts).
* **UI/UX**: Interfaz en React simple y de previsualización sin menús de navegación funcionales ni componentes visuales de dashboard administrativo.
* **Datos**: Ausencia de operaciones de mutación (escritura) y de sincronización diferida (clave para el desarrollo móvil offline).
* **Testing**: Carencia de pruebas unitarias y de integración automáticas en la aplicación cliente de React.
* **Deployment**: Inexistencia de una estrategia integrada de empaquetado y despliegue del frontend estático y el backend ASP.NET Core de forma unificada.

---

## 3. Matriz de Registro de Deuda Técnica

El nivel de prioridad y el riesgo se evalúan conforme a la escala: **Alta**, **Media** y **Baja**.

| ID | Categoría | Descripción | Riesgo | Prioridad | Sprint Sugerido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **DT-01** | Seguridad | APIs REST expuestas públicamente y sin autenticación. | **Alta** | **Alta** | Sprint 5F | Pendiente |
| **DT-02** | Seguridad | Falta de protección de rutas y evaluación de roles en el frontend React. | **Alta** | **Alta** | Sprint 5G | Pendiente |
| **DT-03** | Seguridad | Falta de una estrategia formal de almacenamiento seguro de tokens de sesión. | **Alta** | **Alta** | Sprint 5G | Pendiente |
| **DT-04** | Frontend | Inexistencia de un enrutador SPA (React Router) para navegar por módulos. | **Media** | **Alta** | Sprint 5H | Pendiente |
| **DT-05** | UI/UX | Falta un layout administrativo (Sidebar, Header y paneles colapsables). | **Baja** | **Media** | Sprint 5H | Pendiente |
| **DT-06** | Datos | Falta de operaciones de escritura (POST/PUT/DELETE) desde el frontend. | **Alta** | **Alta** | Sprint 5I | Pendiente |
| **DT-07** | API | Ausencia de un middleware global de manejo de excepciones en la API REST. | **Media** | **Media** | Sprint 6A | Pendiente |
| **DT-08** | API | Falta de paginación en endpoints de listas masivas (Clientes, Solicitudes). | **Media** | **Media** | Sprint 6B | Pendiente |
| **DT-09** | Frontend | Ausencia de filtros y cuadros de búsqueda interactivos en las vistas SPA. | **Baja** | **Media** | Sprint 6B | Pendiente |
| **DT-10** | UI/UX | Falta de controles de visualización y separación por roles en la UI del frontend. | **Media** | **Media** | Sprint 6C | Pendiente |
| **DT-11** | Testing | Inexistencia de suites de pruebas automáticas (unitarias/integración) en React. | **Baja** | **Baja** | Sprint 6D | Pendiente |
| **DT-12** | Deployment| Falta estructurar el despliegue unificado (servir SPA estática desde ASP.NET). | **Media** | **Baja** | Sprint 6E | Pendiente |

---

## 4. Criterios de Prioridad y Definiciones de Riesgo

* **Prioridad Alta / Riesgo Alto**: Afecta directamente la seguridad de la información o bloquea el flujo básico de desarrollo del negocio. Debe resolverse de forma mandatoria antes de cualquier paso a pre-producción.
* **Prioridad Media / Riesgo Medio**: Limita la usabilidad o el rendimiento de la aplicación en producción. Impacta la mantenibilidad del código pero permite el funcionamiento básico controlado.
* **Prioridad Baja / Riesgo Bajo**: Características complementarias de experiencia de usuario, optimizaciones estéticas finas o automatización de pruebas que pueden posponerse sin comprometer la integridad estructural del monolito.

---

## 5. Roadmap Sugerido para Reducción de Deuda

El plan de mitigación para reducir de forma segura la deuda acumulada se divide en tres fases lógicas:

### Fase 1: Blindaje de la API (Sprints 5F y 5G)
Enfocada en resolver **DT-01**, **DT-02** y **DT-03**. Es el prerrequisito crítico. Consiste en habilitar la autenticación JWT en el backend y crear el contexto de seguridad en React para interceptar las peticiones y proteger las pantallas.

### Fase 2: Modularidad y Funcionalidad Completa (Sprints 5H y 5I)
Enfocada en resolver **DT-04**, **DT-05** y **DT-06**. Consiste en instalar React Router, diseñar la maquetación del panel principal con menús navegables y habilitar las operaciones de escritura (creación, edición y borrado) de clientes, técnicos y visitas técnicas a través de la API REST.

### Fase 3: Robustez Operacional (Sprints de la serie 6)
Resuelve la paginación (**DT-08**), manejo de errores del servidor (**DT-07**), robustez visual en filtros (**DT-09**), pruebas automatizadas (**DT-11**) y configuración final de despliegue (**DT-12**).
