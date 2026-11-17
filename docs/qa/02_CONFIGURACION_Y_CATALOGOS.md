# 2) Configuración inicial y catálogos

## 2.1 Dependencias reales detectadas
Orden recomendado antes de alta de bienes:
1. Usuarios/Roles/Permisos (`/Admin/Usuarios`, `/Admin/Roles`)
2. Áreas (`/Operaciones/Areas`)
3. Ubicaciones (`/Operaciones/Ubicaciones`)
4. Responsables (`/Operaciones/Responsables`)
5. Alta de bienes (`/Patrimonio/Create`)

> Observación: No existe módulo separado de “categorías”; la clasificación se realiza con `Tipo de Activo` (`assetType`).

## 2.2 Usuarios y roles (base para todo)

### PR-USR-001
**ID:** PR-USR-001  
**Módulo:** Administración > Usuarios  
**Nombre de la prueba:** Crear usuario administrador de prueba  
**Objetivo:** Validar alta de usuario y asignación de rol `Administrator`.  
**Precondiciones:** Sesión con usuario admin activo.  
**Usuario requerido:** Administrator.  
**Datos de prueba:**
- Nombre completo: `QA Admin Uno`
- Correo: `qa.admin1@sicpai.local`
- Contraseña temporal: `TempAdmin123!`
- Rol: `Administrator`
**Procedimiento paso a paso:**
1. Ir a `/Admin/Usuarios`.
2. Clic en **Nuevo Usuario**.
3. Capturar datos en `#userFullName`, `#userEmail`, `#userPassword`.
4. Seleccionar rol en `#userRoleId`.
5. Clic en **Guardar Usuario**.
6. Confirmar aparición en tabla `#usersRows`.
**Resultado esperado:** Usuario creado, estado Activo, rol visible.  
**Verificación de persistencia:** Revisar `USERS`, `USERROLES` y relación con `USERCATALOGS`.  
**Validaciones adicionales:** Repetir con email duplicado y esperar conflicto.  
**Criterio de aprobación:** Alta correcta + unicidad respetada.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-USR-002
**ID:** PR-USR-002  
**Módulo:** Administración > Usuarios  
**Nombre de la prueba:** Crear usuarios por perfil existente  
**Objetivo:** Crear usuarios para roles semilla reales.  
**Precondiciones:** Existen roles: Capturista, Supervisor, Auditor, Consultor.  
**Usuario requerido:** Administrator.  
**Datos de prueba:**
- `qa.capturista@sicpai.local` / `TempCap123!` / Rol Capturista
- `qa.supervisor@sicpai.local` / `TempSup123!` / Rol Supervisor
- `qa.auditor@sicpai.local` / `TempAud123!` / Rol Auditor
- `qa.consultor@sicpai.local` / `TempCon123!` / Rol Consultor
**Procedimiento paso a paso:**
1. Repetir flujo de PR-USR-001 para cada usuario.
2. Verificar KPIs de usuarios activos.
**Resultado esperado:** 4 usuarios creados con su rol.  
**Verificación de persistencia:** `USERS` + `USERROLES`.  
**Validaciones adicionales:** Campo vacío debe mostrar advertencia frontend y rechazo backend.  
**Criterio de aprobación:** Todos los perfiles reales creados sin error.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-USR-003
**ID:** PR-USR-003  
**Módulo:** Administración > Usuarios  
**Nombre de la prueba:** Activar/desactivar usuario  
**Objetivo:** Verificar control de estado y bloqueo de login para inactivos.  
**Precondiciones:** Usuario de prueba existente.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** `qa.consultor@sicpai.local`.  
**Procedimiento paso a paso:**
1. En `/Admin/Usuarios`, ubicar usuario en tabla.
2. Pulsar botón de energía (acción toggle).
3. Confirmar badge `Activo/Inactivo`.
4. Intentar login con usuario inactivo.
**Resultado esperado:** Estado cambia y login se rechaza si está inactivo.  
**Verificación de persistencia:** `USERS.ActiveFlag`.  
**Validaciones adicionales:** Volver a activar y confirmar login exitoso.  
**Criterio de aprobación:** Estado persistente y aplicado en autenticación.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 2.3 Roles y permisos

### PR-ROL-001
**ID:** PR-ROL-001  
**Módulo:** Administración > Roles y Permisos  
**Nombre de la prueba:** Consultar matriz de permisos por rol  
**Objetivo:** Verificar lectura de roles/permisos reales.  
**Precondiciones:** Sesión admin.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Ir a `/Admin/Roles`.
2. Verificar carga de roles desde `/api/admin/roles`.
3. Verificar permisos desde `/api/admin/permisos`.
4. Cambiar rol en `#rolesSelect` y revisar checks en `#rolesPermissionsRows`.
**Resultado esperado:** Matriz se actualiza por rol seleccionado.  
**Verificación de persistencia:** N/A (consulta).  
**Validaciones adicionales:** Sin roles debe mostrar estado vacío.  
**Criterio de aprobación:** Datos coherentes con semilla y asignaciones actuales.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-ROL-002
**ID:** PR-ROL-002  
**Módulo:** Administración > Roles y Permisos  
**Nombre de la prueba:** Crear rol personalizado de solo lectura  
**Objetivo:** Probar caso “solo lectura” cuando no existe de fábrica.  
**Precondiciones:** Sesión admin.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Nombre rol: `SoloLecturaQA`.  
**Procedimiento paso a paso:**
1. En `/Admin/Roles`, clic **Nuevo Rol**.
2. Capturar nombre `SoloLecturaQA`.
3. Guardar y seleccionar ese rol.
4. Asignar únicamente permiso `assets.read` (y opcional sin `reports.export`).
5. Guardar permisos con botón asociado (`#saveRolePermissions`).
**Resultado esperado:** Rol creado y permisos guardados.  
**Verificación de persistencia:** `USERCATALOGS`, `ROLEPERMISSIONS`.  
**Validaciones adicionales:** Crear rol duplicado debe fallar con conflicto.  
**Criterio de aprobación:** Rol usable para pruebas de restricción.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-ROL-003
**ID:** PR-ROL-003  
**Módulo:** Seguridad  
**Nombre de la prueba:** Matriz de permisos por perfil funcional  
**Objetivo:** Validar qué puede/no puede cada perfil real.  
**Precondiciones:** Usuarios de prueba creados.  
**Usuario requerido:** Cada perfil.  
**Datos de prueba:** Credenciales QA de cada rol.  
**Procedimiento paso a paso:**
1. Iniciar sesión con cada usuario.
2. Intentar:
   - `/api/assets` (lectura)
   - `POST /api/assets` (alta)
   - `PATCH /api/transferencias/{id}/aprobar`
   - `GET /api/bitacora`
   - `/api/reportes/inventario/excel`
3. Registrar respuesta (200/403/401).
**Resultado esperado:** Restricción consistente con permisos asignados.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** Probar acceso directo por URL y por menú.  
**Criterio de aprobación:** Backend bloquea operaciones no autorizadas.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 2.4 Catálogo de Áreas

### PR-CAT-AREA-001
**ID:** PR-CAT-AREA-001  
**Módulo:** Operaciones > Áreas y Departamentos  
**Nombre de la prueba:** Alta de área  
**Objetivo:** Crear área institucional válida.  
**Precondiciones:** Sesión autenticada.  
**Usuario requerido:** Administrator o rol con acceso operativo.  
**Datos de prueba:** Código `AREA-TI-QA`, Nombre `Tecnologías QA`, Descripción `Área para pruebas`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Areas`.
2. Clic en **Nueva Área Institucional**.
3. Capturar `#areaCode`, `#areaName`, `#areaDescription`, estado activo.
4. Guardar con `#saveAreaButton`.
5. Buscar en `#areaSearchInput`.
**Resultado esperado:** Área visible en tabla y estado Activa.  
**Verificación de persistencia:** Tabla `INSTITUTIONALAREAS`.  
**Validaciones adicionales:** Código duplicado debe fallar.  
**Criterio de aprobación:** Alta persistida y consultable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-CAT-AREA-002
**ID:** PR-CAT-AREA-002  
**Módulo:** Operaciones > Áreas y Departamentos  
**Nombre de la prueba:** Edición y desactivación de área  
**Objetivo:** Comprobar actualización de nombre/estado.  
**Precondiciones:** Área existente.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Cambiar nombre a `Tecnologías QA Editada`, estado Inactiva.  
**Procedimiento paso a paso:**
1. En tabla, clic ícono editar.
2. Modificar datos.
3. Guardar.
4. Filtrar por estado en `#areaStatusFilter`.
**Resultado esperado:** Cambios reflejados en listado y filtros.  
**Verificación de persistencia:** `INSTITUTIONALAREAS` actualizado.  
**Validaciones adicionales:** No permitir nombre/código vacíos.  
**Criterio de aprobación:** Edición consistente UI/BD.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 2.5 Catálogo de Ubicaciones

### PR-CAT-UBI-001
**ID:** PR-CAT-UBI-001  
**Módulo:** Operaciones > Ubicaciones físicas  
**Nombre de la prueba:** Alta de ubicación física  
**Objetivo:** Registrar sede/espacio usable por bienes y transferencias.  
**Precondiciones:** Sesión activa.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Código `UBI-QA-01`, Sede `Campus QA`, Espacio `Almacén QA`, Edificio `A`, Piso `1`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Ubicaciones`.
2. Clic `#addLocationButton`.
3. Capturar formulario emergente.
4. Guardar.
5. Verificar fila en `#locationsRows`.
**Resultado esperado:** Ubicación creada y activa.  
**Verificación de persistencia:** `PHYSICALLOCATIONS`.  
**Validaciones adicionales:** Campos obligatorios `Code/Site/Name`.  
**Criterio de aprobación:** Registro disponible para formularios de activos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 2.6 Catálogo de Responsables

### PR-CAT-RESP-001
**ID:** PR-CAT-RESP-001  
**Módulo:** Operaciones > Responsables  
**Nombre de la prueba:** Alta de responsable patrimonial  
**Objetivo:** Crear responsable para asignación de bienes.  
**Precondiciones:** Áreas cargadas.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Nombre `Responsable QA 1`, correo `responsable.qa1@sicpai.local`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Responsables`.
2. Abrir modal `#modalResponsable`.
3. Completar `#responsableNombre` y `#correo`.
4. Guardar con botón **Guardar**.
5. Confirmar en `#responsablesRows`.
**Resultado esperado:** Responsable creado; estado activo.  
**Verificación de persistencia:** Se almacena en `USERS` (modelo de responsable sobre usuarios).  
**Validaciones adicionales:** Correo duplicado debe fallar con conflicto.  
**Criterio de aprobación:** Responsable seleccionable en alta/transferencia de activos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-CAT-RESP-002
**ID:** PR-CAT-RESP-002  
**Módulo:** Operaciones > Responsables  
**Nombre de la prueba:** Editar responsable  
**Objetivo:** Actualizar nombre/correo.  
**Precondiciones:** Responsable existente.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Nombre nuevo `Responsable QA 1 Editado`.  
**Procedimiento paso a paso:**
1. En tabla, clic en editar.
2. Cambiar nombre/correo.
3. Guardar.
**Resultado esperado:** Fila actualizada.  
**Verificación de persistencia:** `USERS` actualizado.  
**Validaciones adicionales:** Correo inválido debe ser rechazado.  
**Criterio de aprobación:** Edición efectiva y consistente.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 2.7 Diferencias contra perfiles requeridos originales
- Implementados por semilla: **Administrator, Capturista, Supervisor, Auditor, Consultor**.
- No detectados como rol semilla independiente: **Responsable de área** y **Solo lectura**.
- Estrategia de prueba recomendada:
  - Responsable de área: usar usuario en catálogo de responsables + rol operativo definido por negocio.
  - Solo lectura: crear rol `SoloLecturaQA` y asignar únicamente permisos de lectura.
