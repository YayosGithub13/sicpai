# MANUAL MAESTRO DE OPERACIÓN Y VALIDACIÓN FUNCIONAL – SICPAI

## Índice general
1. [Preparación del ambiente y seguridad](./01_PREPARACION_Y_SEGURIDAD.md)
2. [Configuración inicial y catálogos](./02_CONFIGURACION_Y_CATALOGOS.md)
3. [Gestión de bienes y operación diaria](./03_BIENES_Y_OPERACION.md)
4. [Auditorías, inventarios, reportes y bitácora](./04_AUDITORIA_INVENTARIO_REPORTES.md)
5. [Matriz de permisos, checklist, incidencias y resumen](./05_MATRICES_Y_CHECKLIST.md)

---

## Alcance y reglas de uso
- Manual basado en la implementación real actual del repositorio.
- Enfoque: pruebas funcionales manuales end-to-end.
- Este manual **no modifica código**, **no ejecuta migraciones destructivas** y **no asume funcionalidades inexistentes**.
- Cuando una funcionalidad no está completa, se marca como **Pendiente/Incompleta** y se especifica qué sí puede probarse.

## Convenciones
- **Pendiente**: no ejecutada aún.
- **Aprobada**: cumple resultado esperado.
- **Fallida**: no cumple resultado esperado.
- **Bloqueada**: no puede ejecutarse por dependencia faltante.

## Estructura de cada prueba
Cada caso de prueba en los capítulos 1–4 usa el formato:
- ID
- Módulo
- Nombre de la prueba
- Objetivo
- Precondiciones
- Usuario requerido
- Datos de prueba
- Procedimiento paso a paso
- Resultado esperado
- Verificación de persistencia
- Validaciones adicionales
- Criterio de aprobación
- Resultado de ejecución
- Observaciones

## Referencias técnicas usadas
- `SICPAI/Program.cs`
- `Infrastructure/Seed/DataSeeder.cs`
- `SICPAI/Security/PermissionAuthorization.cs`
- Controladores en `SICPAI/Controllers/*.cs`
- Vistas en `SICPAI/Views/**/*.cshtml`

---

## Estado funcional general detectado (resumen rápido)
- Login/autorización: funcional, con políticas por permiso.
- Catálogos (áreas/ubicaciones/responsables): funcional en nivel base.
- Bienes (alta/listado/detalle/movimientos/importación): funcional con validaciones principales.
- Transferencias: creación + aprobación/rechazo funcionales.
- Inventarios y auditorías: funcionales en flujos principales.
- Reportes: exportación Excel/PDF funcional en endpoints principales.
- Bitácora: consulta API funcional con restricción de permiso.
- Configuración: tasas de depreciación funcionales.

## Funcionalidades con alcance parcial o pendiente
- Algunos elementos de detalle avanzado (ej. secciones de auditoría detallada por activo) muestran información derivada o placeholder controlado.
- No existe módulo dedicado de “categorías de bienes” separado; se usa `AssetType` como clasificación.
- Algunas exportaciones visuales secundarias dependen de botones de conveniencia (se homologaron a endpoints reales principales).

---

Siguiente paso: iniciar por [Capítulo 1](./01_PREPARACION_Y_SEGURIDAD.md).