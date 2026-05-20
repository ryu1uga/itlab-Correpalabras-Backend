# OpenShift Deployment Guide para CorrePalabras API

## Resumen de Cambios Realizados

Se han implementado mejoras críticas para hacer el proyecto compatible con OpenShift:

### 1. ✅ Dockerfile Corregido
- **Cambio**: Se removió el `--environment=Development` hardcodeado
- **Cambio**: Se agregó `ENV ASPNETCORE_ENVIRONMENT=Production` como default
- **Razón**: OpenShift debe controlar el ambiente mediante variables de entorno

**Antes:**
```dockerfile
ENTRYPOINT ["dotnet", "CorrePalabras.dll", "--environment=Development"]
```

**Después:**
```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "CorrePalabras.dll"]
```

### 2. ✅ .dockerignore Creado
- Optimiza las imágenes Docker excluyendo archivos innecesarios
- Reduce el tamaño de la imagen y el tiempo de build

### 3. ✅ Credenciales Removidas del Código
- **appsettings.json**: Vacío de credenciales sensibles
- **appsettings.Production.json**: Nuevo archivo sin credenciales
- **Razón**: Las credenciales se inyectarán via Secrets de OpenShift

### 4. ✅ Health Check Verificado
- El endpoint `/api/healthcheck` ya existe y funciona
- OpenShift usa este endpoint para:
  - **Liveness Probe**: Detectar si el pod está vivo
  - **Readiness Probe**: Detectar si está listo para recibir tráfico

### 5. ✅ Archivos de OpenShift Creados

Se encuentran en `openshift/`:

- **imagestream.yml**: Define la imagen Docker en OpenShift
- **buildconfig.yml**: Configuración del build desde GitHub
- **deploymentconfig.yml**: Configuración del deployment con:
  - 3 replicas para alta disponibilidad
  - Health checks (liveness y readiness)
  - Limits y requests de recursos
  - Manejo seguro de variables de entorno
  - Contexto de seguridad (no-root, read-only filesystem)
- **service.yml**: Expone la aplicación internamente
- **route.yml**: Expone la aplicación al exterior (con HTTPS)
- **secrets-config.yml**: Plantilla para secrets y ConfigMaps

### 6. ✅ GitHub Actions Workflow Creado

Archivo: `.github/workflows/build-and-deploy.yml`

**Pasos:**
1. Build del proyecto .NET
2. Ejecución de tests (opcional)
3. Build y push a GitHub Container Registry
4. Deployment automático en OpenShift

---

## Instrucciones de Deployment en OpenShift

### Paso 1: Preparación Inicial

1. **Instalar CLI de OpenShift:**
   ```bash
   # En Windows (si no está instalado)
   # Descargar de: https://mirror.openshift.com/pub/openshift-v4/clients/ocp/latest/
   ```

2. **Loguearse en OpenShift:**
   ```bash
   oc login --token=YOUR_TOKEN --server=https://your-openshift-server:6443
   ```

### Paso 2: Crear Namespace y Secrets

1. **Ejecutar el archivo de secrets:**
   ```bash
   oc apply -f openshift/secrets-config.yml
   ```

2. **Actualizar los secrets con valores reales:**
   ```bash
   # Connection String
   oc set env secret/correpalabras-db CONNECTION_STRING="Server=your-db;Database=correpalabras;..." -n correpalabras

   # JWT Secrets
   oc set env secret/correpalabras-secrets \
     JWT_KEY="your-secret-key" \
     JWT_ISSUER="your-issuer" \
     JWT_AUDIENCE="your-audience" \
     -n correpalabras

   # Email Configuration
   oc set env secret/correpalabras-email \
     SMTP_SERVER="your-smtp-server" \
     SMTP_PORT="587" \
     USERNAME="your-email" \
     PASSWORD="your-password" \
     FROM_ADDRESS="noreply@example.com" \
     FROM_NAME="CorrePalabras" \
     -n correpalabras

   # ConfigMap para CORS
   oc set env configmap/correpalabras-config \
     ALLOWED_ORIGINS="https://your-frontend-url.com" \
     -n correpalabras
   ```

### Paso 3: Configurar GitHub Actions Secrets

En tu repositorio de GitHub, ir a: **Settings → Secrets and variables → Actions**

Agregar los siguientes secrets:
- `OPENSHIFT_SERVER`: URL del servidor OpenShift (ej: https://api.openshift.example.com:6443)
- `OPENSHIFT_TOKEN`: Token de autenticación de OpenShift

### Paso 4: Actualizar el Workflow

En `.github/workflows/build-and-deploy.yml`:

Reemplazar la URL del repositorio:
```yaml
git:
  uri: https://github.com/YOUR_ORG/itlab-Correpalabras-Backend.git
```

### Paso 5: Hacer Push y Verificar Build

```bash
# Hacer push a main branch
git push origin main

# Verificar que el workflow se ejecutó
# En GitHub: Actions → Build and Deploy
```

### Paso 6: Verificar el Deployment

```bash
# Ver status del deployment
oc get deploymentconfig -n correpalabras
oc get pods -n correpalabras
oc logs dc/correpalabras-api -n correpalabras -f

# Ver la URL de acceso
oc get route correpalabras-api -n correpalabras
```

---

## Configuración de Variables de Entorno

Las siguientes variables se inyectarán automáticamente:

| Variable | Origen | Ejemplo |
|----------|--------|---------|
| `ASPNETCORE_ENVIRONMENT` | DeploymentConfig | `Production` |
| `CONNECTION_STRING` | Secret: correpalabras-db | PostgreSQL connection string |
| `JWT_KEY` | Secret: correpalabras-secrets | Secret key para JWT |
| `JWT_ISSUER` | Secret: correpalabras-secrets | Issuer del JWT |
| `JWT_AUDIENCE` | Secret: correpalabras-secrets | Audience del JWT |
| `ALLOWED_ORIGINS` | ConfigMap | URLs permitidas para CORS |
| `EMAIL_SMTP_SERVER` | Secret: correpalabras-email | mail.smtp2go.com |
| `EMAIL_USERNAME` | Secret: correpalabras-email | Tu usuario SMTP |
| `EMAIL_PASSWORD` | Secret: correpalabras-email | Tu contraseña SMTP |

---

## Health Checks

OpenShift verifica constantemente la salud de la aplicación:

```
GET /api/healthcheck
```

**Response exitosa:**
```json
{
  "success": true,
  "data": {
    "status": "ok"
  }
}
```

- **Liveness Probe**: Cada 10 segundos (después de 30s de delay)
- **Readiness Probe**: Cada 5 segundos (después de 10s de delay)
- **FailureThreshold**: 3 intentos fallidos = reinicio

---

## Autoscaling (Opcional)

Para agregar autoscaling horizontal:

```bash
oc autoscale dc/correpalabras-api --min=2 --max=5 --cpu-percent=80 -n correpalabras
```

---

## Troubleshooting

### El pod no inicia
```bash
oc describe pod <pod-name> -n correpalabras
oc logs <pod-name> -n correpalabras
```

### Error de conexión a base de datos
- Verificar que la connection string está correcta en el secret
- Verificar que PostgreSQL es accesible desde el pod

### Error de autenticación JWT
- Verificar que JWT_KEY, JWT_ISSUER y JWT_AUDIENCE están configurados

### CORS errors
- Verificar que ALLOWED_ORIGINS incluye tu frontend URL

---

## Documentación Adicional

- [OpenShift Docs](https://docs.openshift.com/)
- [GitHub Actions](https://docs.github.com/en/actions)
- [.NET en Containers](https://learn.microsoft.com/en-us/dotnet/core/docker/building-net-docker-images)
