# 1) Preparación del ambiente y seguridad

## 1.1 Preparación del ambiente de pruebas (SQL Server + app)

### PR-ENV-001
**ID:** PR-ENV-001  
**Módulo:** Ambiente  
**Nombre de la prueba:** Verificar cadena de conexión activa  
**Objetivo:** Confirmar a qué base apunta la app antes de probar.  
**Precondiciones:** Repositorio disponible.  
**Usuario requerido:** N/A.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Abrir `SICPAI/appsettings.json`.
2. Revisar `ConnectionStrings:DefaultConnection`.
3. Confirmar servidor y nombre de base.
4. Si aplica, revisar overrides de `appsettings.Development.json` o variables de entorno.
**Resultado esperado:** Cadena identificada y documentada para ambiente QA.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** Confirmar que no apunta a productivo.  
**Criterio de aprobación:** Cadena validada y registrada.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-ENV-002
**ID:** PR-ENV-002  
**Módulo:** Ambiente  
**Nombre de la prueba:** Verificar creación/aplicación de migraciones al iniciar  
**Objetivo:** Confirmar que `DataSeeder` aplica `context.Database.Migrate()` en arranque.  
**Precondiciones:** App compilable.  
**Usuario requerido:** N/A.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Abrir `Infrastructure/Seed/DataSeeder.cs`.
2. Verificar llamada a `context.Database.Migrate()`.
3. Verificar baseline de `__EFMigrationsHistory` para bases antiguas.
4. Iniciar aplicación y revisar que no falle en fase de seed.
**Resultado esperado:** Migraciones aplicadas sin error.  
**Verificación de persistencia:** Consultar `__EFMigrationsHistory` (solo lectura).  
**Validaciones adicionales:** Registrar mensajes de error si fallan permisos SQL.  
**Criterio de aprobación:** App inicia y base queda en estado migrado.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-ENV-003
**ID:** PR-ENV-003  
**Módulo:** Ambiente  
**Nombre de la prueba:** Verificar datos semilla (roles/permisos/admin)  
**Objetivo:** Confirmar inicialización mínima para operar pruebas.  
**Precondiciones:** Base vacía o recién creada.  
**Usuario requerido:** N/A.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Revisar `DataSeeder.cs`.
2. Verificar roles sembrados: `Administrator`, `Capturista`, `Supervisor`, `Auditor`, `Consultor`.
3. Verificar permisos sembrados: `assets.read`, `assets.create`, `assets.update`, `assets.delete`, `audits.manage`, `transfers.approve`, `users.manage`, `reports.export`.
4. Verificar usuario admin semilla en ambiente de prueba.
**Resultado esperado:** Roles/permisos creados y admin existente en QA.  
**Verificación de persistencia:** Consultar tablas `ROLECATALOGS`, `PERMISSIONCATALOGS`, `USERS`, `USERROLES`, `ROLEPERMISSIONS`.  
**Validaciones adicionales:** Confirmar que el rol Administrator tenga permisos completos.  
**Criterio de aprobación:** Datos base completos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

> Nota de credenciales detectadas en código (solo QA): en `DataSeeder.cs` se crea `admin@sicpai.local` con contraseña inicial `Admin123!` si no existen usuarios.

## 1.2 Login y autenticación

### PR-AUTH-001
**ID:** PR-AUTH-001  
**Módulo:** Login  
**Nombre de la prueba:** Inicio de sesión válido  
**Objetivo:** Validar autenticación cookie y redirección a dashboard.  
**Precondiciones:** Usuario activo existente.  
**Usuario requerido:** Administrador.  
**Datos de prueba:** Email y contraseña válidos.  
**Procedimiento paso a paso:**
1. Ir a `/Auth/Login`.
2. Capturar correo/contraseña válidos.
3. Enviar formulario.
4. Confirmar redirección a `/Home/Dashboard`.
**Resultado esperado:** Sesión iniciada y menú habilitado según rol/permisos.  
**Verificación de persistencia:** En bitácora debe registrarse “Login exitoso”.  
**Validaciones adicionales:** Revisar cookie auth emitida.  
**Criterio de aprobación:** Login exitoso + claims cargados.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-AUTH-002
**ID:** PR-AUTH-002  
**Módulo:** Login  
**Nombre de la prueba:** Contraseña incorrecta  
**Objetivo:** Verificar rechazo de credenciales inválidas.  
**Precondiciones:** Usuario existente.  
**Usuario requerido:** N/A.  
**Datos de prueba:** Email válido + password inválido.  
**Procedimiento paso a paso:**
1. Ir a `/Auth/Login`.
2. Capturar credenciales inválidas.
3. Enviar.
**Resultado esperado:** Mensaje de error; no crea sesión.  
**Verificación de persistencia:** Bitácora debe registrar “Login fallido”.  
**Validaciones adicionales:** Repetir 5 veces para validar rate limit/login lockout.  
**Criterio de aprobación:** Rechazo consistente.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-AUTH-003
**ID:** PR-AUTH-003  
**Módulo:** Login  
**Nombre de la prueba:** Usuario desactivado  
**Objetivo:** Verificar que `ActiveFlag=false` no pueda iniciar sesión.  
**Precondiciones:** Usuario desactivado.  
**Usuario requerido:** N/A.  
**Datos de prueba:** Credenciales correctas de usuario inactivo.  
**Procedimiento paso a paso:**
1. Desactivar usuario desde Admin/Usuarios.
2. Intentar login con ese usuario.
**Resultado esperado:** Login rechazado.  
**Verificación de persistencia:** Registro de intento en bitácora.  
**Validaciones adicionales:** Confirmar que backend retorna error.  
**Criterio de aprobación:** No permite autenticación de inactivos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-AUTH-004
**ID:** PR-AUTH-004  
**Módulo:** Sesión  
**Nombre de la prueba:** Cierre de sesión y acceso posterior  
**Objetivo:** Verificar invalidación de cookie y protección de rutas.  
**Precondiciones:** Sesión activa.  
**Usuario requerido:** Cualquiera autenticado.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Ejecutar logout.
2. Intentar abrir `/Home/Dashboard` directamente.
3. Presionar atrás del navegador.
**Resultado esperado:** Redirección a login y sin acceso a contenido protegido.  
**Verificación de persistencia:** Bitácora registra “Logout”.  
**Validaciones adicionales:** Revisar expiración por inactividad.  
**Criterio de aprobación:** Sesión no reutilizable tras logout.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 1.3 Seguridad por permisos

### PR-SEC-001
**ID:** PR-SEC-001  
**Módulo:** Autorización  
**Nombre de la prueba:** Validar acceso backend por permiso  
**Objetivo:** Comprobar que no basta ocultar botones en frontend.  
**Precondiciones:** Dos usuarios: admin y no-admin sin permiso.  
**Usuario requerido:** No-admin.  
**Datos de prueba:** URL/API protegida, ej. `GET /api/bitacora`.  
**Procedimiento paso a paso:**
1. Iniciar sesión con usuario sin `users.manage`.
2. Consumir `GET /api/bitacora`.
3. Repetir con admin.
**Resultado esperado:** Sin permiso => 403/denegado; admin => 200.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** Probar también `/api/reportes/*` sin `reports.export`.  
**Criterio de aprobación:** Restricción efectiva en backend.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____
