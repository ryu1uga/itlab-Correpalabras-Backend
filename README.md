# Guía de Despliegue: .NET Core + PostgreSQL en OpenShift

Esta guía detalla el proceso para preparar una aplicación .NET Web API, desplegar una instancia de base de datos PostgreSQL mediante un manifiesto YAML en OpenShift, e inyectar las variables de entorno de manera segura sin dejar rastros en el código fuente.

---

## 1. Limpieza y Configuración del Proyecto .NET

Para cumplir con las prácticas de desarrollo nativas de la nube, la aplicación debe confiar exclusivamente en el proveedor de configuración del entorno del contenedor.

### appsettings.json limpio

El archivo de configuración no debe contener credenciales ni la cadena de conexión real. Debe quedar estructurado de la siguiente manera:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}

```

### Remoción de cargas locales en Program.cs

Asegúrate de **no incluir** llamadas a librerías como DotNetEnv o similares que intenten forzar la lectura de archivos .env en producción. .NET Core lee automáticamente las variables de entorno del sistema a través de builder.Configuration.

Tu código para registrar el contexto de la base de datos debe mapear la variable de entorno del sistema de forma limpia:

```csharp
var builder = WebApplication.CreateBuilder(args);

// El proveedor por defecto busca automáticamente la variable de entorno ConnectionStrings__DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

```

---

## 2. Despliegue de la Base de Datos en OpenShift

Utilizaremos el siguiente manifiesto consolidado para levantar el almacenamiento persistente, el secreto de credenciales, el servicio interno y el deployment de PostgreSQL 16 basado en la imagen de Bitnami.

Aplica este contenido usando la CLI de OpenShift (oc apply -f postgres-deployment.yaml) o desde la consola web en el namespace dev-itlab:

```yaml
# =====================
# Secret
# =====================
kind: Secret
apiVersion: v1
metadata:
  name: correpalabras
  namespace: dev-itlab
type: Opaque
stringData:
  POSTGRESQL_USERNAME: correpalabras
  POSTGRESQL_PASSWORD: correpalabras
  POSTGRESQL_DATABASE: correpalabras
---
# =====================
# PersistentVolumeClaim
# =====================
kind: PersistentVolumeClaim
apiVersion: v1
metadata:
  name: correpalabras
  namespace: dev-itlab
spec:
  storageClassName: ocs-storagecluster-ceph-rbd
  accessModes:
    - ReadWriteOnce
  volumeMode: Filesystem
  resources:
    requests:
      storage: 1Gi
---
# =====================
# Service
# =====================
kind: Service
apiVersion: v1
metadata:
  name: correpalabras
  namespace: dev-itlab
  labels:
    app: correpalabras
spec:
  selector:
    app: correpalabras
  ports:
    - name: postgresql
      protocol: TCP
      port: 5432
      targetPort: 5432
  type: ClusterIP
---
# =====================
# Deployment
# =====================
kind: Deployment
apiVersion: apps/v1
metadata:
  name: correpalabras
  namespace: dev-itlab
  labels:
    app: correpalabras
    app.kubernetes.io/part-of: mindful-git-app
spec:
  replicas: 1
  selector:
    matchLabels:
      app: correpalabras
  strategy:
    type: Recreate
  revisionHistoryLimit: 10
  progressDeadlineSeconds: 600
  template:
    metadata:
      labels:
        app: correpalabras
    spec:
      restartPolicy: Always
      terminationGracePeriodSeconds: 30
      dnsPolicy: ClusterFirst
      schedulerName: default-scheduler
      securityContext: {}
      volumes:
        - name: correpalabras
          persistentVolumeClaim:
            claimName: correpalabras
      containers:
        - name: correpalabras
          image: 'public.ecr.aws/bitnami/postgresql:16'
          imagePullPolicy: IfNotPresent
          ports:
            - containerPort: 5432
              protocol: TCP
          resources:
            limits:
              cpu: 500m
              memory: 512Mi
            requests:
              cpu: 250m
              memory: 256Mi
          envFrom:
            - secretRef:
                name: correpalabras
          volumeMounts:
            - name: correpalabras
              mountPath: /bitnami/postgresql
          securityContext:
            capabilities:
              drop:
                - ALL
            runAsNonRoot: true
            allowPrivilegeEscalation: false
          readinessProbe:
            exec:
              command:
                - pg_isready
                - '-U'
                - correpalabras
                - '-d'
                - correpalabras
            initialDelaySeconds: 15
            timeoutSeconds: 1
            periodSeconds: 10
            successThreshold: 1
            failureThreshold: 5
          livenessProbe:
            exec:
              command:
                - pg_isready
                - '-U'
                - correpalabras
                - '-d'
                - correpalabras
            initialDelaySeconds: 30
            timeoutSeconds: 1
            periodSeconds: 20
            successThreshold: 1
            failureThreshold: 3
          terminationMessagePath: /dev/termination-log
          terminationMessagePolicy: File

```

---

## 3. Migración Manual de Datos desde la Terminal del Pod

Dado que las migraciones se ejecutarán manualmente a través de la terminal de OpenShift, sigue estos pasos una vez que el pod de la base de datos se encuentre en estado *Running*:

### Paso 1: Obtener el nombre del Pod de la Base de Datos

```bash
oc get pods -n dev-itlab -l app=correpalabras

```

### Paso 2: Abrir una sesión interactiva en el Pod

```bash
oc rsh -n dev-itlab pod/nombredelpod-xxxxx

```

### Paso 3: Ejecutar la migración o restauración de datos

Una vez dentro del contenedor, puedes interactuar directamente con la base de datos usando psql.

Si necesitas restaurar un archivo SQL previamente copiado al pod (usando oc cp), ejecuta:

```bash
psql -U correpalabras -d correpalabras -f /tmp/tu_archivo_migracion.sql

```

O para crear estructuras puntuales de forma interactiva:

```bash
psql -U correpalabras -d correpalabras

```

---

## 4. Inyección de Variables de Entorno en el Deployment de la Aplicación .NET

Para conectar tu API con la base de datos recién creada, debes configurar las variables de entorno dentro del Deployment correspondiente de la aplicación .NET en OpenShift.

> El proveedor de configuración de .NET convierte los guiones bajos dobles (__) en jerarquías de configuración (equivalente a los dos puntos : en local).

La cadena de conexión apuntará al servicio interno de OpenShift (correpalabras.dev-itlab.svc.cluster.local o simplemente correpalabras si están en el mismo namespace).

Añade la sección env en el contenedor de tu API dentro de su respectivo archivo de despliegue:

```yaml
spec:
  containers:
    - name: api-dotnet
      image: tu-registro/api-dotnet:latest
      env:
        - name: ConnectionStrings__DefaultConnection
          value: "Host=correpalabras;Port=5432;Database=correpalabras;Username=correpalabras;Password=correpalabras;"
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"

```

Alternativamente, puedes añadirla directamente desde la consola CLI de OpenShift sin modificar el archivo YAML del API corriendo:

```bash
oc set env deployment/tu-app-dotnet ConnectionStrings__DefaultConnection="Host=correpalabras;Port=5432;Database=correpalabras;Username=correpalabras;Password=correpalabras;" -n dev-itlab

```