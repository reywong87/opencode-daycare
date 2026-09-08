# SPEC 07 — Autenticación y protección de rutas

> **Status:** Approved
> **Depends on:** SPEC 03, SPEC 08
> **Date:** 2026-09-08
> **Objective:** Integrar inicio de sesión por email y contraseña con Supabase, persistir la sesión y restringir las rutas funcionales a usuarios con un perfil activo.

## Scope

**In:**

- Inicializar y usar Supabase Auth desde servicios .NET inyectables para iniciar y cerrar sesión con email y contraseña.
- Persistir la sesión de Supabase en almacenamiento protegido del navegador y restaurarla tras una recarga o visita posterior.
- Crear un `AuthenticationStateProvider` para exponer la sesión de Supabase a los componentes Blazor.
- Cargar el perfil propio desde `public.users` y permitir el acceso solo cuando el perfil exista y tenga estado `active`.
- Mostrar el `full_name`, el rol y la guardería del perfil autenticado en `Components/Shared/Sidebar.razor` en lugar de los datos demo.
- Actualizar `Components/Pages/Login.razor` para autenticar contra Supabase, mostrar estados de envío y errores genéricos de credenciales o conexión.
- Redirigir a una ruta interna solicitada mediante `returnUrl` validado después de iniciar sesión, con `/` como destino predeterminado.
- Redirigir a una ruta privada solicitada por una persona anónima hacia `/login` con su ruta de retorno.
- Impedir que una persona con sesión activa vea el formulario de `/login` y redirigirla a su retorno válido o al feed.
- Aplicar autorización a las rutas funcionales actuales y hacer que las nuevas rutas funcionales se declaren protegidas por defecto.
- Mantener públicas `/login`, `/activate-account`, `/not-found` y `/Error`.
- Mantener `Components/Pages/ActivateAccount.razor` y el enlace `Activa tu cuenta` como interfaz visual sin crear cuentas ni sesiones.
- Conectar el botón existente `Cerrar sesión` de `Components/Shared/Sidebar.razor` para cerrar la sesión remota, eliminar la sesión local y navegar a `/login`.

**Out of scope (for future specs):**

- Registro público de usuarios.
- Activación real mediante invitaciones, códigos o creación de contraseña desde `/activate-account`.
- Recuperación o cambio de contraseña.
- Autorización por rol, guardería, niño o recurso.
- Crear, editar o sincronizar perfiles de `public.users`.
- Modificar el esquema, RLS, políticas o datos de Supabase.
- Inicio de sesión con proveedores sociales, magic links, MFA o passkeys.

## Data model

Esta funcionalidad no modifica estructuras persistentes de la base de datos.

Reutiliza `auth.users` y el perfil propio de `public.users` definido en SPEC 08. El estado de aplicación mantendrá una sesión autenticada y el perfil asociado:

```csharp
AuthenticatedUser
{
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? DaycareName,
    string Status
}
```

La sesión serializada de Supabase se guardará bajo una clave de almacenamiento protegido exclusiva de OpenDaycare. Se eliminará al cerrar sesión o cuando la restauración no produzca un usuario con perfil activo.

## Implementation plan

1. Actualizar `Program.cs` para registrar los servicios de autorización, estado autenticado, almacenamiento protegido y los servicios de autenticación y perfil de OpenDaycare.
2. Crear `Services/SupabaseAuthService.cs` para inicializar el cliente Supabase, iniciar sesión con email y contraseña, restaurar una sesión persistida y cerrar sesión sin exponer credenciales ni claves de servicio.
3. Crear el modelo de perfil autenticado y añadir al servicio la consulta del perfil propio de `public.users`, incluida su guardería, mediante las políticas RLS existentes.
4. Crear `Services/SupabaseAuthenticationStateProvider.cs` para convertir una sesión con perfil activo en un `ClaimsPrincipal` y notificar a Blazor cada cambio de autenticación.
5. Crear un componente interactivo de inicialización de autenticación que restaure el almacenamiento protegido después de la conexión del circuito y no renderice rutas privadas antes de resolver la sesión.
6. Actualizar `Components/App.razor` para envolver el enrutado en el inicializador y el estado de autenticación en cascada.
7. Actualizar `Components/Routes.razor` para usar `AuthorizeRouteView`, enviar personas anónimas al login con un retorno interno seguro y mantener una lista explícita de rutas públicas.
8. Declarar protegidas las páginas funcionales actuales: `/`, `/kids`, `/kids/{Slug}`, `/counter` y `/weather`; mantener públicas las cuatro rutas excluidas en el alcance.
9. Actualizar `Components/Pages/Login.razor` para enviar email y contraseña al servicio, deshabilitar el formulario durante el envío, comunicar errores genéricos y navegar al retorno validado después de autenticar y validar el perfil.
10. Actualizar `Components/Shared/Sidebar.razor` para mostrar `full_name`, rol y guardería del perfil autenticado, y conectar su botón de salida al servicio de autenticación.
11. Verificar compilación, restauración de sesión, navegación anónima, redirección de retorno, perfil inexistente o inactivo, errores de acceso y cierre de sesión en navegador.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] Una cuenta existente de Supabase puede iniciar sesión desde `/login` usando solo email y contraseña.
- [ ] El formulario muestra un estado no reenviable mientras la autenticación está en curso.
- [ ] Credenciales rechazadas muestran un mensaje genérico que no revela si falló el email o la contraseña.
- [ ] Un fallo temporal de red muestra un mensaje de error temporal distinto al de credenciales.
- [ ] Una sesión autenticada con perfil `public.users` activo navega a la ruta de retorno interna solicitada o a `/` cuando no existe una ruta válida.
- [ ] Un `returnUrl` externo, vacío o malformado nunca redirige fuera de la aplicación.
- [ ] Una persona anónima que abre `/`, `/kids`, `/kids/{Slug}`, `/counter` o `/weather` termina en `/login` con la ruta original como retorno.
- [ ] Una persona autenticada que abre `/login` no ve el formulario y navega al retorno válido o a `/`.
- [ ] `/login`, `/activate-account`, `/not-found` y `/Error` permanecen accesibles sin sesión.
- [ ] `/activate-account` y el enlace `Activa tu cuenta` siguen siendo visuales y no crean cuentas ni sesiones.
- [ ] Tras recargar el navegador o abrir una nueva visita, una sesión válida se restaura y conserva el acceso a las rutas privadas.
- [ ] Si la sesión restaurada no tiene perfil en `public.users` o su perfil no está `active`, se elimina la sesión y se bloquea el acceso a rutas privadas.
- [ ] La barra lateral muestra el nombre completo, rol y guardería del perfil propio autenticado en vez de `Caro Giménez` y `Maestra · Soles`.
- [ ] Pulsar `Cerrar sesión` revoca o cierra la sesión de Supabase, elimina la sesión almacenada y redirige a `/login`.
- [ ] Tras cerrar sesión, abrir una ruta privada vuelve a redirigir a `/login`.

## Decisions

- **Sí:** solo inicio de sesión para cuentas ya existentes. La cuenta staff de SPEC 08 permite verificar el flujo sin crear un alta pública.
- **No:** registro o activación real. Invitaciones, altas y códigos requieren su propia especificación y controles de autorización adicionales.
- **Sí:** autenticación exclusivamente por email y contraseña. Es el alcance funcional acordado.
- **Sí:** persistencia en almacenamiento protegido del navegador. El SDK de Supabase C# no persiste sesiones por defecto y el usuario espera que sobrevivan recargas y visitas posteriores.
- **Sí:** restaurar el estado solo después de que Blazor sea interactivo. El almacenamiento del navegador no está disponible durante el prerenderizado.
- **Sí:** `AuthenticationStateProvider` y `AuthorizeRouteView`. Integran la sesión externa con la autorización de rutas durante la navegación interactiva de Blazor.
- **Sí:** rutas funcionales protegidas y lista explícita de rutas públicas. Evita que una ruta nueva quede accesible por omisión.
- **Sí:** volver a la ruta interna solicitada tras el login. Conserva la intención de navegación sin permitir redirecciones externas.
- **Sí:** mensaje genérico para credenciales y mensaje temporal para red. Protege información de cuentas y permite distinguir problemas recuperables.
- **Sí:** exigir un perfil activo en `public.users`. Una identidad válida de Auth sin perfil autorizado no puede acceder a la aplicación.
- **Sí:** mostrar el perfil propio real en la barra lateral. `public.users` ya dispone de RLS para su lectura por el usuario correspondiente.
- **No:** recuperación de contraseña. El enlace existente seguirá siendo visual hasta una especificación dedicada.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| El prerenderizado redirige antes de poder leer la sesión del navegador | Esperar a la conexión interactiva y a la restauración de sesión antes de renderizar el enrutado protegido. |
| Un token persistido ha caducado o fue revocado | Restaurar y validar la sesión con Supabase; borrar el estado local y tratar al usuario como anónimo si falla. |
| Una cuenta Auth no tiene perfil o está inactiva | Cerrar su sesión y mostrar un error genérico sin conceder acceso a rutas privadas. |
| `returnUrl` permite una redirección abierta | Aceptar únicamente rutas locales internas validadas antes de navegar. |
| El perfil no se puede cargar por RLS o conectividad | No conceder acceso y mostrar un error temporal o genérico según corresponda. |

## What is **not** in this spec

- Registro, invitaciones o activación funcional de cuentas.
- Recuperación, cambio o políticas de complejidad de contraseña.
- Permisos por rol, guardería, niño, publicación u otro recurso.
- Cambios de esquema, políticas RLS, migraciones o datos de Supabase.
- Proveedores sociales, magic links, MFA, passkeys o autenticación sin contraseña.
