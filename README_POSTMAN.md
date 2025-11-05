# Colección de Postman - TimeControl API

## Instalación

1. Abre Postman
2. Click en "Import" (botón en la esquina superior izquierda)
3. Selecciona el archivo `TimeControlApi.postman_collection.json`
4. La colección se importará con todas las requests preconfiguradas

## Configuración de Variables

La colección incluye las siguientes variables que debes configurar:

### Variables de Colección

- **baseUrl**: URL base de la API (por defecto: `https://localhost:5001`)
- **tenant**: Subdominio de la empresa (por defecto: `empresa1`)
- **accessToken**: Se guarda automáticamente después del login
- **refreshToken**: Se guarda automáticamente después del login

### Cómo configurar las variables:

1. Click derecho en la colección "TimeControl API"
2. Selecciona "Edit"
3. Ve a la pestaña "Variables"
4. Actualiza los valores según tu entorno:

```
baseUrl: http://localhost:5000 (o tu URL)
tenant: siscore (subdominio de SisCore)
```

## Endpoints Disponibles

### Auth (Autenticación)

#### 1. Login
- **POST** `/api/auth/login`
- **Descripción**: Iniciar sesión con email/password o teléfono/password
- **Headers**: 
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Body**:
```json
{
  "email": "usuario@ejemplo.com",
  "password": "password123",
  "deviceId": "device-123",
  "deviceName": "Mi Dispositivo"
}
```
- **Nota**: El accessToken y refreshToken se guardan automáticamente después de un login exitoso

#### 2. Refresh Token
- **POST** `/api/auth/refresh`
- **Descripción**: Refrescar token de acceso
- **Headers**: 
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Body**:
```json
{
  "refreshToken": "{{refreshToken}}",
  "deviceId": "device-123"
}
```

#### 3. Logout
- **POST** `/api/auth/logout`
- **Descripción**: Cerrar sesión
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Body**:
```json
{
  "refreshToken": "{{refreshToken}}"
}
```

#### 4. Get Current User
- **GET** `/api/auth/me`
- **Descripción**: Obtener información del usuario actual
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa

#### 5. Validate Token
- **GET** `/api/auth/validate`
- **Descripción**: Validar token de acceso
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa

### Users (Usuarios)

#### 1. Register User
- **POST** `/api/users`
- **Descripción**: Registrar nuevo usuario
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Body**:
```json
{
  "email": "nuevo@ejemplo.com",
  "password": "password123",
  "fullName": "Nuevo Usuario",
  "employeeNumber": "EMP001",
  "roleIds": [1]
}
```

#### 2. Get All Users
- **GET** `/api/users`
- **Descripción**: Obtener todos los usuarios
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa

#### 3. Get User by ID
- **GET** `/api/users/:id`
- **Descripción**: Obtener usuario por ID
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
- **Path Variables**:
  - `id`: ID del usuario

#### 4. Update User
- **PUT** `/api/users/:id`
- **Descripción**: Actualizar usuario
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Path Variables**:
  - `id`: ID del usuario
- **Body**:
```json
{
  "email": "actualizado@ejemplo.com",
  "fullName": "Usuario Actualizado",
  "employeeNumber": "EMP002"
}
```

#### 5. Delete User
- **DELETE** `/api/users/:id`
- **Descripción**: Eliminar usuario (soft delete)
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
- **Path Variables**:
  - `id`: ID del usuario

#### 6. Assign Roles to User
- **POST** `/api/users/:id/roles`
- **Descripción**: Asignar roles a usuario
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Path Variables**:
  - `id`: ID del usuario
- **Body**:
```json
{
  "roleIds": [1, 2]
}
```

### Roles (Roles)

#### 1. Get All Roles
- **GET** `/api/roles`
- **Descripción**: Obtener todos los roles
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa

#### 2. Get Role by ID
- **GET** `/api/roles/:id`
- **Descripción**: Obtener rol por ID
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
- **Path Variables**:
  - `id`: ID del rol

#### 3. Create Role
- **POST** `/api/roles`
- **Descripción**: Crear nuevo rol
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Body**:
```json
{
  "name": "Nuevo Rol",
  "description": "Descripción del nuevo rol",
  "permissionIds": [1, 2, 3]
}
```

#### 4. Update Role
- **PUT** `/api/roles/:id`
- **Descripción**: Actualizar rol
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
  - `Content-Type`: application/json
- **Path Variables**:
  - `id`: ID del rol
- **Body**:
```json
{
  "name": "Rol Actualizado",
  "description": "Nueva descripción",
  "permissionIds": [1, 2, 3, 4]
}
```

#### 5. Delete Role
- **DELETE** `/api/roles/:id`
- **Descripción**: Eliminar rol (soft delete)
- **Headers**: 
  - `Authorization`: Bearer {{accessToken}}
  - `X-Tenant`: Subdominio de la empresa
- **Path Variables**:
  - `id`: ID del rol

## Flujo de Uso Recomendado

1. **Configurar variables**: Actualiza `baseUrl` y `tenant` según tu entorno
2. **Login**: Ejecuta el endpoint "Login" para obtener los tokens
   - Los tokens se guardan automáticamente en las variables de colección
3. **Usar endpoints protegidos**: Todos los endpoints excepto Login y Refresh Token requieren el token en el header `Authorization`
4. **Refresh Token**: Si el access token expira, usa el endpoint "Refresh Token"
5. **Logout**: Cuando termines, usa el endpoint "Logout" para cerrar la sesión

## Notas Importantes

- **Multi-tenancy**: Todos los endpoints requieren el header `X-Tenant` con el subdominio de la empresa
- **Autenticación**: Los tokens JWT tienen una vida útil de 20 minutos (configurable)
- **Refresh Tokens**: Los refresh tokens tienen una vida útil de 14 días (configurable)
- **Soft Delete**: Los endpoints de eliminación realizan un "soft delete" (marcan el registro como inactivo, no lo eliminan físicamente)

## Solución de Problemas

### Error 401 Unauthorized
- Verifica que el token en la variable `accessToken` sea válido
- Asegúrate de que el header `Authorization` esté configurado correctamente
- Usa el endpoint "Refresh Token" para obtener un nuevo access token

### Error 404 Not Found
- Verifica que el `baseUrl` sea correcto
- Asegúrate de que el endpoint existe y está bien escrito

### Error de Tenant
- Verifica que el header `X-Tenant` esté configurado correctamente
- Asegúrate de que la empresa existe en la base de datos maestra

