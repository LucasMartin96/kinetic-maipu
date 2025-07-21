
# Sistema de Procesamiento de Documentos

## Descripción General

Este proyecto es un sistema de procesamiento de documentos desarrollado en .NET, diseñado para cargar, procesar y analizar archivos de texto de forma asíncrona. Expone su funcionalidad a través de una API REST que permite controlar y monitorear los procesos de análisis de documentos.

## Funcionalidades Principales

- **API RESTful** para iniciar, consultar estado y obtener resultados de procesos de análisis de documentos.
- **Procesamiento asíncrono real** de archivos de texto en lotes.
- **Extracción de estadísticas**: conteo de palabras, líneas y caracteres.
- **Identificación de palabras más frecuentes** (excluyendo stop words comunes).
- **Generación de resumen simple** del contenido de los documentos.
- **Persistencia de estados y resultados** de los procesos.
- **Registro de actividad (logs)** para auditoría y monitoreo.

## Endpoints Disponibles

- `POST /process/start`: Inicia un nuevo proceso de análisis. Ahora acepta archivos directamente.
    - Requiere una solicitud `multipart/form-data`
    - Límite de 50 archivos, máximo 10MB por archivo, 100MB en total
    - Extensiones aceptadas: `.txt`, `.md`, `.csv`
- `POST /process/stop/{process_id}`: Detiene un proceso específico.
- `GET /process/status/{process_id}`: Consulta el estado de un proceso.
- `GET /process/list`: Lista todos los procesos y sus estados.
- `GET /process/results/{process_id}`: Obtiene los resultados del análisis.

## Estados del Proceso

- **PENDING**: Proceso creado pero no iniciado.
- **RUNNING**: Procesamiento en curso.
- **COMPLETED**: Proceso finalizado con éxito.
- **FAILED**: Proceso terminado con errores.

## Instalación y Ejecución

1. Clona el repositorio:
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   ```
2. Ingresa al directorio del proyecto y restaura los paquetes:
   ```bash
   cd DocumentProcessor
   dotnet restore
   ```
3. Ejecuta la solución:
   ```bash
   dotnet build
   dotnet run --project DocumentProcessor.API/DocumentProcessor.API.csproj
   ```
4. Accede a la documentación Swagger en:
   ```
   http://localhost:<puerto>/swagger
   ```

## Uso con Docker

1. Para iniciar todos los servicios con Docker Compose:
   ```bash
   cd DocumentProcessor
   docker compose up -d
   ```
2. Verificar que los servicios estén corriendo:
   ```bash
   docker compose ps
   ```
3. Ver logs de ejecución:
   ```bash
   docker compose logs -f
   ```
4. Acceder a la API desde Docker:
   ```
   http://localhost:5000/swagger
   ```

## Health Checks

- **MySQL**: se verifica con `mysqladmin ping`
- **RabbitMQ**: se verifica con `rabbitmq-diagnostics ping`
- Los servicios esperan a que las dependencias estén listas gracias a `condition: service_healthy` en el `docker-compose.yml`
- Esto evita errores de timing y race conditions

## Pruebas y Datos de Ejemplo

- El proyecto incluye al menos 10 archivos de texto de ejemplo (mínimo 500 palabras cada uno) para pruebas.
- Se provee una colección de Postman/Insomnia para probar los endpoints.

## Estrategia de Resiliencia

1. **Reinicio de la aplicación durante un proceso:**
   - El sistema persiste el estado de los procesos y archivos, permitiendo reanudar o recuperar el estado tras un reinicio inesperado.
2. **Manejo de archivos corruptos:**
   - Si un archivo dentro de un lote está corrupto, el sistema lo marca como fallido y continúa procesando el resto, registrando el error en los logs.

## Enfoque y Decisiones de Diseño

El sistema fue diseñado bajo una arquitectura de microservicios orientada a eventos, utilizando MassTransit como middleware de mensajería sobre RabbitMQ. La solución implementa el patrón Saga para la orquestación de procesos.

### Decisiones Clave

- **Modelo asincrónico y desacoplado**: Flujo completamente asincrónico, escalable horizontalmente.
- **Saga como orquestador/coordinador**: Se implementó `ProcessSaga`, controlando el proceso con estado persistente (en memoria).
- **Separación de responsabilidades**:
  - API: Control y monitoreo.
  - Writer: Persistencia en base de datos.
  - Worker: Procesamiento individual de archivos.
  - Master (Saga Host): Control de ejecución.
  - DAO: Interfaces específicas por servicio, inyectadas vía DI.
  - Contracts: Contratos centralizados en librería compartida.
- **Persistencia y tracking de estado**: Persistencia de estado del Saga en memoria (mejorable).
- **Resiliencia**: Retries y timeouts configurados.
- **Escalabilidad**: Workers stateless, escalables horizontalmente.

Nota: Más info sobre diseño en `architecture.md`

## Escalabilidad

### Cambios Clave para Escalar a Millones de Documentos Diarios

1. **Escalamiento del Writer con idempotencia**: Uso de claves únicas.
2. **Persistencia del estado del Saga**: En base de datos transaccional.
3. **Monitoreo y métricas en tiempo real**: Throughput, DLQ, retries.

### Infraestructura Escalable

- **Mensajería**: RabbitMQ clúster / Amazon MQ / Azure Service Bus
- **Contenedores y orquestación**: Kubernetes (EKS, AKS) o AWS ECS
- **Base de datos escalable**: Aurora, Azure SQL Hyperscale, Cosmos DB
- **Archivos y logs**: Amazon S3 / Azure Blob Storage

## Política de Uso de IA

Se utilizó ChatGPT:

- https://chatgpt.com/share/687e883a-c648-8006-9841-18db63ddadb0
- https://chatgpt.com/share/687e9725-f06c-8006-87b9-67857f8af27d
- https://chatgpt.com/share/687e974f-9a50-8006-8508-cee88ea47545
- https://chatgpt.com/share/687ea430-d274-8006-93c4-4605b9e68554

La razón fue validación de arquitectura pensada, solución de errores durante el desarrollo y dudas puntuales sobre tiempos.

## Colección de Postman

Se encuentra en el documento `DocumentProcessor.postman_collection.json`

## Bonus

- **Implementado**:
  - Serilog en consola
  - Docker Compose con servicios en contenedores separados
- **No implementado**:
  - Interfaz web y WebSockets: Se usaría React + SignalR para streaming de eventos (Hub para progreso y completado).

## Archivos de Texto

En carpeta `test-files` de `DocumentProcessor`, obtenidos del Proyecto Gutenberg (libres para uso).
