# 5) Matriz de permisos, escenarios integrales, checklist e incidencias

## 5.1 Matriz de permisos funcional (basada en permisos reales)

Permisos técnicos detectados:
- `assets.read`
- `assets.create`
- `assets.update`
- `assets.delete`
- `audits.manage`
- `transfers.approve`
- `users.manage`
- `reports.export`

### Matriz sugerida para ejecución QA (confirmar contra configuración actual en /Admin/Roles)
| Perfil | assets.read | assets.create | assets.update | assets.delete | audits.manage | transfers.approve | users.manage | reports.export |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Administrator | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Capturista | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Supervisor | ✅ | ✅ | ✅ | ❌ | ❌/✅* | ✅ | ❌ | ✅* |
| Auditor | ✅ | ❌/✅* | ✅* | ❌ | ✅ | ❌ | ❌ | ✅* |
| Consultor | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌/✅* |
| SoloLecturaQA (si se crea) | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

\* Depende de cómo quede configurado en la matriz real de `/Admin/Roles`.

## 5.2 Pruebas de permisos por usuario (plantilla ejecutable)

### PR-PERM-001
**ID:** PR-PERM-001  
**Módulo:** Seguridad por perfil  
**Nombre de la prueba:** Validación de menú y endpoints por rol  
**Objetivo:** Comprobar que frontend y backend aplican restricciones coherentes.  
**Precondiciones:** Usuarios de prueba activos por rol.  
**Usuario requerido:** Cada perfil.  
**Datos de prueba:** listado de rutas protegidas.  
**Procedimiento paso a paso:**
1. Iniciar sesión.
2. Registrar opciones visibles del menú lateral.
3. Ejecutar operación autorizada (esperar éxito).
4. Intentar operación no autorizada (esperar denegación).
5. Intentar URL/API directa protegida.
6. Cerrar sesión y probar acceso con botón atrás del navegador.
**Resultado esperado:** No hay bypass por URL ni por caché de navegador.  
**Verificación de persistencia:** solo operaciones autorizadas deben generar cambios en BD.  
**Validaciones adicionales:** validar código HTTP (401/403).  
**Criterio de aprobación:** seguridad consistente en frontend/backend.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 5.3 Escenarios integrales de principio a fin (A–G)

### ESC-A
**ID:** ESC-A  
**Módulo:** Flujo integral  
**Nombre de la prueba:** Alta y asignación inicial de un bien  
**Objetivo:** Recorrer catálogos + alta + detalle + persistencia.  
**Precondiciones:** Catálogos creados (área/ubicación/responsable).  
**Usuario requerido:** Administrator/Capturista autorizado.  
**Datos de prueba:** `EQ-QA-0100` y adjuntos PDF/JPG.  
**Procedimiento paso a paso:**
1. Login admin.
2. Verificar áreas (`/Operaciones/Areas`).
3. Verificar ubicaciones (`/Operaciones/Ubicaciones`).
4. Verificar responsables (`/Operaciones/Responsables`).
5. Alta de bien (`/Patrimonio/Create`).
6. Asignar área/responsable/ubicación.
7. Adjuntar documentos.
8. Guardar.
9. Abrir detalle y validar datos.
10. Confirmar persistencia en `ASSETS` y `ASSETATTACHMENTS`.
**Resultado esperado:** activo completo y trazable.  
**Verificación de persistencia:** SQL Server en tablas clave.  
**Validaciones adicionales:** intentar guardado sin campos obligatorios.  
**Criterio de aprobación:** flujo completo sin errores.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-B
**ID:** ESC-B  
**Módulo:** Seguridad  
**Nombre de la prueba:** Gestión por diferentes perfiles  
**Objetivo:** Verificar acceso por rol y restricción backend.  
**Precondiciones:** Usuarios de prueba creados por rol.  
**Usuario requerido:** admin + cada rol evaluado.  
**Datos de prueba:** endpoints críticos de activos/reportes/bitácora.  
**Procedimiento paso a paso:** crear roles/usuarios, iniciar con cada uno, ejecutar autorizadas/no autorizadas, comprobar 403 en backend.  
**Resultado esperado:** cumplimiento de matriz de permisos.  
**Verificación de persistencia:** solo operaciones válidas impactan BD.  
**Validaciones adicionales:** intentar acceso directo a `/api/bitacora` sin permiso.  
**Criterio de aprobación:** sin escalación de privilegios.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-C
**ID:** ESC-C  
**Módulo:** Transferencias  
**Nombre de la prueba:** Transferencia de un bien  
**Objetivo:** Validar ciclo solicitud-aprobación-actualización-historial.  
**Precondiciones:** Bien existente y usuario aprobador.  
**Usuario requerido:** Solicitante + aprobador.  
**Datos de prueba:** activo `EQ-QA-0001`.  
**Procedimiento paso a paso:** crear solicitud, aprobar, revisar detalle, revisar historial movimientos y bitácora.  
**Resultado esperado:** asignación actualizada y movimiento registrado.  
**Verificación de persistencia:** `ASSETTRANSFERS`, `ASSETS`, `ASSETMOVEMENTS`, `ACTIVITYLOGS`.  
**Validaciones adicionales:** rechazo con motivo.  
**Criterio de aprobación:** trazabilidad total.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-D
**ID:** ESC-D  
**Módulo:** Mantenimiento/Estado  
**Nombre de la prueba:** Mantenimiento y cambio de estado  
**Objetivo:** Validar mantenimiento + actualización de estado del activo.  
**Precondiciones:** Bien activo con permiso de actualización.  
**Usuario requerido:** Supervisor/Admin.  
**Datos de prueba:** mantenimiento preventivo + estado `En reparación`.  
**Procedimiento paso a paso:** registrar mantenimiento en detalle, cambiar estado, consultar historial.  
**Resultado esperado:** movimientos reflejados y estado visible en listados/reportes.  
**Verificación de persistencia:** `ASSETMOVEMENTS` + `ASSETS.Status`.  
**Validaciones adicionales:** costo/fecha inválidos.  
**Criterio de aprobación:** datos coherentes entre módulos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-E
**ID:** ESC-E  
**Módulo:** Auditorías  
**Nombre de la prueba:** Auditoría patrimonial completa  
**Objetivo:** Programar, ejecutar y cerrar auditoría con diferencias.  
**Precondiciones:** Usuario con `audits.manage`, bienes existentes.  
**Usuario requerido:** Auditor/Admin.  
**Datos de prueba:** auditoría `Auditoría QA S1`.  
**Procedimiento paso a paso:** crear auditoría, agregar bienes por código, marcar encontrados/faltantes/dañados, completar auditoría, revisar resultados y reportes.  
**Resultado esperado:** resultados persistidos y consultables.  
**Verificación de persistencia:** `AUDITRECORDS`, `AUDITASSETRESULTS`.  
**Validaciones adicionales:** estado inválido debe fallar.  
**Criterio de aprobación:** diferencias registradas correctamente.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-F
**ID:** ESC-F  
**Módulo:** Reportes  
**Nombre de la prueba:** Reportes institucionales  
**Objetivo:** Validar inventario general, por filtros y consolidado financiero.  
**Precondiciones:** Datos suficientes y permiso `reports.export`.  
**Usuario requerido:** Rol autorizado.  
**Datos de prueba:** filtros por área/responsable/estado.  
**Procedimiento paso a paso:** generar Excel/PDF, comparar con BD, ejecutar consolidado financiero.  
**Resultado esperado:** archivos correctos, sin discrepancias relevantes.  
**Verificación de persistencia:** N/A (lectura/export).  
**Validaciones adicionales:** usuario sin permiso obtiene 403.  
**Criterio de aprobación:** exactitud documental.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### ESC-G
**ID:** ESC-G  
**Módulo:** Baja definitiva  
**Nombre de la prueba:** Ciclo de baja de activo  
**Objetivo:** Verificar retiro con motivo y efecto en listados/reportes.  
**Precondiciones:** Activo candidato a baja y permiso `assets.delete`.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** motivo `Baja QA por obsolescencia`.  
**Procedimiento paso a paso:** ejecutar baja por API, validar estado final, revisar historial y reportes.  
**Resultado esperado:** activo retirado según regla implementada.  
**Verificación de persistencia:** `ASSETS` + `ASSETMOVEMENTS`.  
**Validaciones adicionales:** motivo vacío debe fallar.  
**Criterio de aprobación:** baja controlada y auditable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 5.4 Casos negativos transversales mínimos
- Login con usuario inexistente.
- Login con password incorrecta.
- Login con usuario inactivo.
- Intento de crear registros con requeridos vacíos.
- Códigos duplicados (áreas, ubicaciones, activos).
- Importación de archivo inválido/no `.xlsx`.
- Adjuntos con tipo o tamaño no permitido.
- Acceso a API protegida sin permiso.
- Cambios de estado con valores no permitidos (auditoría/inventario).

## 5.5 Checklist general de validación

### Checklist técnico-funcional
- [ ] Conexión SQL Server verificada en ambiente QA.
- [ ] Migraciones aplicadas sin error.
- [ ] Seed inicial validado (roles/permisos/admin).
- [ ] Login/logout y seguridad de sesión validados.
- [ ] Gestión de usuarios/roles/permisos validada.
- [ ] Catálogos base (áreas/ubicaciones/responsables) validados.
- [ ] Alta/consulta/edición de activos validada.
- [ ] Adjuntos y descarga de documentos validados.
- [ ] Transferencias (crear/aprobar/rechazar) validadas.
- [ ] Mantenimientos y estados validados.
- [ ] Inventarios físicos validados.
- [ ] Auditorías validadas.
- [ ] Reportes Excel/PDF validados.
- [ ] Bitácora y trazabilidad validadas.
- [ ] Configuración de depreciación validada.

## 5.6 Registro de incidencias (plantilla)
| ID Incidencia | Caso de prueba relacionado | Fecha/Hora | Usuario | Severidad | Descripción | Evidencia | Estado | Responsable | Observaciones |
|---|---|---|---|---|---|---|---|---|---|
| INC-001 | PR-___ | ____ | ____ | Alta/Media/Baja | ____ | screenshot/log/SQL | Abierta/En análisis/Resuelta | ____ | ____ |

## 5.7 Resumen de resultados (plantilla)
| Módulo | Total casos | Aprobadas | Fallidas | Bloqueadas | % aprobación |
|---|---:|---:|---:|---:|---:|
| Preparación y seguridad |  |  |  |  |  |
| Usuarios y permisos |  |  |  |  |  |
| Catálogos |  |  |  |  |  |
| Bienes y operación |  |  |  |  |  |
| Inventarios y auditorías |  |  |  |  |  |
| Reportes y bitácora |  |  |  |  |  |
| Configuración |  |  |  |  |  |
| **Total general** |  |  |  |  |  |

## 5.8 Criterio de cierre de validación
Se recomienda cerrar la validación solo cuando:
1. No existan casos críticos/altos en estado Fallida.
2. Los escenarios integrales A–G estén ejecutados al 100%.
3. La bitácora y persistencia SQL confirmen la trazabilidad de operaciones clave.
4. Se emita reporte final con evidencias y plan de remediación para pendientes.
