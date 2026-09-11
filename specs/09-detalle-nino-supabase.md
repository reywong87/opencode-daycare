# SPEC 09 — Detalle de niño con Supabase

> **Status:** Approved
> **Depends on:** SPEC 02, SPEC 05, SPEC 08
> **Date:** 2026-09-11
> **Objective:** Cargar y presentar el detalle de un niño persistido y autorizado desde Supabase mediante su slug único.

## Scope

**In:**

- Mantener la ruta `/kids/{Slug}` e identificar al niño mediante un slug único persistido.
- Añadir y rellenar `children.slug` con una migración que preserve la unicidad de los niños existentes.
- Generar y persistir un slug único al crear un niño desde `AddKidModal.razor`.
- Hacer que cada tarjeta de niño de `/kids` navegue al detalle mediante su slug.
- Cargar el niño activo autorizado y el nombre de su sala desde `ChildrenService`.
- Mostrar el nombre, avatar derivado, edad calculada, sala, fecha de nacimiento, fecha de ingreso y notas médicas del niño cargado.
- Mantener los controles visuales existentes `Editar` y `Resumen del día` sin comportamiento funcional.
- Mantener el botón `Vincular otro padre` y el modal visual de invitación existente.
- Ocultar los padres demo del catálogo y no mostrar relaciones de padre en el detalle persistido.
- Redirigir a `/not-found` si el slug no existe, el niño está archivado o RLS no permite leerlo.
- Mostrar un estado de carga y un estado de error recuperable con reintento ante fallos temporales de carga.

**Out of scope (for future specs):**

- Crear, enviar, aceptar, cancelar o persistir invitaciones de padre.
- Consultar o mostrar `parent_children`, usuarios padre o estados de invitación.
- Editar, archivar o eliminar niños.
- Crear o mostrar resúmenes diarios.
- Subir o persistir fotografías y consentimientos de fotos.
- Cambiar políticas RLS de Supabase.

## Data model

La migración añadirá la columna siguiente a `public.children` sin modificar las políticas RLS existentes:

```sql
slug text not null unique
```

La migración generará el slug para cada niño existente a partir de las primeras tres palabras de `full_name`: nombre, primer apellido y segundo apellido. Normalizará a minúsculas, removerá acentos y sustituirá los caracteres no alfanuméricos por guiones. Si faltan apellidos, usará las palabras disponibles. Si el resultado ya existe, añadirá un sufijo incremental, como `sofia-2` o `sofia-garcia-2`, hasta obtener un valor único.

La funcionalidad añadirá o actualizará los siguientes contratos de aplicación:

```csharp
ChildDetailItem
{
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    DateOnly EnrolledAt,
    string RoomName,
    string? MedicalNotes
}

ChildListItem
{
    Guid Id,
    string Slug,
    string FullName,
    DateOnly BirthDate,
    Guid RoomId,
    string RoomName,
    string? MedicalNotes
}
```

`Models/Child.cs` mapeará la columna `slug`. `ChildrenService` generará un slug candidato desde `FullName`, comprobará los slugs autorizados existentes y añadirá un sufijo incremental cuando sea necesario antes de insertar. La restricción única de la base de datos conservará la integridad si dos altas concurrentes generan el mismo candidato.

`ChildrenService.GetActiveChildBySlugAsync(string slug)` consultará el niño con `slug` solicitado y `status = active`. Obtendrá el nombre de la sala autorizada asociada a `room_id` y devolverá `null` si no existe una coincidencia visible. La ausencia por RLS se tratará igual que un niño inexistente para no revelar datos de otra guardería.

La edad se calculará en la interfaz a partir de `BirthDate` y la fecha local actual. El avatar conservará las iniciales de `FullName` y el color determinista derivado de `Id` ya usado en `KidCard.razor`.

## Implementation plan

1. Crear una migración en `supabase/migrations/` que añada `children.slug`, rellene los slugs únicos de niños existentes y aplique las restricciones `NOT NULL` y `UNIQUE` al terminar el relleno.
2. Actualizar `Models/Child.cs` con el mapeo PostgREST de `slug`, y `Dtos/ChildListItem.cs` y `Dtos/ChildDetailItem.cs` con los contratos de lectura necesarios.
3. Actualizar `Services/ChildrenService.cs` para generar un slug normalizado y único durante el alta, incluirlo en el listado y añadir `GetActiveChildBySlugAsync(string slug)` que resuelva el niño activo y su sala autorizada.
4. Actualizar `Components/Shared/KidCard.razor` para enlazar toda la tarjeta persistida a `/kids/{Child.Slug}` y conservar la información visual actual.
5. Actualizar `Components/Pages/KidProfile.razor` para recibir `string Slug`, cargar `ChildDetailItem` al cambiar el parámetro y reemplazar el acceso a `KidCatalog`.
6. Presentar en `KidProfile.razor` los estados de carga, error con `Reintentar`, redirección a `/not-found` por resultado nulo y el detalle cargado con edad, fechas, sala y notas médicas.
7. Retirar la lista de padres demo del detalle y conservar únicamente el botón que abre `LinkParentModal`; dejar `Editar` y `Resumen del día` como acciones visuales sin lógica.
8. Ejecutar `dotnet build` y verificar en navegador la navegación desde `/kids`, el detalle autorizado, el slug inexistente, un niño archivado y el diseño a 1440px y 390px.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] La migración añade `children.slug` como columna `NOT NULL` y `UNIQUE` sin modificar políticas RLS.
- [ ] La migración rellena cada slug existente con nombre y hasta dos apellidos normalizados, usa las palabras disponibles cuando faltan apellidos y añade un sufijo incremental si existe una colisión.
- [ ] Crear un niño desde `/kids` persiste un slug único con el mismo formato.
- [ ] Cada `KidCard` persistida de `/kids` navega a `/kids/{slug}` con el slug del niño.
- [ ] La ruta `/kids/{Slug}` no consulta `KidCatalog`.
- [ ] Un usuario autorizado ve el nombre, avatar, edad, sala, fecha de nacimiento, fecha de ingreso y notas médicas del niño activo solicitado.
- [ ] La edad disminuye en un año hasta que llegue el aniversario de `BirthDate` en el año local actual.
- [ ] Un niño sin `medical_notes` muestra `Sin alergias ni notas registradas`.
- [ ] Un slug inexistente redirige a `/not-found`.
- [ ] Un slug de niño archivado redirige a `/not-found`.
- [ ] Un niño no visible por RLS redirige a `/not-found` sin revelar que pertenece a otra guardería.
- [ ] Un fallo temporal de Supabase muestra un error visible y `Reintentar` vuelve a solicitar el mismo detalle.
- [ ] El detalle muestra un estado de carga mientras la solicitud inicial está pendiente.
- [ ] El detalle no muestra nombres, parentescos ni estados demo de padres.
- [ ] El botón `Vincular otro padre` permanece visible y abre el modal visual existente.
- [ ] `Editar` y `Resumen del día` siguen visibles y no ejecutan operaciones de persistencia.
- [ ] A 1440px y 390px, el detalle, sus estados de carga/error y el modal de invitación son legibles y no producen desbordamiento horizontal.

## Decisions

- **Sí:** slug único persistido en `children`. Conserva la ruta estable `/kids/{Slug}` sin depender de datos demo.
- **Sí:** nombre y hasta dos apellidos como base del slug. Produce rutas legibles a partir de `full_name` sin nuevos campos de entrada.
- **Sí:** palabras disponibles y sufijo incremental por colisión. Permite valores únicos como `sofia-2` y `sofia-garcia-2` sin requerir nombres completos.
- **No:** UUID en la ruta. La URL debe conservar el formato basado en slug solicitado.
- **Sí:** consulta filtrada por `status = active`. Un niño archivado no debe poder abrirse mediante una URL conocida.
- **Sí:** tratar la ausencia por RLS como no encontrado. Evita confirmar la existencia de un niño de otra guardería.
- **Sí:** cálculo local de edad. La base de datos persiste la fecha de nacimiento y no necesita almacenar un valor que cambia con el tiempo.
- **Sí:** conservar el botón y modal de invitación como interfaz visual. Es un flujo existente, pero su carga y persistencia quedan fuera de este spec.
- **No:** padres demo en el detalle. No corresponden a los niños persistidos y darían información falsa.
- **Sí:** dejar visibles `Editar` y `Resumen del día` sin acción. Conserva el diseño actual hasta que sus flujos tengan una especificación propia.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| RLS y un slug inexistente devuelven el mismo resultado | Redirigir ambos casos a `/not-found` y no exponer detalles de autorización. |
| La sala asociada no es visible o fue eliminada | Tratar el resultado incompleto como no encontrado y evitar presentar un detalle inconsistente. |
| La edad queda desactualizada tras un aniversario | Calcularla a partir de la fecha local en cada renderizado del detalle. |
| El modal visual sugiere una invitación funcional | Mantener explícita la exclusión de creación y persistencia de invitaciones. |
| Dos altas simultáneas generan el mismo slug | Mantener la restricción única y mostrar un error recuperable si la inserción entra en conflicto. |

## What is **not** in this spec

- Vínculos de padre, cuentas padre, `parent_children` o estados de invitación cargados desde Supabase.
- Creación, envío, aceptación, cancelación o persistencia de invitaciones.
- Edición, archivo o eliminación de niños.
- Resúmenes diarios, fotografías, documentos o consentimiento de fotos.
- Cambios de políticas RLS.
