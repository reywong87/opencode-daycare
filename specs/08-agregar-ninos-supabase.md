# SPEC 08 — Agregar niños con Supabase

> **Status:** Implemented
> **Depends on:** SPEC 04, SPEC 07
> **Date:** 2026-09-10
> **Objective:** Persistir niños desde `AddKidModal.razor` en Supabase y mostrar en `/kids` el listado real agrupado por sala para la guardería autorizada.

## Scope

**In:**

- Crear `public.rooms` y `public.children` según el esquema de referencia, con claves foráneas, valores predeterminados y RLS.
- Permitir leer salas y niños activos, e insertar niños, solo a perfiles `active` con rol `staff` o `admin` dentro de su propia guardería.
- Sembrar `Soles`, `Estrellas` y `Arcoíris` para cada guardería existente mediante una operación atómica.
- Sembrar los ocho niños de `KidCatalog` solo en `Guardería Soles`, con los datos de niño y notas existentes, dentro de la misma transacción de la migración.
- Reemplazar el selector fijo de `AddKidModal.razor` por las salas cargadas desde Supabase para la guardería del usuario autenticado.
- Persistir `full_name`, `birth_date`, `room_id`, `enrolled_at`, `medical_notes`, `photo_consent` y `status` cuando se pulse Guardar con datos válidos.
- Guardar alergias y notas en `medical_notes` como bloques etiquetados, omitiendo los bloques sin contenido.
- Mantener el modal abierto, conservar el borrador y mostrar un error general reintentable cuando falle el alta.
- Cargar `/kids` desde Supabase, filtrar localmente por nombre y agrupar los niños activos por sala en orden alfabético.
- Mostrar un estado de error con reintento si falla la carga inicial de salas o niños.
- Renderizar avatares con iniciales y un color determinista derivado del UUID del niño.

**Out of scope (for future specs):**

- Migrar `/kids/{slug}` a perfiles persistidos y reemplazar los perfiles demo existentes.
- Crear, editar, archivar o eliminar salas.
- Crear, editar, archivar o eliminar niños después de su alta inicial.
- Crear o vincular padres, invitaciones y relaciones `parent_children`.
- Persistir `allergy_tags`, fotografías, documentos o consentimiento configurable de fotos.
- Permitir que perfiles `parent`, inactivos o de otra guardería lean o creen niños.

## Data model

La migración añadirá las tablas siguientes en `public` y activará RLS en ambas:

```sql
rooms
{
    id uuid primary key,
    daycare_id uuid not null references daycares(id),
    name text not null,
    created_at timestamptz not null
}

children
{
    id uuid primary key,
    room_id uuid not null references rooms(id),
    full_name text not null,
    birth_date date not null,
    enrolled_at date not null,
    medical_notes text null,
    allergy_tags text[] null,
    photo_consent boolean not null default true,
    status child_status not null default 'active',
    created_at timestamptz not null,
    updated_at timestamptz not null
}
```

La migración declarará el enum `child_status` con `active` y `archived`. Las políticas de `rooms` y `children` comprobarán, mediante `public.users`, que `auth.uid()` corresponde a un perfil `active` de rol `staff` o `admin` de la misma `daycare_id`; los niños se autorizan a través de la sala relacionada. No se concederán políticas de actualización ni eliminación en esta spec.

Los ocho registros demo usarán la sala `Soles` de `Guardería Soles`. `enrolled_at` convertirá el mes y año de `KidCatalog` en el primer día de ese mes. La migración localizará esa guardería por su nombre dentro de la transacción, sin depender de UUID escritos de forma fija.

El formulario enviará este comando de aplicación:

```csharp
CreateChildRequest
{
    string FullName,
    DateOnly BirthDate,
    Guid RoomId,
    string Allergies,
    string MedicalNotes
}
```

`ChildrenService` transformará el comando en un registro `children` con `enrolled_at` igual a la fecha local actual, `photo_consent` en `true` y `status` en `active`. `medical_notes` se formará como `Alergias: {valor}` y `Notas médicas: {valor}` separados por una línea nueva, omitiendo cualquier bloque vacío. Los nombres duplicados se permiten.

La carga de `/kids` usará un modelo de lectura con `Id`, `FullName`, `BirthDate`, `RoomId`, `RoomName` y `MedicalNotes`. No reutilizará `KidCatalog` en esta ruta. El color del avatar se seleccionará de una paleta fija mediante un hash estable de `Id` y sus iniciales se derivarán de `FullName`.

## Implementation plan

1. Crear una migración imperativa en `supabase/migrations/` que defina `child_status`, `public.rooms` y `public.children`, sus restricciones, valores predeterminados, RLS y políticas de lectura e inserción para personal activo de la misma guardería.
2. Añadir a la misma migración el sembrado transaccional de las tres salas para cada guardería existente y de los ocho niños demo en `Guardería Soles`; convertir cada mes de ingreso demo al primer día de ese mes.
3. Crear `Models/Room.cs` y `Models/Child.cs` con el mapeo de PostgREST, y `Dtos/CreateChildRequest.cs` y `Dtos/ChildListItem.cs` para los contratos de alta y listado.
4. Crear `Services/ChildrenService.cs` para cargar las salas autorizadas, cargar niños activos con su sala ordenados por nombre e insertar un niño en una sala autorizada; registrar detalles técnicos sin exponerlos en la interfaz.
5. Registrar `ChildrenService` en `Program.cs` y verificar que el cliente Supabase conserva la sesión autenticada para que las políticas RLS se apliquen a sus operaciones.
6. Actualizar `Components/Shared/AddKidModal.razor` para recibir las salas y un callback de guardado asíncrono, bloquear acciones duplicadas durante el envío, persistir el borrador válido y solo cerrarse después de una inserción correcta.
7. Mantener la validación actual de nombre, fecha no futura y sala obligatoria, añadir el error de envío visible y conservar borrador, errores de validación y foco cuando la inserción falle.
8. Actualizar `Components/Pages/Kids.razor` para cargar salas y niños a través de `ChildrenService`, abrir el modal con salas reales, recargar la colección tras el alta y mostrar secciones por sala con sus contadores y búsqueda local.
9. Actualizar `Components/Shared/KidCard.razor` para presentar niños persistidos sin enlace a perfil, con iniciales, color determinista, fecha de nacimiento, sala y notas disponibles; conservar `KidCatalog` solo para los perfiles demo fuera de alcance.
10. Añadir estados de carga y de error recuperable con reintento en `/kids`, y verificar en navegador el alta, la actualización inmediata, los límites de RLS y el comportamiento a 1440px y 390px.

## Acceptance criteria

- [x] `dotnet build` termina sin errores.
- [x] La migración crea `public.rooms`, `public.children` y el enum `child_status` con RLS habilitada.
- [x] La migración crea `Soles`, `Estrellas` y `Arcoíris` para cada guardería existente sin dejar resultados parciales si falla.
- [x] La migración inserta los ocho niños de `KidCatalog` solo en `Guardería Soles` y usa el primer día del mes indicado como `enrolled_at`.
- [x] Un perfil `active` con rol `staff` o `admin` solo puede leer salas y niños de su propia guardería.
- [x] Un perfil `active` con rol `staff` o `admin` solo puede insertar un niño en una sala de su propia guardería.
- [x] Un perfil `parent`, un perfil inactivo y un usuario de otra guardería no pueden leer ni insertar niños o salas mediante RLS.
- [x] `/kids` no usa `KidCatalog` para su listado, contador ni búsqueda.
- [x] `/kids` muestra únicamente niños con `status = active` cargados desde Supabase, agrupados por sala y ordenados alfabéticamente por nombre.
- [x] El selector de `AddKidModal.razor` muestra exclusivamente las salas de la guardería autorizada.
- [x] Guardar con nombre, fecha no futura y sala válidos inserta un niño con `enrolled_at` igual a la fecha local actual, `photo_consent = true` y `status = active`.
- [x] Si se rellenan alergias y notas, `medical_notes` contiene los bloques `Alergias:` y `Notas médicas:`; un bloque vacío no se guarda.
- [x] Se permiten niños activos con el mismo nombre y sala.
- [x] Tras una inserción correcta, el modal se cierra, se restablece y el nuevo niño aparece sin recargar el navegador en la sección de la sala elegida.
- [x] Si falla la inserción, el modal permanece abierto, conserva los campos y muestra un error general que permite volver a intentarlo.
- [x] Si falla cargar salas o niños, `/kids` muestra un error visible con una acción de reintento y no lo presenta como una lista vacía.
- [x] Cada tarjeta persistida muestra iniciales y un color consistente para el mismo UUID y no navega a `/kids/{slug}`.
- [x] A 1440px y 390px, el modal, los estados de carga/error y las secciones por sala son legibles y no producen desbordamiento horizontal.

## Decisions

- **Sí:** `rooms` y `children` se crean en esta spec. El proyecto remoto aún no dispone de las tablas necesarias para el alta real.
- **Sí:** RLS por perfil activo, rol operativo y guardería. Impide que padres, perfiles inactivos y personal de otra guardería accedan a datos infantiles.
- **Sí:** `staff` y `admin` pueden crear. Ambos son roles operativos del modelo existente.
- **No:** permisos basados en metadatos del JWT. Los roles y la guardería se consultan desde `public.users`, que es la fuente de autorización de la aplicación.
- **Sí:** tres salas iniciales por guardería. El selector tiene datos reales desde la primera ejecución sin introducir todavía gestión de salas.
- **Sí:** ocho niños demo solo en `Guardería Soles`. Evita multiplicar datos de demostración en todas las guarderías.
- **Sí:** siembra atómica. No deben permanecer salas o niños demo incompletos después de un fallo.
- **Sí:** `medical_notes` con bloques etiquetados. Conserva el texto libre exacto de alergias y notas sin imponer una taxonomía clínica.
- **No:** `allergy_tags` en el alta. La interfaz no ofrece etiquetas normalizadas ni una conversión fiable a inglés.
- **Sí:** datos del listado exclusivamente desde Supabase. El catálogo estático no representa la fuente de verdad para `/kids`.
- **Sí:** tarjetas no navegables. Los perfiles persistidos y sus relaciones requieren una segunda spec específica.
- **Sí:** avatares derivados de UUID y nombre. Mantienen una apariencia estable sin almacenar imágenes o estilos por niño.
- **Sí:** entidades PostgREST en `Models/` y contratos de aplicación en `Dtos/`. Separa las columnas de base de datos de los datos intercambiados entre servicio e interfaz.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| Una política RLS permite cruzar guarderías a través de `room_id` | Verificar lectura e inserción con identidades de guarderías distintas y comprobar la guardería mediante la sala relacionada. |
| El selector no tiene salas por un fallo de red o autorización | Mostrar error recuperable y no permitir crear con una sala implícita. |
| La semilla deja registros parciales | Ejecutar creación y siembra en una única transacción. |
| Alergias de texto libre no permiten búsquedas clínicas fiables | Guardarlas como notas etiquetadas y posponer `allergy_tags` para una UX normalizada. |
| Los perfiles demo divergen temporalmente de la lista real | Evitar enlaces desde tarjetas persistidas y migrar perfiles en SPEC 09. |

## What is **not** in this spec

- Perfiles de niños cargados desde Supabase.
- Vínculos de padres, invitaciones o cuentas de padre.
- Gestión, edición o archivo de salas y niños.
- Etiquetas clínicas normalizadas, fotografías o documentos.
- Acceso de padres a salas o niños.
