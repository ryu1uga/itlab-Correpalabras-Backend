== INSTRUCCIONES PARA DESPLEGAR EN OPENSHIFT CON GITHUB WEBHOOK ==

El proyecto está configurado para buildear automáticamente en OpenShift cuando hagas push a GitHub.
Sigue estos pasos para completar la configuración:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 1: CREAR EL SECRET CON VARIABLES DE ENTORNO EN OPENSHIFT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Ejecuta el siguiente comando en tu CLI de OpenShift (reemplaza los valores):

oc create secret generic app-secrets \
  --from-literal=db-user=tu_usuario_postgres \
  --from-literal=db-password=tu_password_postgres \
  --from-literal=app-url=http://correpalabras-backend-git-dev-itlab.apps.example.com \
  --from-literal=jwt-key=tu_llave_jwt_super_secreta_123 \
  --from-literal=cloudinary-cloud-name=tu_cloud_name \
  --from-literal=cloudinary-api-key=tu_api_key \
  --from-literal=cloudinary-api-secret=tu_api_secret \
  -n dev-itlab

NOTA: Asegúrate de:
  - Reemplazar todos los valores con tus datos reales
  - El namespace sea "dev-itlab"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 2: APLICAR LOS YAML EN OPENSHIFT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Desde la raíz del proyecto, ejecuta:

oc apply -f openshift/buildconfig.yml
oc apply -f openshift/service.yml
oc apply -f openshift/deploymentconfig.yml
oc apply -f openshift/route.yml

Verifica que se crearon correctamente:

oc get bc -n dev-itlab
oc get svc -n dev-itlab
oc get dc -n dev-itlab
oc get routes -n dev-itlab

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 3: OBTENER LA URL DEL WEBHOOK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Ejecuta este comando para obtener la URL del webhook:

oc describe bc correpalabras-backend-git -n dev-itlab | grep -A5 "Webhook URL"

Copiarás una URL como esta:
https://openshift-master.example.com/apis/build.openshift.io/v1/namespaces/dev-itlab/buildconfigs/correpalabras-backend-git/webhooks/correpalabras-webhook-secret/github

NOTA IMPORTANTE: El "secret" es: correpalabras-webhook-secret
(Es el que configuramos en buildconfig.yml)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 4: REGISTRAR WEBHOOK EN GITHUB
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. Ve al repositorio: https://github.com/ryu1uga/itlab-Correpalabras-Backend

2. Haz clic en: Settings → Webhooks → Add webhook

3. Completa los campos:

   ✓ Payload URL: 
     https://openshift-master.example.com/apis/build.openshift.io/v1/namespaces/dev-itlab/buildconfigs/correpalabras-backend-git/webhooks/correpalabras-webhook-secret/github

   ✓ Content type: application/json

   ✓ Which events would you like to trigger this webhook?
     → Selecciona: "Push events" (o "Just the push event")

   ✓ Active: ✓ (marcado)

4. Haz clic en "Add webhook"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 5: VERIFICAR QUE TODO FUNCIONA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Haz un push a la rama "main" del repositorio:

git add .
git commit -m "Configurar OpenShift"
git push origin main

Luego monitorea el build en OpenShift:

oc logs -f bc/correpalabras-backend-git -n dev-itlab

O en la UI de OpenShift:
Builds → correpalabras-backend-git → Ver últimos builds

El build debería pasar por las fases:
1. Source (clonando del GitHub)
2. Building (compilando con .NET SDK)
3. Push (empujando imagen al registry)
4. Deployment (desplegando pods)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PASO 6: ACCEDER A LA APLICACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Una vez que el deployment esté listo, obtén la URL:

oc get routes -n dev-itlab

Verás algo como:
NAME                          HOST/PORT                                         PATH   SERVICES                      PORT   TERMINATION
correpalabras-backend-git     correpalabras-backend-git-dev-itlab.apps...      http   correpalabras-backend-git    8080   edge

Accede a: https://correpalabras-backend-git-dev-itlab.apps.example.com/swagger

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

TROUBLESHOOTING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ El build falla: Ver logs
   oc logs -f bc/correpalabras-backend-git -n dev-itlab

❌ Los pods no inician: Ver eventos del deployment
   oc describe dc correpalabras-backend-git -n dev-itlab

❌ Las variables de entorno no funcionan:
   - Verifica que el Secret existe: oc get secrets -n dev-itlab
   - Verifica los datos: oc describe secret app-secrets -n dev-itlab

❌ El webhook no triguer builds:
   - En GitHub, ve a Settings → Webhooks
   - Verifica que la URL sea correcta
   - Mira los "Recent Deliveries" para ver si hay errores

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ARCHIVOS MODIFICADOS/CREADOS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✓ Dockerfile                          - Modificado (removido --environment=Development)
✓ openshift/buildconfig.yml           - CREADO (build automático desde GitHub)
✓ openshift/deploymentconfig.yml      - CREADO (configuración del deployment)
✓ openshift/service.yml               - CREADO (servicio interno)
✓ openshift/route.yml                 - CREADO (exposición pública con TLS)
✓ OPENSHIFT_DEPLOYMENT.md             - CREADO (este archivo)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
