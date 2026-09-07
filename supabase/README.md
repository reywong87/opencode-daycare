# Migraciones de Supabase

`supabase/migrations/` es el historial SQL versionado del proyecto. Cada archivo
representa una migración ya aplicada al proyecto Supabase conectado.

## Aplicar una migración nueva

1. Consulta el proyecto conectado, las tablas existentes y el historial remoto
   con las herramientas MCP de Supabase. Antes de un cambio de esquema, revisa
   también los asesores de seguridad y rendimiento.
2. Redacta y revisa el SQL. Las migraciones de estructura y datos se mantienen
   separadas cuando corresponda.
3. Aplica el SQL una sola vez con `supabase_apply_migration`, usando un nombre
   descriptivo en `snake_case`.
4. Consulta el historial remoto con `supabase_list_migrations`. Crea en
   `supabase/migrations/` el archivo
   `<version>_<nombre>.sql`, con la versión efectiva devuelta por Supabase y
   exactamente el SQL aplicado.
5. Verifica el cambio con consultas de solo lectura y, si aplica, con pruebas
   de permisos. Ejecuta de nuevo los asesores tras los cambios de esquema.

## Reglas del historial

- No se reinicia la base de datos remota ni se vuelven a ejecutar migraciones
  históricas.
- Las migraciones aplicadas son inmutables. Una corrección se realiza mediante
  una migración nueva; no se edita ni se elimina un archivo ya registrado.
- Antes de aplicar una migración, compara el historial local con el remoto para
  evitar duplicar una ejecución o desalinear sus versiones.
- Las operaciones DDL se aplican exclusivamente mediante
  `supabase_apply_migration`. Las consultas de verificación y los cambios de
  datos que no sean DDL usan `supabase_execute_sql`.
