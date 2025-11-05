# Cómo Enviar el Header X-Tenant

El header `X-Tenant` es **OBLIGATORIO** en todas las peticiones. Sin él, el sistema no puede determinar a qué empresa (tenant) pertenece la petición.

## Opciones para Enviar el Header

### 1. En Swagger (Swagger UI)

1. Abre Swagger en: `https://localhost:7004/swagger`
2. En la parte superior, busca el botón **"Authorize"** o **"🔓"**
3. Si no hay botón de autorización para headers, **debes agregarlo manualmente**:
   - En cada endpoint, expande los detalles
   - Busca la sección "Try it out"
   - Antes de hacer click en "Execute", desplázate hacia abajo
   - Verás una sección "Parameters" o "Headers"
   - Agrega un nuevo header:
     - **Key**: `X-Tenant`
     - **Value**: `siscore`

**Nota:** Si Swagger no permite agregar headers personalizados fácilmente, usa Postman o curl.

### 2. En Postman

1. Abre la colección "TimeControl API"
2. Click derecho en la colección → **"Edit"**
3. Ve a la pestaña **"Variables"**
4. Asegúrate de que la variable `tenant` tenga el valor `siscore`
5. Todos los requests automáticamente incluirán el header `X-Tenant: siscore`

**O manualmente en cada request:**
- En la pestaña "Headers"
- Agrega:
  - **Key**: `X-Tenant`
  - **Value**: `siscore`

### 3. En cURL (Terminal)

```bash
curl -X GET "https://localhost:7004/api/diagnostics/tenant" \
  -H "X-Tenant: siscore" \
  -k
```

**Nota:** El flag `-k` es necesario para aceptar certificados autofirmados en desarrollo.

### 4. En Código (C#)

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Tenant", "siscore");
var response = await client.GetAsync("https://localhost:7004/api/diagnostics/tenant");
```

### 5. En JavaScript/Fetch

```javascript
fetch('https://localhost:7004/api/diagnostics/tenant', {
  headers: {
    'X-Tenant': 'siscore'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

## Verificación Rápida

Ejecuta este comando en tu terminal:

```bash
curl -X GET "https://localhost:7004/api/diagnostics/tenant" \
  -H "X-Tenant: siscore" \
  -k
```

Deberías ver un resultado similar a:

```json
{
  "headerName": "X-Tenant",
  "tenantHeaderValue": "siscore",
  "host": "localhost",
  "defaultDomain": "localhost",
  "tenantContext": {
    "companyId": 1,
    "subdomain": "siscore",
    "hasConnectionString": true
  },
  "companies": [...],
  "message": "Tenant resuelto correctamente"
}
```

## Problema Común en Swagger

Swagger a veces no permite agregar headers personalizados fácilmente. Si es tu caso:

1. **Usa Postman** (recomendado)
2. **O modifica temporalmente el código** para permitir un query parameter como alternativa
3. **O usa curl** desde la terminal

## Solución Alternativa: Agregar Query Parameter

Si prefieres usar query parameters en lugar de headers, puedo modificar el código para que también acepte `?tenant=siscore` como alternativa.

