# OpenShift Readiness Checklist

## ✅ Problemas Identificados y Corregidos

### CRÍTICOS
- [x] **Dockerfile - Environment hardcodeado a Development**
  - **Archivo**: Dockerfile
  - **Cambio**: Removido `--environment=Development`, ahora respeta variable de entorno
  - **Impacto**: La aplicación ahora puede ejecutarse en Production/Development según configuración

- [x] **Credenciales sensibles en código**
  - **Archivo**: appsettings.json
  - **Cambio**: Removidas contraseñas SMTP y connection string
  - **Impacto**: Las credenciales se inyectarán via Secrets de OpenShift

- [x] **Falta .dockerignore**
  - **Archivo**: .dockerignore (CREADO)
  - **Cambio**: Agregado para optimizar tamaño de imagen
  - **Impacto**: Builds más rápidos, imágenes más pequeñas

### IMPORTANTES
- [x] **Health Check no registrado**
  - **Estado**: Ya existe `HealthCheckController`, verificado que funciona
  - **Cambio**: Documentado en DeploymentConfig para OpenShift
  - **Impacto**: OpenShift puede monitorear la salud de la aplicación

- [x] **Swagger solo en Development**
  - **Archivo**: Program.cs
  - **Estado**: Funcionamiento correcto (solo en Development)
  - **Impacto**: En Production no expone documentación innecesaria

- [x] **Archivo de configuración Production faltante**
  - **Archivo**: appsettings.Production.json (CREADO)
  - **Cambio**: Agregado para override de configuración en Production
  - **Impacto**: Mejor manejo de configuración por ambiente

### RECOMENDACIONES
- [x] **Falta configuración de OpenShift**
  - **Archivos CREADOS**:
    - openshift/imagestream.yml
    - openshift/buildconfig.yml
    - openshift/deploymentconfig.yml
    - openshift/service.yml
    - openshift/route.yml
    - openshift/secrets-config.yml
  - **Impacto**: Deployment automatizado en OpenShift

- [x] **Falta CI/CD desde GitHub**
  - **Archivo CREADO**: .github/workflows/build-and-deploy.yml
  - **Funcionalidad**:
    - Build automático en push a main
    - Push a GitHub Container Registry
    - Deployment automático en OpenShift
  - **Impacto**: Deployment continuo sin intervención manual

---

## 📋 Estructura de Archivos Creados

```
proyecto/
├── .dockerignore (NUEVO)
├── .github/
│   └── workflows/
│       └── build-and-deploy.yml (NUEVO)
├── appsettings.Production.json (NUEVO)
├── appsettings.json (MODIFICADO - sin credenciales)
├── Dockerfile (MODIFICADO - environment mejorado)
├── OPENSHIFT_DEPLOYMENT.md (NUEVO)
├── OPENSHIFT_READY_CHECKLIST.md (Este archivo)
├── openshift/ (NUEVA CARPETA)
│   ├── imagestream.yml
│   ├── buildconfig.yml
│   ├── deploymentconfig.yml
│   ├── service.yml
│   ├── route.yml
│   └── secrets-config.yml
```

---

## 🚀 Próximos Pasos

### 1. Verificación Local
```bash
# Probar que el Dockerfile buildea correctamente
docker build -t correpalabras-api:test .

# Probar que la app arranca
docker run -e CONNECTION_STRING="..." -e JWT_KEY="..." -p 8080:8080 correpalabras-api:test
```

### 2. Configurar GitHub Secrets
Ir a: `Settings → Secrets and variables → Actions`

Agregar:
- `OPENSHIFT_SERVER`: Tu URL de OpenShift
- `OPENSHIFT_TOKEN`: Tu token

### 3. Actualizar BuildConfig en openshift/buildconfig.yml
Reemplazar:
```yaml
git:
  uri: https://github.com/YOUR_ORG/itlab-Correpalabras-Backend.git
```

### 4. Crear Namespace en OpenShift
```bash
oc create namespace correpalabras
oc project correpalabras
```

### 5. Aplicar Secrets y ConfigMap
```bash
oc apply -f openshift/secrets-config.yml
# Luego editar con valores reales
```

### 6. Hacer Push al Main Branch
El workflow se ejecutará automáticamente y deployará en OpenShift.

---

## 📊 Comparación Antes vs Después

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Dockerfile** | Environment hardcodeado | Dynamic con env variables |
| **Credenciales** | En el código (INSEGURO) | En Secrets de OpenShift |
| **Docker Optimization** | Sin .dockerignore | Con .dockerignore |
| **Configuración Production** | Solo appsettings.json | + appsettings.Production.json |
| **OpenShift Config** | No existe | 6 archivos YAML completos |
| **CI/CD** | Manual | Automatizado con GitHub Actions |
| **Health Checks** | Existe endpoint | Configurado en DeploymentConfig |
| **Deployment** | Local con docker-compose | OpenShift con 3 replicas |

---

## ⚠️ Consideraciones Importantes

1. **Base de Datos PostgreSQL**
   - Debe estar accesible desde el pod de OpenShift
   - Connection string debe ser correcta

2. **SMTP Configuration**
   - Las credenciales SMTP deben estar en Secrets
   - No debe haber credenciales en el código

3. **JWT Secrets**
   - Usar valores seguros y únicos
   - Guardar en secreto (no en Git)

4. **Replicas y Recursos**
   - 3 replicas para HA
   - Limites: 1 CPU, 512Mi RAM
   - Requests: 250m CPU, 256Mi RAM

5. **Security Context**
   - No root user
   - Read-only filesystem
   - Capabilities limitadas

---

## 📞 Soporte

Para más información sobre OpenShift:
- [Red Hat OpenShift Documentation](https://docs.openshift.com/)
- [OpenShift Training](https://learn.openshift.com/)

Para problemas con .NET:
- [Microsoft .NET Documentation](https://learn.microsoft.com/dotnet/)
