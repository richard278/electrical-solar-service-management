# Arquitectura Objetivo

Este documento describe la arquitectura objetivo definida para el proyecto **Electrical Solar Service Management / EcoInstalacion**. Detalla el proceso de modernización progresiva y los pasos requeridos para transicionar de una aplicación basada puramente en servidor (MVC/Razor) a una solución SPA desacoplada con **React + Vite**.

## 1. Estado Actual

El sistema está construido en .NET 8 utilizando una arquitectura clásica por capas:

* **EcoInstalacion.AppWeb**: Capa de presentación (MVC y Vistas Razor).
* **EcoInstalacion.BLL**: Capa de lógica de negocio (servicios). Completamente desacoplada de Entity Framework Core.
* **EcoInstalacion.DAL**: Capa de acceso a datos (repositorios y DbContext único).
* **EcoInstalacion.Entity**: Capa de entidades de base de datos auto-contenidas.
* **EcoInstalacion.IOC**: Capa de contenedor de inversión de control para inyección de dependencias.

## 2. Principio de Modernización Progresiva

El rediseño de la arquitectura sigue el principio de **modernización progresiva** (o estrangulación gradual de la aplicación monolítica). Esto significa que:

1. **Convivencia**: Las vistas Razor y los controladores MVC continúan funcionando a la par de los nuevos controladores de API REST.
2. **Sin Cambios Disruptivos**: No se altera la base de datos, el esquema relacional ni la lógica de negocio en la capa `BLL`.
3. **API-First**: Se construyen y validan todos los contratos de API y DTOs en el backend antes de iniciar el desarrollo de la interfaz de usuario en el frontend SPA.

---

## 3. Arquitectura Objetivo

La arquitectura de destino migrará la responsabilidad de la presentación a un cliente independiente, convirtiendo al backend .NET en un servidor puramente API REST.

```
+-------------------------------------------------------------+
|                      React + Vite (SPA)                     |
+-------------------------------------------------------------+
                               |
                               | (HTTP/JSON - REST)
                               v
+-------------------------------------------------------------+
|                    ASP.NET Core Web API                     |
|            (ApiControllers - DTOs Manual Mappings)          |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                 EcoInstalacion.BLL (Negocio)                |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                 EcoInstalacion.DAL (Repositorios)           |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                      SQL Server Database                    |
+-------------------------------------------------------------+
```

### Componentes de la Arquitectura de Destino

* **Frontend SPA (React + Vite)**: Aplicación SPA moderna escrita en TypeScript, que consume la API REST del backend de forma asíncrona.
* **Contratos (DTOs)**: Capa de transferencia de datos en `AppWeb` que define el esquema exacto de entrada y salida del cliente React, abstrayendo las entidades de base de datos.
* **Mapeo Manual**: Los controladores de API web traducen manualmente las entidades a DTOs y viceversa, manteniendo el control total de los datos expuestos y evitando la sobrecarga o configuraciones mágicas de librerías como AutoMapper.
* **Capa de Negocio (BLL)**: Almacena las validaciones de negocio y flujos lógicos, garantizando la consistencia tanto para las llamadas Razor heredadas como para la API REST.
* **Persistencia (DAL)**: Administra el acceso a SQL Server por medio de Entity Framework Core bajo un patrón de repositorios genéricos y específicos.

---

## 4. Estado de Módulos y APIs Completados

Se han implementado y verificado bajo el estándar API-first los siguientes módulos:

* [x] **Clientes** (`ClientesApiController` + DTOs)
* [x] **Técnicos** (`TecnicosApiController` + DTOs)
* [x] **Roles** (`RolesApiController` + DTOs)
* [x] **Solicitudes de Instalación** (`SolicitudesApiController` + DTOs)
* [x] **Visitas Técnicas** (`VisitasTecnicasApiController` + DTOs)

### Módulos Sensibles Pendientes de Migración

* **Usuarios**: Requiere diseño y mapeo cuidadoso de datos personales y de acceso.
* **Autenticación y Autorización**: Requiere la implementación de un esquema compatible con arquitecturas desacopladas (JWT o cookies HTTP-Only de origen cruzado).

---

## 5. Criterios de Aceptación antes de iniciar React

Antes de iniciar el desarrollo del frontend SPA, se deben garantizar los siguientes aspectos técnicos en el backend:

* **Contrato API Documentado**: Disponibilidad del documento completo del contrato de la API.
* **Endpoints Principales Validados**: Pruebas de funcionamiento correctas en los métodos GET, POST, PUT y DELETE.
* **Configuración CORS**: Habilitación en `Program.cs` del intercambio de recursos de origen cruzado (CORS) para permitir peticiones desde el servidor de desarrollo de Vite (ej. `http://localhost:5173`).
* **Estrategia de Autenticación SPA**: Definición clara del almacenamiento y transmisión de credenciales o tokens.
* **Manejo de Errores Consistente**: Respuesta estandarizada ante excepciones del servidor y fallos de validación.

---

## 6. Roadmap Sugerido de Modernización

El siguiente mapa de ruta detalla el avance progresivo planificado para la integración con React:

* **Sprint 5A: CORS y configuración frontend-ready**
  * Habilitación de políticas CORS en `Program.cs`.
  * Validación del comportamiento del ruteo API.
* **Sprint 5B: Scaffold de React + Vite**
  * Inicialización del proyecto React con TypeScript y Vite.
  * Estructura base de carpetas (components, hooks, services, views).
* **Sprint 5C: Cliente de API en el Frontend**
  * Configuración de Axios/Fetch.
  * Implementación de servicios para Clientes y Técnicos.
* **Sprint 5D: Dashboard Operacional**
  * Creación de vistas para gestionar Solicitudes e Instalaciones.
  * Integración con la API de Visitas Técnicas.
* **Sprint 5E: Autenticación en el Frontend**
  * Migración del inicio de sesión Razor a un flujo de API REST.
  * Gestión de tokens/sesión y protección de rutas en React.
