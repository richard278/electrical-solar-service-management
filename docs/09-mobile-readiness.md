# Preparación para Cliente Móvil React Native

Este documento describe la preparación arquitectónica del backend **EcoInstalacion / Electrical Solar Service Management** para soportar de manera nativa y desacoplada un cliente móvil construido con **React Native / Expo** para técnicos en campo.

## 1. Propósito del documento

Establecer las bases de diseño técnico, contratos de API reutilizables, consideraciones de conectividad y el mapa de ruta (roadmap) para extender el backend API-first hacia el canal móvil.

## 2. Visión Multi-Cliente

El backend basado en ASP.NET Core está diseñado bajo un modelo API-first, lo que permite que una única API REST sirva a múltiples aplicaciones cliente de manera simultánea:

* **Backend .NET Core (API-first)**: Orquestador único de persistencia, reglas de negocio e integraciones.
* **React + Vite (Cliente Web Administrativo)**: SPA enfocada en la administración centralizada, gestión de catálogos y reportería.
* **React Native / Expo (Cliente Móvil de Campo)**: Aplicación híbrida de alto rendimiento optimizada para su uso en dispositivos iOS y Android por técnicos instaladores en campo.

## 3. Caso de Uso Móvil Principal

* **App móvil para técnicos en campo**: Los técnicos asignados a instalaciones solares y eléctricas residenciales requieren una herramienta ligera y portátil para gestionar sus tareas cotidianas en el lugar de la instalación.

## 4. Módulos API Relevantes para Móvil

El cliente móvil operará principalmente sobre tres módulos del backend:

1. **Técnicos**: Para identificar el perfil y estado de disponibilidad del técnico.
2. **Solicitudes**: Para acceder a los detalles técnicos de las instalaciones asignadas.
3. **Visitas Técnicas**: Para reportar, editar y validar el resultado de las inspecciones en sitio.

## 5. Flujos Móviles Candidatos

* **Consultar solicitudes asignadas**: El técnico visualiza un listado con las tareas y ubicaciones de las instalaciones que tiene asignadas.
* **Ver detalle de solicitud**: Visualización de la descripción, tipo de instalación y datos del cliente asociados a una solicitud específica.
* **Registrar visita técnica**: Creación de un registro para reportar la llegada y el diagnóstico inicial en el sitio.
* **Actualizar observaciones**: Edición del informe técnico directamente en la visita.
* **Marcar visita como aprobada o no aprobada**: Cambio de estado de la inspección técnica una vez culminados los trabajos de validación.

## 6. Contratos Actuales Reutilizables y Permisos Vigentes (`Baseline a1cc4ef`)

Los endpoints expuestos en la API REST de `EcoInstalacion.AppWeb` ofrecen la base de contratos JSON y paginación para un futuro cliente móvil, con la siguiente distinción entre operaciones ya accesibles para el rol `Técnico` y operaciones actualmente restringidas a `Administrador`:

### 6.1 Endpoints de autenticación y consulta (disponibles para `Administrador` y `Técnico`)

* `POST /api/auth/login`: Autenticación mediante credenciales (`AuthLoginRequestDto`) y emisión de token JWT Bearer (`AuthLoginResponseDto`).
* `GET /api/tecnicos` y `GET /api/tecnicos/paginados?q={texto}&page=1&pageSize=25`: Recuperación del catálogo de personal técnico.
* `GET /api/solicitudes` y `GET /api/solicitudes/paginadas?q={texto}&estado={estado}&page=1&pageSize=25`: Consulta de solicitudes de instalación con búsqueda por cliente y filtro por estado.
* `GET /api/solicitudes/{id}`: Recupera información técnica detallada del cliente, técnico asignado y descripción de la solicitud.
* `GET /api/visitas-tecnicas?solicitudId={id}` y `GET /api/visitas-tecnicas/paginadas?q={texto}&aprobado={bool}&page=1&pageSize=25`: Consulta del historial de visitas técnicas.
* `GET /api/visitas-tecnicas/{id}`: Detalle de una visita técnica específica.

### 6.2 Endpoints de escritura en visitas (actualmente restringidos a `ApiAdminOnly`)

* `POST /api/visitas-tecnicas` (`VisitaTecnicaCreateDto`) y `PUT /api/visitas-tecnicas/{id}` (`VisitaTecnicaUpdateDto`): En el baseline público actual (`a1cc4ef`), ambos endpoints exigen la política **`ApiAdminOnly`** (`Administrador`), por lo que un usuario autenticado con rol `Técnico` recibe `403 Forbidden`.
* **Requisito previo para operación móvil de campo**: Antes de habilitar que un técnico de campo registre o edite visitas desde la app móvil, se deberá definir e implementar una política de autorización específica y reglas de alcance por técnico asignado (`TecnicoAsignadoId`) en el backend.

## 7. Consideraciones Técnicas para React Native

* **Comportamiento CORS**: A diferencia de los navegadores web tradicionales (como el que ejecutará el cliente React + Vite), las aplicaciones nativas móviles de Android e iOS no están sujetas a restricciones del mismo origen (CORS). Sin embargo, mantener las cabeceras CORS en el servidor sigue siendo crucial para el cliente web.
* **Configuración de la URL Base**: La dirección del servidor no debe estar quemada en el código. Debe utilizarse un archivo de configuración de variables de entorno para apuntar a diferentes servidores (desarrollo, QA, producción).
* **Entorno de Emulador Android**: En el emulador local de Android, la dirección IP `127.0.0.1` apunta al emulador mismo. Para realizar peticiones al backend local ejecutándose en la máquina anfitriona, se debe apuntar al alias **`10.0.2.2`** (ej. `http://10.0.2.2:5240`).
* **Pruebas en Dispositivo Físico**: Si se depura en un teléfono real conectado por USB o Wi-Fi, este debe estar en la misma red de área local (LAN) que el backend. La URL de la API debe apuntar a la dirección IP local de la computadora (ej. `http://192.168.1.XX:5240`) o bien utilizar un proxy/túnel de desarrollo seguro (ej. `ngrok` o `dev tunnels` de VS).
* **Estabilidad del Contrato**: Al compilar e instalar la app en los dispositivos de los usuarios, es más difícil realizar actualizaciones inmediatas. Los DTOs deben mantenerse estables y con retrocompatibilidad.
* **Mensajes de Error Consistentes**: En caso de fallas de negocio o de red, el backend debe responder siempre con un payload JSON estructurado para que el cliente móvil pueda interpretar y pintar alertas amigables al usuario en lugar de páginas de error HTML de ASP.NET Core.
* **Autenticación JWT e Integración Móvil**: El backend ya expone `POST /api/auth/login` con emisión de JWT Bearer. Para el canal móvil resta definir el almacenamiento seguro en dispositivo (ej. `expo-secure-store`) y la estrategia de renovación de sesión (`refresh tokens`).

## 8. Consideraciones de Evolución Futura

Para escalar la aplicación móvil a un ambiente productivo maduro, se deben considerar los siguientes requerimientos arquitectónicos en fases posteriores:

* **Alcance y Permisos de Técnico en Campo**: Filtrado en servidor de solicitudes y visitas por `TecnicoAsignadoId`, y habilitación controlada de registro/actualización de observaciones de visitas para el técnico asignado (manteniendo la aprobación final reservada o regulada según regla de negocio).
* **Refresh Tokens y Almacenamiento Seguro**: Renovación automática de tokens JWT expirados para no forzar al técnico a iniciar sesión constantemente en campo.
* **Aprovechamiento de Paginación Existente**: Consumo móvil de los endpoints `/api/solicitudes/paginadas` y `/api/visitas-tecnicas/paginadas` ya disponibles en el backend para optimizar el consumo de datos móviles.
* **Modo Offline**: Almacenamiento local (ej. `AsyncStorage` o `SQLite` en el dispositivo) para permitir a los técnicos registrar visitas y observaciones en zonas sin cobertura de internet.
* **Sincronización**: Lógica de sincronización diferida en la app móvil que envíe los registros locales acumulados al backend una vez detectada la reconexión.
* **Captura de Fotos**: Carga de imágenes (evidencia fotográfica de la instalación) mediante endpoints multipart en el backend.
* **Firma del Cliente**: Captura de firmas digitalizadas en pantalla y almacenamiento de las coordenadas de trazo o imágenes de firma.
* **Geolocalización**: Envío de coordenadas GPS asociadas al momento y lugar del registro de la visita técnica para auditoría.
* **Notificaciones Push**: Integración con servicios de mensajería (ej. Firebase Cloud Messaging - FCM o Expo Push Notifications) para alertar al técnico en tiempo real sobre nuevas solicitudes asignadas.

---

## 9. Roadmap de Desarrollo Móvil Sugerido

El desarrollo del cliente móvil (actualmente en estado planificado; no existe proyecto React Native / Expo en el repositorio) se estructurará bajo el siguiente plan progresivo:

* **Sprint M1: Scaffold React Native / Expo**
  * Inicialización del proyecto usando Expo CLI con TypeScript.
  * Configuración del sistema de navegación básica (Expo Router o React Navigation).
* **Sprint M2: Configuración del API Client y Autenticación JWT**
  * Implementación de cliente HTTP (`fetch`) configurado para detectar entornos locales, emuladores y producción.
  * Integración con `POST /api/auth/login` y almacenamiento seguro del token en el dispositivo.
* **Sprint M3: Pantalla de Solicitudes Asignadas**
  * Maquetación de la pantalla principal del técnico de campo.
  * Consumo de `GET /api/solicitudes/paginadas` e incorporación del futuro filtro por técnico asignado.
* **Sprint M4: Detalle de Solicitud**
  * Creación de la pantalla de detalle (`GET /api/solicitudes/{id}` y `GET /api/visitas-tecnicas?solicitudId={id}`).
  * Carga y presentación de los campos del cliente e instrucciones técnicas.
* **Sprint M5: Registro y Seguimiento de Visita Técnica**
  * Ajuste previo de políticas de autorización en backend para permitir el reporte en sitio por parte del técnico asignado.
  * Pantalla con formulario de visitas técnicas e integración con los endpoints correspondientes.
