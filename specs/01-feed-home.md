# SPEC 01 — Home de feed con Tailwind

> **Status:** Implemented
> **Depends on:** Ninguna
> **Date:** 2026-08-16
> **Objective:** Reemplazar el home inicial por un feed estático que reproduzca el mockup proporcionado con Tailwind CSS y componentes Blazor reutilizables.

## Scope

**In:**

- Configurar Tailwind CSS v4 mediante CLI local y scripts npm para generar `wwwroot/app.css` desde `Styles/tailwind.css`.
- Sustituir Bootstrap y los estilos globales iniciales por la salida de Tailwind.
- Cargar las fuentes remotas Fredoka y Nunito desde Google Fonts en `Components/App.razor`.
- Reemplazar el layout inicial por el marco visual del mockup: sidebar de 248px en escritorio, contenido de feed centrado y fondo, colores, tipografías, tarjetas y espaciados equivalentes.
- Mostrar en `Components/Pages/Home.razor` los tres posts estáticos del mockup con sus textos, contadores, etiquetas y placeholder de foto.
- Crear `Components/Shared/Sidebar.razor`, `Components/Shared/PostCard.razor` y `Components/Shared/Avatar.razor` para las piezas visuales repetidas.
- Implementar navegación móvil bajo 768px con barra superior, botón de menú, panel lateral superpuesto y overlay que cierra el panel.
- Usar SVG inline para los iconos y CSS propio mínimo solo para reglas que no puedan expresarse limpiamente con utilidades Tailwind.

**Out of scope (for future specs):**

- Autenticación, perfiles reales o cierre de sesión.
- Base de datos, API, modelos de dominio o persistencia entre sesiones.
- Rutas nuevas para publicaciones, niños, avisos, cuenta, fotos o acciones del feed.
- Acciones funcionales de publicar, editar, reaccionar o comentar.
- Rediseñar el contenido de las páginas existentes `Counter` y `Weather`.
- Reemplazar el placeholder de foto por una imagen real.

## Data model

Esta funcionalidad no introduce modelos de dominio ni datos persistentes. El contenido del feed queda declarado de forma estática en `Home.razor` y se pasa a componentes de presentación.

Las interfaces visuales serán:

- `Avatar`: iniciales y tono de color.
- `PostCard`: nombre o título, subtítulo, tipo de publicación, destinatario, mensaje, etiqueta opcional de foto y contadores de reacciones y comentarios.
- `Sidebar`: enlaces y botones solamente visuales; el control de menú móvil es la única interacción de interfaz.

## Implementation plan

1. Añadir `package.json` y `package-lock.json` con `tailwindcss` y `@tailwindcss/cli` como dependencias de desarrollo, más los scripts `build:css` y `watch:css`.
2. Crear `Styles/tailwind.css` con la importación de Tailwind, el registro explícito de `Components/` como fuente de clases Razor y el CSS excepcional mínimo; compilarlo hacia `wwwroot/app.css` mediante `npm run build:css`.
3. Actualizar `Components/App.razor` para eliminar la hoja de Bootstrap, conservar la referencia a `app.css` compilada y añadir las preconexiones y la carga de Fredoka y Nunito desde Google Fonts.
4. Reemplazar `Components/Layout/MainLayout.razor` por la estructura del layout del feed y migrar sus reglas visuales a clases Tailwind; eliminar `Components/Layout/MainLayout.razor.css` cuando deje de ser necesaria.
5. Crear `Components/Shared/Avatar.razor` y `Components/Shared/PostCard.razor` con los estilos y SVG inline reutilizables para los avatares, tipos de post, métricas y placeholder de foto.
6. Crear `Components/Shared/Sidebar.razor` con la navegación visual de escritorio y la variante móvil a partir de 768px, incluido el botón de menú y el overlay de cierre.
7. Retirar `Components/Layout/NavMenu.razor` y `Components/Layout/NavMenu.razor.css`, y conectar `Sidebar` desde el nuevo layout sin añadir rutas para sus enlaces.
8. Reescribir `Components/Pages/Home.razor` con el encabezado, el compositor visual y los tres `PostCard` estáticos del mockup; eliminar los estilos o marcado de plantilla que ya no se usen.

## Acceptance criteria

- [x] `npm install` instala las dependencias de Tailwind y `npm run build:css` genera `wwwroot/app.css` sin errores.
- [x] `dotnet build` termina sin errores después de generar el CSS.
- [x] `Components/App.razor` no carga Bootstrap y carga Fredoka y Nunito desde Google Fonts.
- [x] A 1440px de ancho, el home muestra el sidebar fijo de 248px, el feed centrado y los tres posts estáticos con la jerarquía visual, colores, tipografías y placeholder del mockup.
- [x] A 390px de ancho, el sidebar no ocupa espacio permanente, se muestra una barra superior con logo y menú, y el feed continúa siendo legible.
- [x] En móvil, el botón de menú abre el panel lateral sobre el feed y tocar el overlay lo cierra.
- [x] Los enlaces y botones de publicación, edición, reacciones, comentarios y cierre de sesión no navegan a rutas inexistentes ni cambian datos.
- [x] Los iconos del home y sidebar se renderizan como SVG inline sin una biblioteca externa de iconos.
- [x] Al recargar el home, se mantiene el mismo contenido estático y no se realiza ninguna solicitud de autenticación, API o persistencia.
- [x] `Counter` y `Weather` conservan su contenido actual dentro del nuevo layout y no reciben rediseño de contenido.

## Decisions

- **Sí:** Tailwind CSS v4 mediante CLI local. Genera CSS estático compatible con Blazor y evita cargar Tailwind en tiempo de ejecución.
- **No:** Tailwind mediante CDN. Añadiría una dependencia de red en tiempo de ejecución y no es una base adecuada para una app publicada.
- **Sí:** scripts npm explícitos para compilar el CSS. Mantienen el proceso de `dotnet build` sin acoplarlo a npm.
- **No:** ejecutar npm automáticamente desde MSBuild. Añade complejidad innecesaria a la compilación .NET para este alcance visual.
- **Sí:** Tailwind como base única de estilos y eliminación de Bootstrap. Evita conflictos de utilidades y estilos de la plantilla inicial.
- **Sí:** CSS propio mínimo en `Styles/tailwind.css`. Se reserva para reglas excepcionales que las utilidades no expresen con claridad.
- **Sí:** `Sidebar`, `PostCard` y `Avatar` en `Components/Shared/`. Son piezas repetibles que mantienen `Home.razor` enfocado en componer el feed.
- **No:** extraer cada sección o icono como componente. Aumentaría la complejidad sin reutilización real.
- **Sí:** SVG inline. Permite igualar los iconos del mockup sin añadir otra dependencia.
- **Sí:** contenido estático sin autenticación, base de datos ni persistencia. Es el alcance explícito del home actual.
- **Sí:** breakpoint de 768px. Coincide con `md` de Tailwind y ofrece espacio suficiente para el feed en escritorio.

## Risks

| Risk | Mitigation |
| --- | --- |
| Google Fonts no está disponible | Las familias de respaldo del CSS mantienen el home utilizable, aunque la coincidencia visual será menor. |
| No se ejecuta `npm run build:css` después de cambiar clases Razor | Documentar el script en `package.json` y usarlo antes de `dotnet build` durante la verificación. |
| Tailwind no detecta clases usadas en componentes Razor | Registrar explícitamente `Components/` con `@source` en `Styles/tailwind.css`. |
| El contenido estático se interpreta como funcional | Mantener los elementos no interactivos sin destinos de navegación y limitar la interacción al menú móvil. |

## What is **not** in this spec

- Autenticación, sesiones, perfiles reales o permisos.
- Persistencia local o remota, API y base de datos.
- Rutas nuevas y acciones reales de las publicaciones.
- Una imagen real para la publicación de actividad.
- Rediseñar el contenido de `Counter` y `Weather`.
