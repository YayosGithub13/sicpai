# 3) Gestión de bienes y operación diaria

## 3.1 Registro de bienes patrimoniales

### PR-BIEN-REG-001
**ID:** PR-BIEN-REG-001  
**Módulo:** Patrimonio > Alta de nuevo bien  
**Nombre de la prueba:** Alta de bien tipo Computadora  
**Objetivo:** Verificar alta completa de activo con datos operativos.  
**Precondiciones:** Existen área, ubicación y responsable activos.  
**Usuario requerido:** Rol con `assets.create` (ej. Administrator/Capturista con permiso).  
**Datos de prueba:**
- Código patrimonial: `EQ-QA-0001`
- Código de barras: `CB-QA-0001`
- Nombre: `Laptop QA Dell`
- Tipo: `Computadora`
- Estado: `Activo`
- Marca/Modelo: `Dell / Latitude 5420`
- Serie: `SN-QA-0001`
- Área: `Tecnologías QA`
- Responsable: `Responsable QA 1`
- Ubicación: `Campus QA - Almacén QA`
- Valor: `18999.90`
- Fecha adquisición: fecha actual
**Procedimiento paso a paso:**
1. Ir a `/Patrimonio/Create`.
2. En pestaña General, capturar `assetCode`, `assetName`, `assetType`, `assetStatus`, `assetBrand`, `assetModel`, `assetSerialNumber`.
3. En Ubicación, seleccionar `assetArea`, `assetResponsible`, `assetLocation`.
4. En Financiero, capturar `assetValue` y `assetAcquisitionDate`.
5. Clic en **Guardar Bien** (`#saveAssetButton`).
6. Confirmar redirección a `/Patrimonio/Detail?id={id}`.
**Resultado esperado:** Bien creado y visible en detalle con código y estado.  
**Verificación de persistencia:** `ASSETS` con datos capturados; `ASSETMOVEMENTS` con movimiento de alta si aplica por servicio.  
**Validaciones adicionales:** omitir código/nombre/tipo debe bloquear guardado.  
**Criterio de aprobación:** Alta completa y persistida.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-REG-002
**ID:** PR-BIEN-REG-002  
**Módulo:** Patrimonio > Alta de nuevo bien  
**Nombre de la prueba:** Alta de bien tipo Impresora  
**Objetivo:** Validar otro tipo de activo y valores monetarios.  
**Precondiciones:** Igual a PR-BIEN-REG-001.  
**Usuario requerido:** Con `assets.create`.  
**Datos de prueba:** Código `EQ-QA-0002`, Tipo `Impresora`, Valor `7999.50`, Serie `SN-QA-IMP-01`.  
**Procedimiento paso a paso:** repetir flujo PR-BIEN-REG-001 adaptando datos.  
**Resultado esperado:** Registro exitoso.  
**Verificación de persistencia:** `ASSETS.AssetType='Impresora'`.  
**Validaciones adicionales:** Valor negativo debe rechazarse.  
**Criterio de aprobación:** Bien consultable y exportable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-REG-003
**ID:** PR-BIEN-REG-003  
**Módulo:** Patrimonio > Alta de nuevo bien  
**Nombre de la prueba:** Carga de documentos/fotografías  
**Objetivo:** Verificar adjuntos permitidos y asociación al activo.  
**Precondiciones:** Bien ya creado o flujo de alta en curso.  
**Usuario requerido:** Con `assets.create`/`assets.update`.  
**Datos de prueba:**
- `factura-qa.pdf` (<5MB)
- `foto-qa.jpg` (<5MB)
**Procedimiento paso a paso:**
1. En `/Patrimonio/Create`, pestaña Documentos.
2. Seleccionar archivos con `#documentInput` o botón `#btnSelect`.
3. Guardar bien.
4. Verificar en detalle del activo descarga de adjuntos vía endpoint de download.
**Resultado esperado:** Archivos aceptados (pdf/jpg/png/jpeg) y asociados.  
**Verificación de persistencia:** `ASSETATTACHMENTS` + archivo físico en `wwwroot/uploads`.  
**Validaciones adicionales:**
- Extensión no permitida => error.
- >5MB => error.
**Criterio de aprobación:** Descarga funcional por `/api/assets/{assetId}/attachments/{attachmentId}/download`.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.2 Validaciones de registro

### PR-BIEN-VAL-001
**ID:** PR-BIEN-VAL-001  
**Módulo:** Activos API  
**Nombre de la prueba:** Código patrimonial duplicado  
**Objetivo:** Garantizar unicidad por `AssetCode`.  
**Precondiciones:** Existe `EQ-QA-0001`.  
**Usuario requerido:** Con `assets.create`.  
**Datos de prueba:** nuevo registro con mismo `assetCode`.  
**Procedimiento paso a paso:** Intentar alta con código repetido.  
**Resultado esperado:** Error de conflicto (409) con mensaje de duplicado.  
**Verificación de persistencia:** No debe crear nueva fila en `ASSETS`.  
**Validaciones adicionales:** repetir para importación masiva.  
**Criterio de aprobación:** Sin duplicados.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-VAL-002
**ID:** PR-BIEN-VAL-002  
**Módulo:** Importación de activos  
**Nombre de la prueba:** Vista previa e importación Excel con filas inválidas  
**Objetivo:** Validar rechazo por requeridos/duplicados/valores inválidos.  
**Precondiciones:** Archivo xlsx de prueba preparado.  
**Usuario requerido:** Con `assets.create`.  
**Datos de prueba:** archivo con filas válidas e inválidas.  
**Procedimiento paso a paso:**
1. Ir a `/Patrimonio/Index`.
2. Clic `Importar Excel` (`#importAssetsButton`).
3. Subir archivo en `#importAssetsFile`.
4. Revisar preview por `/api/assets/import/preview`.
5. Confirmar importación `/api/assets/import`.
**Resultado esperado:** Se importan válidas y se reportan rechazadas con razón.  
**Verificación de persistencia:** Conteo en `ASSETS` coincide con `imported`; rechazadas no persisten.  
**Validaciones adicionales:** archivo no `.xlsx` debe rechazarse.  
**Criterio de aprobación:** Integridad del lote y reporte de errores correcto.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.3 Consulta, filtros y administración

### PR-BIEN-LST-001
**ID:** PR-BIEN-LST-001  
**Módulo:** Patrimonio > Gestión de bienes  
**Nombre de la prueba:** Listado general y filtros  
**Objetivo:** Validar búsqueda por texto/área/estado y exportaciones filtradas.  
**Precondiciones:** Varios activos cargados.  
**Usuario requerido:** Con `assets.read`.  
**Datos de prueba:** código `EQ-QA-0001`; estado `Activo`; área `Tecnologías QA`.  
**Procedimiento paso a paso:**
1. Ir a `/Patrimonio/Index`.
2. Usar `#assetSearch`, `#assetAreaFilter`, `#assetStatusFilter`.
3. Ejecutar búsqueda (Enter o recarga automática).
4. Abrir detalle con acción de fila.
5. Probar `Exportar Excel/PDF` (`#exportAssetsExcel`, `#exportAssetsPdf`).
**Resultado esperado:** Listado responde a filtros; export respeta query.  
**Verificación de persistencia:** N/A (consulta).  
**Validaciones adicionales:** limpiar filtros con botón de limpieza.  
**Criterio de aprobación:** Resultado consistente entre UI y datos almacenados.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-DET-001
**ID:** PR-BIEN-DET-001  
**Módulo:** Patrimonio > Detalle de bien  
**Nombre de la prueba:** Visualización integral de detalle  
**Objetivo:** Validar que se muestren datos generales, ubicación, KPI y trazabilidad.  
**Precondiciones:** Bien existente con movimientos.  
**Usuario requerido:** Con `assets.read`.  
**Datos de prueba:** activo `EQ-QA-0001`.  
**Procedimiento paso a paso:**
1. Abrir `/Patrimonio/Detail?id={id}`.
2. Verificar `detailAssetName`, `detailAssetStatus`, `detailAssetCode`, `detailAssetSerial`.
3. Revisar tabs: Resumen, Ubicación, General, Mantenimiento, Transferencias.
4. Confirmar KPI de mantenimientos/transferencias/documentos.
5. Probar botón imprimir y navegación editar/transferir.
**Resultado esperado:** Datos completos y consistentes con activo real.  
**Verificación de persistencia:** N/A (consulta).  
**Validaciones adicionales:** activo inexistente debe manejarse con error/control de navegación.  
**Criterio de aprobación:** Detalle usable como expediente.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.4 Mantenimiento y estados de ciclo de vida

### PR-BIEN-MNT-001
**ID:** PR-BIEN-MNT-001  
**Módulo:** Detalle de bien > Mantenimiento  
**Nombre de la prueba:** Registrar mantenimiento  
**Objetivo:** Confirmar alta de mantenimiento y movimiento asociado.  
**Precondiciones:** Bien existente; permiso `assets.update`.  
**Usuario requerido:** Administrator/Supervisor con permiso.  
**Datos de prueba:** Tipo `Preventivo`, técnico `Proveedor QA`, fecha actual, costo `1200.00`, descripción libre.  
**Procedimiento paso a paso:**
1. En detalle, tab Mantenimiento.
2. Capturar campos de mantenimiento.
3. Clic `#detailMaintenanceSubmit`.
4. Verificar éxito y recarga de historial.
**Resultado esperado:** Mantenimiento registrado y visible en historial.  
**Verificación de persistencia:** `ASSETMOVEMENTS` con `MovementType='Mantenimiento'`.  
**Validaciones adicionales:** campos incompletos deben alertar en UI.  
**Criterio de aprobación:** historial actualizado y fecha próxima derivada mostrada.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-EST-001
**ID:** PR-BIEN-EST-001  
**Módulo:** Estado del bien  
**Nombre de la prueba:** Cambio de estado (Activo, En reparación, Dañado, Extraviado, Baja definitiva)  
**Objetivo:** Verificar que estados soportados se reflejen en listados y detalle.  
**Precondiciones:** Permiso `assets.update`.  
**Usuario requerido:** Administrator/Supervisor autorizado.  
**Datos de prueba:** actualizar activo QA por cada estado.  
**Procedimiento paso a paso:**
1. Ejecutar edición del activo (vía flujo de edición disponible en detalle/listado).
2. Cambiar `status` al valor objetivo.
3. Guardar cambios.
4. Validar filtro por estado en `/Patrimonio/Index`.
**Resultado esperado:** Estado actualizado en API/UI.  
**Verificación de persistencia:** `ASSETS.Status`.  
**Validaciones adicionales:** revisar restricciones de baja según negocio (si existen en servicio).  
**Criterio de aprobación:** estado consistente en detalle, listados y reportes.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-BIEN-BAJA-001
**ID:** PR-BIEN-BAJA-001  
**Módulo:** Activos API  
**Nombre de la prueba:** Baja definitiva de bien  
**Objetivo:** Validar retiro por endpoint con motivo obligatorio.  
**Precondiciones:** Bien activo; permiso `assets.delete`.  
**Usuario requerido:** Administrator.  
**Datos de prueba:** Motivo `Baja por obsolescencia QA`.  
**Procedimiento paso a paso:**
1. Ejecutar llamada `DELETE /api/assets/{id}` con body `{ reason }`.
2. Consultar detalle/listado del activo.
**Resultado esperado:** respuesta 204 y estado lógico de baja aplicado según servicio.  
**Verificación de persistencia:** `ASSETS.Status` y/o movimiento de baja en `ASSETMOVEMENTS`.  
**Validaciones adicionales:** motivo vacío debe devolver error.  
**Criterio de aprobación:** activo refleja baja y trazabilidad correspondiente.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.5 Escaneo y código de barras

### PR-ESC-001
**ID:** PR-ESC-001  
**Módulo:** Operaciones > Escaneo rápido  
**Nombre de la prueba:** Consulta manual por código  
**Objetivo:** Identificar activo por código patrimonial o barras.  
**Precondiciones:** Activo existente con código conocido.  
**Usuario requerido:** Con `assets.read`.  
**Datos de prueba:** `EQ-QA-0001`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Escaneo`.
2. Capturar código en input principal.
3. Clic en `Escanear` (`#scanButton`).
4. Verificar resultado presentado.
**Resultado esperado:** Retorna activo encontrado desde `/api/assets/scan/{code}`.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** código inexistente debe mostrar aviso de no encontrado.  
**Criterio de aprobación:** consulta rápida funcional.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-ESC-002
**ID:** PR-ESC-002  
**Módulo:** Operaciones > Escaneo rápido  
**Nombre de la prueba:** Escaneo con cámara (si navegador soporta BarcodeDetector)  
**Objetivo:** Validar flujo opcional de lectura física.  
**Precondiciones:** Dispositivo con cámara y soporte `BarcodeDetector`.  
**Usuario requerido:** Con `assets.read`.  
**Datos de prueba:** código impreso legible.  
**Procedimiento paso a paso:**
1. Clic botón cámara (`#cameraButton`).
2. Autorizar acceso a cámara.
3. Mostrar código al lector.
4. Verificar autocompletado y consulta.
**Resultado esperado:** si hay soporte, detecta y consulta; si no, muestra mensaje para captura manual.  
**Verificación de persistencia:** N/A.  
**Validaciones adicionales:** negar permiso de cámara => fallback manual.  
**Criterio de aprobación:** comportamiento controlado en ambos casos.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.6 Transferencias internas

### PR-TRF-001
**ID:** PR-TRF-001  
**Módulo:** Operaciones > Transferencias  
**Nombre de la prueba:** Crear solicitud de transferencia  
**Objetivo:** Registrar movimiento de área/responsable/ubicación.  
**Precondiciones:** Bien existente, catálogos cargados.  
**Usuario requerido:** Con `assets.update`.  
**Datos de prueba:** activo `EQ-QA-0001`, área destino `Administración QA`, responsable destino `Responsable QA 2`, ubicación destino `Campus QA - Oficina 2`, motivo `Reasignación operativa`.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Transferencias`.
2. Consultar activo en `#transferAssetCode` + botón `#loadTransferAsset`.
3. Confirmar datos actuales mostrados.
4. Capturar destino en `#transferArea`, `#transferResponsible`, `#transferLocation`.
5. Capturar observaciones en `#transferReason`.
6. Clic `#submitTransfer`.
7. Validar nueva fila en tabla de solicitudes.
**Resultado esperado:** solicitud creada con estado Pendiente.  
**Verificación de persistencia:** `ASSETTRANSFERS` con `Status='Pendiente'`.  
**Validaciones adicionales:** área destino y motivo son obligatorios.  
**Criterio de aprobación:** solicitud visible y trazable.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-TRF-002
**ID:** PR-TRF-002  
**Módulo:** Operaciones > Transferencias  
**Nombre de la prueba:** Aprobar transferencia  
**Objetivo:** Aplicar cambio de asignación al activo.  
**Precondiciones:** Existe transferencia Pendiente; usuario con `transfers.approve`.  
**Usuario requerido:** Supervisor/Administrator autorizado.  
**Datos de prueba:** ID de transferencia pendiente.  
**Procedimiento paso a paso:**
1. En tabla, localizar transferencia pendiente.
2. Clic botón **Aprobar**.
3. Confirmar mensaje de éxito.
4. Abrir detalle del activo transferido.
**Resultado esperado:** estado Aprobada y activo actualizado con nueva área/responsable/ubicación.  
**Verificación de persistencia:** `ASSETTRANSFERS` + `ASSETS` + movimiento en `ASSETMOVEMENTS`.  
**Validaciones adicionales:** aprobar ya procesada debe devolver error controlado.  
**Criterio de aprobación:** actualización integral aplicada.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

### PR-TRF-003
**ID:** PR-TRF-003  
**Módulo:** Operaciones > Transferencias  
**Nombre de la prueba:** Rechazar transferencia  
**Objetivo:** Registrar rechazo con motivo.  
**Precondiciones:** Solicitud pendiente.  
**Usuario requerido:** Con `transfers.approve`.  
**Datos de prueba:** Motivo `No procede por validación QA`.  
**Procedimiento paso a paso:**
1. Clic **Rechazar** en solicitud pendiente.
2. Capturar motivo en diálogo.
3. Confirmar.
**Resultado esperado:** transferencia pasa a Rechazada sin cambiar asignación del activo.  
**Verificación de persistencia:** `ASSETTRANSFERS.Status='Rechazada'`.  
**Validaciones adicionales:** probar rechazo sin motivo (aceptado como opcional según endpoint).  
**Criterio de aprobación:** rechazo trazable y sin efecto colateral no deseado.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____

## 3.7 Historial de movimientos

### PR-HIST-001
**ID:** PR-HIST-001  
**Módulo:** Operaciones > Historial de movimientos  
**Nombre de la prueba:** Consulta y filtros de movimientos  
**Objetivo:** Validar trazabilidad de altas/transferencias/mantenimientos/bajas.  
**Precondiciones:** Hay movimientos registrados.  
**Usuario requerido:** Con `assets.read`.  
**Datos de prueba:** filtro por tipo `Transferencia`, fechas recientes.  
**Procedimiento paso a paso:**
1. Ir a `/Operaciones/Historial`.
2. Usar `#movementSearch`, `#movementType`, `#movementFrom`, `#movementTo`.
3. Ejecutar búsqueda con `#searchMovements`.
4. Revisar total `#movementTotal` y filas `#movementRows`.
**Resultado esperado:** resultados acordes a filtros.  
**Verificación de persistencia:** confrontar con `ASSETMOVEMENTS`.  
**Validaciones adicionales:** filtros sin datos deben retornar lista vacía controlada.  
**Criterio de aprobación:** historial confiable para auditoría.  
**Resultado de ejecución:** Pendiente / Aprobada / Fallida / Bloqueada  
**Observaciones:** ____
