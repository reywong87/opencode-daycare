# SPEC 07 — Tabla daycares y migraciones Supabase

> **Status:** Approved
> **Depends on:** Ninguna
> **Date:** 2026-09-07
> **Objective:** Crear y poblar `public.daycares` mediante migraciones versionadas en Git y aplicadas a Supabase mediante MCP.

## Scope

**In:**

- Establecer `supabase/migrations/` como historial SQL versionado.
- Crear `public.daycares` siguiendo la referencia `@docs/opendaycare-database-schema.md`.
- Habilitar RLS y mantener la tabla cerrada a clientes anónimos y autenticados.
- Insertar cuatro guarderías mediante una migración de datos independiente.
- Aplicar ambas migraciones al proyecto Supabase conectado durante la implementación.
- Documentar el procedimiento en `supabase/README.md`.

**Out of scope (for future specs):**

- Integración con Blazor, servicios C#, endpoints o CRUD.
- Tablas `rooms`, `users` y relaciones de pertenencia.
- Políticas de acceso por usuario o guardería.
- Configuración de un entorno Supabase local, automatización CI/CD y despliegues con CLI.

## Data model

Tabla: `public.daycares`.

| Columna | Tipo | Restricciones y valor predeterminado |
| --- | --- | --- |
| `id` | `uuid` | Clave primaria, `default gen_random_uuid()` |
| `name` | `text` | `not null`, restricción `check (length(btrim(name)) > 0)` |
| `created_at` | `timestamptz` | `not null`, `default now()` |

- No se añade `updated_at`.
- El nombre no es único.
- No se añaden índices secundarios.
- RLS queda habilitado, sin políticas que permitan acceso a `anon` o `authenticated`.
- Se revocan los privilegios de tabla de `PUBLIC`, `anon` y `authenticated` como protección adicional.
- La administración de los registros requiere acceso privilegiado.

La migración de datos insertará exactamente estos nombres:

1. `Guardería Soles`
2. `Guardería Luna`
3. `Guardería Arcoíris`
4. `Guardería Estrellitas`

Los UUID y las fechas se generan con los valores predeterminados. `Guardería Soles` representa una guardería, no una fila de `rooms`.

## Implementation plan

1. **Crear la migración de estructura.** Consultar el proyecto conectado y las tablas existentes. Preparar y revisar el SQL de `create_daycares`, incluyendo columnas, restricciones, RLS y revocación de privilegios. Aplicarlo exclusivamente mediante `supabase_apply_migration`. Conservar en Git exactamente el SQL aplicado y usar en el nombre del archivo la versión registrada por Supabase.
2. **Crear la migración de datos.** Preparar y aplicar `seed_daycares` mediante `supabase_apply_migration`, con una sola inserción de las cuatro guarderías y sin IDs fijos. Conservar el SQL y la versión efectiva en `supabase/migrations/`.
3. **Documentar el flujo.** Añadir `supabase/README.md` con el procedimiento de revisión, aplicación por MCP y verificación. Establecer que las migraciones aplicadas son inmutables y cualquier corrección requiere una nueva migración.

Cada aplicación debe verificarse antes de continuar al siguiente paso. No se reinicia la base remota ni se ejecutan nuevamente las migraciones históricas.

## Acceptance criteria

- [x] Las migraciones `create_daycares` y `seed_daycares` existen tanto en Git como en el historial remoto, con versiones y contenido coincidentes.
- [x] `public.daycares` contiene únicamente las columnas `id`, `name` y `created_at`, con los tipos y restricciones definidos.
- [x] Una inserción válida que omite `id` y `created_at` genera ambos valores.
- [x] Se rechazan nombres nulos, vacíos o compuestos únicamente por espacios.
- [x] Se permiten guarderías distintas con el mismo nombre.
- [x] RLS está habilitado y no existen políticas de acceso para clientes.
- [x] Los roles `anon` y `authenticated` no pueden consultar ni modificar registros, comprobado mediante pruebas de permisos.
- [x] La tabla contiene exactamente las cuatro guarderías acordadas al finalizar la implementación.
- [x] Las pruebas de escritura se revierten y no dejan registros adicionales.
- [x] No se han creado tablas de dominio adicionales ni modificado Blazor.
- [x] Se han consultado los asesores de seguridad y rendimiento después de los cambios y resuelto los hallazgos atribuibles a esta funcionalidad.
- [x] `supabase/README.md` explica cómo aplicar futuras migraciones sin duplicar ejecuciones ni desalinear el historial.
- [x] `dotnet build` termina sin errores.

## Decisions

- **Sí:** migraciones SQL en Git y aplicación mediante MCP, conforme a `AGENTS.md`.
- **Sí:** separar estructura y datos en dos migraciones.
- **Sí:** insertar datos de ejemplo también en el proyecto remoto conectado, según lo confirmado.
- **Sí:** mantener acceso administrativo únicamente hasta definir usuarios y pertenencia a guarderías.
- **Sí:** usar las versiones efectivas de Supabase para evitar divergencias con los nombres locales.
- **No:** añadir unicidad al nombre, `updated_at`, salas o integración con la aplicación.
- **No:** hacer el seed repetible por nombre. El historial controla su ejecución única y los nombres no son identificadores únicos.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| Datos de ejemplo presentes en futuros entornos | Documentar que `seed_daycares` forma parte del historial y se ejecutará al reproducirlo completo. |
| Confundir RLS con permisos de tabla | Verificar ambos controles y probar los roles de cliente. |
| Aplicar cambios al proyecto equivocado | Confirmar la identidad del proyecto conectado antes de ejecutar las migraciones. |

## What is **not** in this spec

- Salas, usuarios, autenticación o autorización por guardería.
- Lectura o escritura desde Blazor.
- Datos reales de niños o familias.
- Entorno Supabase local, CI/CD o cambios en la referencia `@docs`.
