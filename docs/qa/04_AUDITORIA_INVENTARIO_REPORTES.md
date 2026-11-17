# 4) Auditorías, inventarios, reportes y bitácora

## 4.1 Inventarios físicos

### PR-INV-001
**ID:** PR-INV-001  
**Módulo:** Operaciones > Inventarios físicos  
**Nombre de la prueba:** Crear sesión de inventario  
**Objetivo:** Iniciar conteo institucional.  
**Precondiciones:** Usuario con permiso `audits.manage`.  
**Usuario requerido:** Auditor / Administrator.  
**Datos de prueba:** Nombre `Inventario QA 2026`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Inventarios`.
2. Clic `#newInventory`.
3. Capturar nombre y confirmar.
4. Verificar fila en `#inventoryRows`.
**Resultado esperado:** Sesión creada en estado abierto.  
**Verificación de persistencia:** `INVENTORYSESSIONS`.  
**Validaciones adicionales:** nombre vacío debe rechazarse.  
**Criterio de aprobación:** sesión disponible para conteo.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-INV-002
**ID:** PR-INV-002  
**Módulo:** Operaciones > Inventarios físicos  
**Nombre de la prueba:** Registrar conteo por bien  
**Objetivo:** Registrar resultados: Pendiente/Encontrado/Faltante/Dañado.  
**Precondiciones:** Sesión abierta.  
**Usuario requerido:** Auditor / Administrator.  
**Datos de prueba:** actualizar 3 bienes con resultados distintos.  
**Procedimiento paso a paso:**
1. Abrir sesión de inventario.
2. En `#countRows`, seleccionar resultado y observaciones por activo.
3. Guardar cada conteo.
**Resultado esperado:** cada conteo se persiste correctamente.  
**Verificación de persistencia:** `INVENTORYCOUNTS`.  
**Validaciones adicionales:** valor no permitido debe retornar error.  
**Criterio de aprobación:** resultados visibles tras recarga.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-INV-003
**ID:** PR-INV-003  
**Módulo:** Operaciones > Inventarios físicos  
**Nombre de la prueba:** Cerrar inventario  
**Objetivo:** Consolidar sesión y aplicar conciliación.  
**Precondiciones:** Sesión con conteos registrados.  
**Usuario requerido:** Con `audits.manage`.  
**Datos de prueba:** sesión activa.  
**Procedimiento paso a paso:**
1. Clic `#closeInventory`.
2. Confirmar cierre.
3. Verificar estado final de la sesión.
**Resultado esperado:** inventario cerrado y conciliación aplicada por servicio.  
**Verificación de persistencia:** `INVENTORYSESSIONS.Status`; revisar impactos en `ASSETS` según reglas internas.  
**Validaciones adicionales:** cierre de sesión inexistente debe fallar controladamente.  
**Criterio de aprobación:** cierre consistente y auditable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 4.2 Auditorías patrimoniales

### PR-AUD-001
**ID:** PR-AUD-001  
**Módulo:** Operaciones > Auditorías  
**Nombre de la prueba:** Programar auditoría  
**Objetivo:** Crear auditoría con fecha y descripción.  
**Precondiciones:** Usuario con `audits.manage`.  
**Usuario requerido:** Auditor / Administrator.  
**Datos de prueba:** Nombre `Auditoría QA S1`, fecha próxima, descripción corta.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Auditorias`.
2. Clic `#newAudit`.
3. Capturar nombre, fecha, descripción.
4. Confirmar programación.
**Resultado esperado:** auditoría en estado `Programada`.  
**Verificación de persistencia:** `AUDITRECORDS`.  
**Validaciones adicionales:** nombre/fecha obligatorios.  
**Criterio de aprobación:** aparece en tabla `#auditRows`.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-AUD-002
**ID:** PR-AUD-002  
**Módulo:** Operaciones > Auditorías  
**Nombre de la prueba:** Agregar bienes y verificar resultados  
**Objetivo:** Levantar evidencia de encontrados/faltantes/dañados.  
**Precondiciones:** Auditoría programada seleccionada.  
**Usuario requerido:** Auditor / Administrator.  
**Datos de prueba:** códigos de bienes QA existentes.  
**Procedimiento paso a paso:**
1. Abrir auditoría.
2. Agregar bien por código (`#auditAssetCode` + `#addAuditAsset`).
3. En tabla de bienes auditados, seleccionar resultado por cada activo.
4. Capturar notas.
**Resultado esperado:** resultados guardados por activo.  
**Verificación de persistencia:** `AUDITASSETRESULTS`.  
**Validaciones adicionales:** código inexistente debe mostrar advertencia.  
**Criterio de aprobación:** resultados consistentes y recuperables.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-AUD-003
**ID:** PR-AUD-003  
**Módulo:** Operaciones > Auditorías  
**Nombre de la prueba:** Cambiar estado de auditoría  
**Objetivo:** Probar estados Programada/En proceso/Completada/Cancelada.  
**Precondiciones:** Auditoría existente.  
**Usuario requerido:** Con `audits.manage`.  
**Datos de prueba:** transición Programada -> En proceso -> Completada.  
**Procedimiento paso a paso:**
1. Abrir auditoría.
2. Pulsar `#startAudit` para En proceso.
3. Pulsar `#completeAudit` para Completada.
4. Verificar badge en listado filtrando por `#auditStatusFilter`.
**Resultado esperado:** cambios de estado persistidos y visibles.  
**Verificación de persistencia:** `AUDITRECORDS.Status`.  
**Validaciones adicionales:** estado no permitido debe ser rechazado.  
**Criterio de aprobación:** ciclo de estados correcto.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 4.3 Reportes y exportaciones

### PR-REP-001
**ID:** PR-REP-001  
**Módulo:** Reportes > Inventario  
**Nombre de la prueba:** Exportar inventario Excel/PDF  
**Objetivo:** Validar generación de reportes principales con permisos.  
**Precondiciones:** Usuario con `reports.export`.  
**Usuario requerido:** Administrator o rol autorizado.  
**Datos de prueba:** filtros opcionales `status`, `area`, `responsible`.  
**Procedimiento paso a paso:**
1. Ir a `/Reportes/Index`.
2. Clic **Exportar Excel** (`/api/reportes/inventario/excel`).
3. Clic **Exportar PDF** (`/api/reportes/inventario/pdf`).
4. Repetir con parámetros filtrados desde enlaces de la vista.
**Resultado esperado:** descarga de archivos sin error y contenido coherente.  
**Verificación de persistencia:** N/A (generación).  
**Validaciones adicionales:** usuario sin permiso debe recibir 403.  
**Criterio de aprobación:** formatos descargables y datos correctos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-REP-002
**ID:** PR-REP-002  
**Módulo:** Reportes > Financiero  
**Nombre de la prueba:** Exportación consolidada financiera  
**Objetivo:** Validar descargas simultáneas de inventario + movimientos.  
**Precondiciones:** Usuario autorizado y datos en movimientos.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** N/A.  
**Procedimiento paso a paso:**
1. Ir a `/Reportes/Financiero`.
2. Clic en `#exportFinancialConsolidated`.
3. Confirmar descarga en diálogo.
4. Verificar apertura/descarga de:
   - `/api/reportes/inventario/excel`
   - `/api/reportes/movimientos/excel`
**Resultado esperado:** dos archivos descargados correctamente.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** bloquear por permiso faltante.  
**Criterio de aprobación:** export consolidado funcional.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 4.4 Bitácora y auditoría de actividad

### PR-BIT-001
**ID:** PR-BIT-001  
**Módulo:** Administración > Bitácora  
**Nombre de la prueba:** Consulta filtrada de bitácora  
**Objetivo:** Comprobar registro y consulta de actividades relevantes.  
**Precondiciones:** Ejecutar previamente operaciones (login, alta, edición, transferencias).  
**Usuario requerido:** Rol con `users.manage` (admin).  
**Datos de prueba:** Acción `Login`, entidad `User`, rango de fechas actual.  
**Procedimiento paso a paso:**
1. Ir a `/Admin/Bitacora`.
2. Capturar filtros `#logAction`, `#logEntity`, `#logFrom`, `#logTo`.
3. Clic `#searchLogs`.
4. Revisar `#logRows` y total `#logTotal`.
**Resultado esperado:** eventos listados con fecha, usuario, acción, módulo, IP, descripción.  
**Verificación de persistencia:** `ACTIVITYLOGS`.  
**Validaciones adicionales:** usuario sin permiso debe fallar al consumir `/api/bitacora`.  
**Criterio de aprobación:** trazabilidad completa y filtrable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 4.5 Configuración general

### PR-CFG-001
**ID:** PR-CFG-001  
**Módulo:** Administración > Configuración  
**Nombre de la prueba:** Actualizar tasas de depreciación  
**Objetivo:** Validar lectura y escritura de parámetros financieros persistidos.  
**Precondiciones:** Sesión admin con token antiforgery válido.  
**Usuario requerido:** Administrator.  
**Datos de prueba:**
- `Computadora: 0.30`
- `Impresora: 0.20`
- `Mobiliario: 0.10`
**Procedimiento paso a paso:**
1. Ir a `/Admin/Configuracion`.
2. Esperar carga de tasas (`/api/admin/finanzas/depreciacion`).
3. Editar valores entre 0 y 1.
4. Clic `#saveDepreciationRatesButton`.
5. Abrir `/Reportes/Financiero` y confirmar impacto visual en métricas estimadas.
**Resultado esperado:** tasas guardadas y reutilizadas por panel financiero.  
**Verificación de persistencia:** tabla `FINANCIALSETTINGS` (`Key`, `ValueDecimal`).  
**Validaciones adicionales:** valor fuera de rango debe rechazarse.  
**Criterio de aprobación:** configuración persistente tras recarga de app.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 4.6 Funcionalidades parciales / pendientes detectadas
- No hay módulo dedicado de “departamentos” independiente de Áreas; se maneja estructura por áreas.
- No existe una pantalla dedicada para “gestión de categorías” separada de tipo de activo.
- Flujo de exportación “bitácora” desde historial de movimientos aparece como botón visual; validar si el botón está enlazado en versión actual antes de aprobar ese caso.
