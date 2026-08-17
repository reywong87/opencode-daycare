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
- Playwright screenshots and whatever things related to playwright must be in the .playwright-mcp folder. 
- Context7 We use this MCP to read the framework documentation

## Spec Driven Development - Skills
- /spec usaremos esta habilidad para crear las especificaciones.
- /spec-impl Usaremos esta skill para hacer las implementaciones.
- El subagente `spec-acceptance-verifier` verifica, corrige y actualiza los criterios de aceptación de un spec. Para criterios de .NET o Blazor consulta Context7 y, para criterios de interfaz, usa Playwright y guarda los artefactos en `.playwright-mcp/`.
