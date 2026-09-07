# SPEC 08 — Usuarios y migraciones Supabase

> **Status:** Approved
> **Depends on:** SPEC 07
> **Date:** 2026-09-07
> **Objective:** Crear los enums y la tabla `public.users` vinculada a Supabase Auth, con perfil propio protegido y una cuenta staff inicial para pruebas.

## Scope

**In:**

- Crear los enums `public.user_role` y `public.user_status`.
- Crear `public.users` vinculada uno a uno a `auth.users` y opcionalmente a `public.daycares`.
- Añadir un trigger para mantener `updated_at` automáticamente.
- Habilitar RLS para que cada usuario autenticado lea y actualice únicamente su propio perfil.
- Limitar las actualizaciones propias a `full_name`, `avatar_url`, `notify_on_post` y `daily_summary_enabled`.
- Crear `rey@google.com` mediante signup público sin guardar la contraseña en Git.
- Crear mediante una migración de datos el perfil `staff` activo de Rey asociado a `Guardería Soles`.
- Conservar las migraciones aplicadas en `supabase/migrations/` y verificar el proyecto remoto.

**Out of scope (for future specs):**

- Trigger de creación automática de perfiles al insertar en `auth.users`.
- Invitaciones, altas de padres, asignación de roles y pertenencia a guarderías.
- Autorización de staff o admin sobre otros perfiles de su guardería.
- Integración de Supabase Auth, sesiones o perfiles en Blazor.
- Verificación del inicio de sesión y confirmación de correo de `rey@google.com`.
- Los enums `relationship_type`, `invitation_status`, `post_type` y `child_status`.

## Data model

Enums:

| Enum | Valores |
| --- | --- |
| `public.user_role` | `staff`, `parent`, `admin` |
| `public.user_status` | `pending`, `active` |

Tabla: `public.users`.

| Columna | Tipo | Restricciones y valor predeterminado |
| --- | --- | --- |
| `id` | `uuid` | Clave primaria; FK a `auth.users(id)` con `on delete cascade` |
| `daycare_id` | `uuid` | Nullable; FK a `public.daycares(id)` con borrado restringido |
| `role` | `public.user_role` | `not null` |
| `status` | `public.user_status` | `not null default 'active'` |
| `full_name` | `text` | `not null`; rechaza vacío y solo espacios |
| `avatar_url` | `text` | Nullable |
| `notify_on_post` | `boolean` | `not null default true` |
| `daily_summary_enabled` | `boolean` | `not null default true` |
| `created_at` | `timestamptz` | `not null default now()` |
| `updated_at` | `timestamptz` | `not null default now()`; actualizado por trigger |

- RLS estará habilitado.
- Los roles `anon` y `authenticated` no podrán insertar ni eliminar perfiles.
- `authenticated` podrá seleccionar únicamente la fila cuyo `id` sea `auth.uid()`.
- `authenticated` podrá actualizar únicamente su propia fila y solo las columnas autorizadas.
- La cuenta inicial tendrá `full_name = 'Rey'`, `role = 'staff'`, `status = 'active'` y la guardería `Guardería Soles`.

## Implementation plan

1. Consultar las tablas, el historial de migraciones y los asesores de seguridad y rendimiento del proyecto Supabase conectado.
2. Preparar, aplicar mediante `supabase_apply_migration` y versionar la migración que crea `public.user_role` y `public.user_status`.
3. Preparar, aplicar y versionar la migración que crea `public.users`, sus claves foráneas, restricciones, trigger de `updated_at`, RLS, políticas y privilegios por columna.
4. Crear `rey@google.com` con signup público usando la contraseña acordada fuera de archivos versionados.
5. Preparar, aplicar y versionar la migración de datos que localiza el UUID de `auth.users` por el email `rey@google.com` e inserta el perfil staff sin UUID ni contraseña fijos.
6. Verificar la estructura, restricciones, permisos, perfil staff e historial de migraciones; revisar los asesores nuevamente y ejecutar `dotnet build`.

## Acceptance criteria

- [ ] Existen exactamente los enums `public.user_role` y `public.user_status` definidos en esta especificación.
- [ ] `public.user_role` contiene `staff`, `parent` y `admin`.
- [ ] `public.user_status` contiene `pending` y `active`.
- [ ] `public.users` contiene las columnas, tipos, valores predeterminados y restricciones definidos.
- [ ] Una inserción válida que omite `created_at` y `updated_at` genera ambos valores.
- [ ] Se rechaza `full_name` nulo, vacío o compuesto solo por espacios.
- [ ] Un perfil puede existir sin `daycare_id`.
- [ ] No se puede eliminar una guardería con perfiles asociados.
- [ ] Eliminar un usuario de `auth.users` elimina su perfil en `public.users`.
- [ ] Actualizar un perfil modifica `updated_at`.
- [ ] RLS está habilitado en `public.users`.
- [ ] Un usuario autenticado puede leer únicamente su propio perfil.
- [ ] Un usuario autenticado puede actualizar únicamente su propio `full_name`, `avatar_url` y preferencias.
- [ ] Un usuario autenticado no puede insertar, eliminar ni modificar su `id`, `daycare_id`, `role`, `status`, `created_at` o `updated_at`.
- [ ] `rey@google.com` existe en `auth.users` y tiene un perfil asociado con `full_name` `Rey`, rol `staff`, estado `active` y `Guardería Soles`.
- [ ] La contraseña de prueba no aparece en migraciones, configuración, documentación ni archivos versionados.
- [ ] Las migraciones locales coinciden con el historial remoto de Supabase.
- [ ] Los asesores de seguridad y rendimiento no tienen hallazgos atribuibles a esta funcionalidad.
- [ ] `dotnet build` termina sin errores.

## Decisions

- **Sí:** `daycare_id` nullable. Un usuario puede no tener guardería y solo puede referenciar una.
- **Sí:** impedir borrar una guardería con usuarios asociados. Obliga a reasignar o tratar los perfiles antes de borrar datos raíz.
- **Sí:** `full_name` obligatorio y no vacío. El perfil debe identificar a la persona.
- **Sí:** `updated_at` automático mediante trigger. Evita que cada cliente deba mantener la marca de modificación.
- **Sí:** acceso de cliente limitado al perfil propio. Protege información de otros usuarios mientras no existe autorización por guardería.
- **Sí:** permisos de actualización por columna. RLS identifica la fila y los privilegios de columna protegen rol, estado y pertenencia.
- **Sí:** cuenta staff creada por signup público y perfil sembrado desde una migración por email. No requiere UUID ni contraseña fijos en Git.
- **No:** trigger de perfil desde `auth.users`. La asignación segura de rol y guardería queda para invitaciones.
- **No:** verificar inicio de sesión de la cuenta staff. La confirmación de correo puede depender de la configuración de Auth y queda fuera de alcance.
- **No:** crear los demás enums de la referencia. Pertenecen a tablas futuras.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| Signup público requiere confirmación de correo | Limitar la verificación al vínculo entre Auth y el perfil, sin exigir inicio de sesión. |
| La migración de perfil se aplica antes de crear la cuenta Auth | Crear y comprobar la cuenta antes de aplicar la migración de datos. |
| RLS por sí sola no impide actualizar columnas sensibles | Revocar privilegios generales y conceder `update` solo sobre las cuatro columnas autorizadas. |
| Una futura asignación de rol desde metadatos permite escalamiento de privilegios | No crear trigger automático; diseñar el flujo de invitaciones con una vía privilegiada. |

## What is **not** in this spec

- Creación automática de perfiles o altas normales de usuarios.
- Invitaciones y asociaciones de padres, niños o guarderías.
- Lectura de usuarios de la misma guardería por staff o admin.
- Autenticación real desde Blazor y verificación de login.
- Los enums y tablas de niños, publicaciones, reacciones o invitaciones.
