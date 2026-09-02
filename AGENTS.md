# OpenDaycare

## Project Shape

- This is a single `net10.0` ASP.NET Core Blazor Web App; run project commands from the repository root with `dotnet` (there is no solution file).
- `Program.cs` configures Razor Components with interactive server rendering and maps `App` as the application root. Keep new interactive UI compatible with server render mode.
- Routed pages live in `Components/Pages/`; `Components/Routes.razor` applies `Layout/MainLayout.razor` by default. Put app-wide document assets in `Components/App.razor` and static files in `wwwroot/`.
- Component-scoped CSS uses adjacent `*.razor.css` files. The generated stylesheet is referenced as `OpenDaycare.styles.css`; do not edit `bin/` or `obj/` artifacts.

## Commands

- Build: `dotnet build`
- Run with the configured HTTPS profile: `dotnet run --launch-profile https` (HTTPS `https://localhost:7088`; HTTP `http://localhost:5138`).
- There is currently no test project, formatter, lint configuration, or CI workflow. Use `dotnet build` as the available verification baseline.

## Runtime

- Development runs with `ASPNETCORE_ENVIRONMENT=Development` through the launch profiles. Development settings are in `appsettings.Development.json`; shared settings are in `appsettings.json`.
- The HTTP pipeline re-executes unmatched status codes at `/not-found`, forces HTTPS redirection, enables antiforgery, and maps static assets. Preserve these behaviors when changing application startup.

## MCP

- Guarda capturas, archivos y otros artefactos de Playwright en `.playwright-mcp/`.
- Usa Context7 para consultar documentación actual de frameworks, librerías, SDKs, APIs, CLIs y servicios en la nube. No lo uses para refactors, scripts desde cero, depuración de lógica de negocio ni revisiones de código.
- Antes de consultar documentación con Context7, resuelve el identificador de la librería y usa después ese identificador en la consulta.

## Supabase

- Antes de cambios de esquema, consulta las tablas existentes con `supabase_list_tables`.
- Aplica DDL únicamente mediante `supabase_apply_migration`; usa `supabase_execute_sql` para consultas o cambios de datos que no sean DDL.
- Antes de investigar problemas de Supabase, revisa logs y asesores. Tras cambios de esquema, consulta los asesores de seguridad y rendimiento.
- Para integraciones del cliente, obtén la URL del proyecto y una clave publicable mediante las herramientas de Supabase; nunca expongas claves de servicio en el cliente.
- Da preferencia al desarrollo y las pruebas locales con la CLI de Supabase cuando esté disponible. Las migraciones y operaciones MCP se aplican al proyecto remoto, por lo que deben usarse con cuidado.

## Skills

- `context7-mcp`: consulta documentación vigente para librerías, frameworks, SDKs, APIs, CLIs y servicios en la nube.
- `find-skills`: localiza e instala skills cuando se necesite una capacidad adicional.
- `spec`: diseña especificaciones con el flujo spec-driven antes de iniciar una funcionalidad grande.
- `spec-impl`: implementa una especificación aprobada; valida su estado, crea una rama con el nombre del spec y avanza por pasos revisando los cambios.
- `supabase`: úsala para cualquier tarea relacionada con Supabase, incluidos Database, Auth, Edge Functions, Realtime, Storage, CLI, MCP, RLS, migraciones y diagnóstico. Antes de implementar, revisa el changelog y la documentación actual; verifica las correcciones mediante una consulta o prueba.
- `supabase-postgres-best-practices`: cárgala antes de crear o modificar cualquier elemento de Postgres, incluidas tablas, columnas, migraciones, políticas RLS, índices, triggers, funciones, trabajos programados o SQL. También úsala para diagnosticar rendimiento, bloqueos, agotamiento de conexiones o exposición incorrecta de filas.
- El subagente `spec-verifier` verifica, corrige y actualiza los criterios de aceptación de un spec. Para criterios de .NET o Blazor consulta Context7 y, para criterios de interfaz, usa Playwright y guarda los artefactos en `.playwright-mcp/`.
