# SPEC 03 — Login y activación de cuenta

> **Status:** Approved
> **Depends on:** SPEC 01
> **Date:** 2026-08-18
> **Objective:** Añadir las pantallas estáticas e interactivas de login y activación de cuenta en `/login` y `/activate-account` sin sidebar ni autenticación real.

## Scope

**In:**

- Crear `Components/Layout/AuthLayout.razor` como layout alternativo sin `Sidebar`.
- Crear `Components/Pages/Login.razor` para `/login` con el panel de marca coral de escritorio y el formulario del mockup.
- Omitir por completo el selector de ingreso `Personal` y `Familia`.
- Crear `Components/Pages/ActivateAccount.razor` para `/activate-account` con la tarjeta demo de `Mateo · Sala Soles`.
- Usar campos inicialmente vacíos con placeholders en ambos formularios.
- Validar localmente campos obligatorios y formato de email antes de enviar cada formulario.
- Mantener el consentimiento de fotos seleccionado inicialmente y exigirlo para activar la cuenta.
- Navegar a `/` al enviar formularios válidos.
- Conectar los enlaces entre login y activación sin bloquear la navegación por errores del formulario actual.
- Mantener el enlace de recuperación de contraseña únicamente como elemento visual.
- Reproducir el diseño de los mockups con Tailwind, tipografías existentes y SVG inline.
- Ocultar el panel coral de login en móvil para priorizar el formulario.

**Out of scope (for future specs):**

- Autenticación real, sesiones, roles, permisos o cierre de sesión.
- API, base de datos, persistencia local o remota.
- Verificación real del código de invitación.
- Recuperación de contraseña.
- Reglas de complejidad de contraseña.
- Sidebar, menú móvil o navegación de áreas autenticadas en estas rutas.

## Data model

Esta funcionalidad no introduce modelos de dominio ni datos persistentes.

`Login.razor` y `ActivateAccount.razor` mantendrán únicamente estado local interactivo para los valores de formulario, errores de validación y el consentimiento de fotos.

La tarjeta de invitación de activación declara contenido demo estático: `Mateo · Sala Soles`.

## Implementation plan

1. Crear `Components/Layout/AuthLayout.razor` con un contenedor de página sin `Sidebar` y conservar el manejo global de errores del layout actual.
2. Crear `Components/Pages/Login.razor` con la ruta `/login`, `AuthLayout`, el panel visual coral en escritorio y el formulario sin selector de roles.
3. Añadir el estado interactivo y la validación local de email y contraseña en `Login.razor`; al enviar datos válidos navegar a `/`.
4. Crear `Components/Pages/ActivateAccount.razor` con la ruta `/activate-account`, `AuthLayout`, el contenido de invitación demo y los campos vacíos del mockup.
5. Añadir el estado interactivo y la validación local de código, email, contraseña y consentimiento en `ActivateAccount.razor`; al enviar datos válidos navegar a `/`.
6. Conectar los enlaces entre `/login` y `/activate-account`, ajustar la responsividad a escritorio y móvil, y compilar el CSS con `npm run build:css`.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] `/login` muestra la pantalla de inicio de sesión sin sidebar.
- [ ] `/activate-account` muestra la pantalla de activación sin sidebar.
- [ ] El login no muestra opciones de ingreso `Personal` ni `Familia`.
- [ ] A 1440px, `/login` muestra el panel coral de marca y el formulario según el mockup.
- [ ] A 390px, `/login` oculta el panel coral y mantiene el formulario legible y usable.
- [ ] Los campos de login comienzan vacíos y muestran placeholders.
- [ ] En login, email vacío, email inválido o contraseña vacía muestran un error local y no navegan.
- [ ] En login, un email válido y una contraseña no vacía navegan a `/`.
- [ ] El enlace `Activá tu cuenta` navega a `/activate-account` aunque el login tenga campos inválidos.
- [ ] El enlace `¿Olvidaste tu contraseña?` no navega ni abre un flujo.
- [ ] La activación muestra la tarjeta estática `Mateo · Sala Soles`.
- [ ] Código, email y contraseña de activación comienzan vacíos.
- [ ] En activación, código vacío, email vacío o inválido, contraseña vacía o consentimiento desmarcado muestran errores locales y no navegan.
- [ ] El consentimiento de fotos inicia marcado y es obligatorio para activar.
- [ ] En activación, los valores válidos y el consentimiento marcado navegan a `/`.
- [ ] El enlace `Iniciar sesión` navega a `/login` aunque activación tenga campos inválidos.
- [ ] No se realizan solicitudes de autenticación, API ni persistencia al cargar, validar o enviar los formularios.

## Decisions

- **Sí:** rutas `/login` y `/activate-account`. Son las rutas acordadas para ambos flujos.
- **Sí:** `AuthLayout.razor` independiente. Evita renderizar el sidebar definido en `MainLayout`.
- **No:** selector de roles Personal y Familia. Se excluye expresamente del login.
- **Sí:** formularios vacíos con placeholders. Evitan presentar credenciales demo como datos reales.
- **Sí:** validación local de campos obligatorios y formato de email. Da feedback verificable sin implementar backend.
- **Sí:** contraseña no vacía sin requisitos de complejidad. Las reglas reales corresponden a autenticación.
- **Sí:** consentimiento inicialmente marcado y obligatorio. Conserva el estado visual del mockup y controla su requisito localmente.
- **Sí:** navegar a `/` tras envíos válidos. Representa el destino demo existente.
- **No:** verificación de invitación ni autenticación real. Requieren servicios, modelos y persistencia fuera de alcance.
- **Sí:** panel coral de login oculto bajo móvil. Prioriza el formulario en pantallas estrechas.

## Risks

| Risk | Mitigation |
| --- | --- |
| El flujo demo se interpreta como autenticación real | Mantenerlo sin API, persistencia ni verificación de credenciales. |
| Los formularios interactivos no se hidratan | Declarar las páginas con renderizado interactivo compatible con servidor. |
| El layout autenticado se aplique por defecto | Asignar explícitamente `AuthLayout` a ambas páginas. |

## What is **not** in this spec

- Autenticación, sesiones, permisos y roles reales.
- Recuperación de contraseña.
- Verificación de códigos de invitación.
- Persistencia, API o base de datos.
- Sidebar en login o activación.
