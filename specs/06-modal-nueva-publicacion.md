# SPEC 06 — Modal para nueva publicación

> **Status:** Approved
> **Depends on:** SPEC 01, SPEC 02
> **Date:** 2026-08-28
> **Objective:** Añadir un modal temporal para crear visualmente una publicación desde Nueva publicación sin persistirla ni modificar el feed estático.

## Scope

**In:**

- Crear `Components/Shared/CreatePostModal.razor` basado en `References/mockups/crear-publicacion.dc.html`.
- Actualizar `Components/Shared/Sidebar.razor` para emitir el evento al pulsar `Nueva publicación`.
- Actualizar `Components/Layout/MainLayout.razor` para abrir y cerrar el modal desde la barra lateral en las rutas con layout principal.
- Mostrar en el modal el encabezado con `Cancelar`, `Nueva publicación` y `Publicar`.
- Mostrar todos los niños de `KidCatalog.All` como chips de destinatarios con sus iniciales y colores de avatar.
- Ofrecer `Toda la sala` como destinatario exclusivo frente a los niños individuales.
- Permitir seleccionar varios niños individuales.
- Iniciar sin destinatarios seleccionados y exigir al menos un niño o `Toda la sala` para publicar.
- Ofrecer los tipos `Comida`, `Siesta`, `Actividad`, `Logro`, `Ánimo`, `Foto` y `Anuncio`, con `Comida` seleccionado inicialmente.
- Mostrar una descripción inicialmente vacía con el placeholder `Cuenta cómo le fue hoy…`.
- Exigir una descripción no vacía para publicar.
- Mantener la sección de fotos como composición visual estática, sin selector de archivos.
- Cerrar el modal al publicar valores válidos, cancelar, pulsar el fondo o pulsar Escape.
- Reiniciar destinatarios, tipo, descripción y errores al cerrar el modal.
- Reproducir el diseño con Tailwind, SVG inline y español de España.
- Mantener el modal legible y operable a 1440px y 390px.

**Out of scope (for future specs):**

- Crear, añadir o persistir publicaciones en el feed.
- Modificar `PostCard`, las publicaciones estáticas de `Home.razor` o el contador de publicaciones.
- API, base de datos, localStorage o cualquier persistencia.
- Subir, previsualizar, eliminar o almacenar fotografías.
- Publicar desde el disparador `Compartí un momento…` de `Home.razor`.
- Programar publicaciones, guardar borradores o enviar notificaciones.

## Data model

Esta funcionalidad no introduce modelos de dominio ni datos persistentes.

`CreatePostModal.razor` mantendrá un borrador local:

```csharp
CreatePostDraft
{
    IReadOnlySet<string> SelectedKidSlugs,
    bool IsRoomSelected,
    string Kind,
    string Description
}
```

El borrador inicia con `SelectedKidSlugs` vacío, `IsRoomSelected` en `false`, `Kind` como `"Comida"` y `Description` vacía.

## Implementation plan

1. Crear `Components/Shared/CreatePostModal.razor` con la estructura visual del diálogo, encabezado, sección de fotos estática y acciones sin modificar el feed.
2. Añadir al modal los chips de todos los niños de `KidCatalog.All`, el chip `Toda la sala` y la selección múltiple exclusiva entre niños y sala completa.
3. Añadir los siete tipos de publicación, el borrador local con `Comida` inicial y el campo de descripción vacío con placeholder en español de España.
4. Añadir validación local de destinatario y descripción; `Publicar` solo cerrará cuando no haya errores.
5. Añadir cierre mediante Cancelar, fondo y Escape, impedir que un clic dentro del panel cierre el modal, restaurar el foco al abrir y reiniciar el borrador y errores al cerrar.
6. Actualizar `Sidebar.razor` con un `EventCallback` para `Nueva publicación` y actualizar `MainLayout.razor` con renderizado interactivo, estado de apertura y el modal global.
7. Ajustar responsividad, compilar Tailwind y verificar el flujo completo en navegador.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] Pulsar `Nueva publicación` en la barra lateral abre el modal sin navegar.
- [ ] El disparador `Compartí un momento…` de `/` no abre el modal.
- [ ] El encabezado muestra `Cancelar`, `Nueva publicación` y `Publicar`.
- [ ] El modal ofrece un chip por cada niño de `KidCatalog.All` con sus iniciales y colores de avatar.
- [ ] El modal ofrece el chip `Toda la sala`.
- [ ] Se pueden seleccionar varios niños individuales.
- [ ] Seleccionar `Toda la sala` deselecciona los niños y seleccionar un niño deselecciona `Toda la sala`.
- [ ] El modal inicia sin destinatarios seleccionados.
- [ ] El modal ofrece exactamente `Comida`, `Siesta`, `Actividad`, `Logro`, `Ánimo`, `Foto` y `Anuncio`.
- [ ] `Comida` inicia como tipo seleccionado y se puede cambiar a cualquiera de los otros tipos.
- [ ] La descripción inicia vacía y muestra el placeholder `Cuenta cómo le fue hoy…`.
- [ ] La sección Fotos muestra la composición visual del mockup y no abre un selector de archivos.
- [ ] Publicar sin destinatario muestra un error local y mantiene abierto el modal.
- [ ] Publicar sin descripción muestra un error local y mantiene abierto el modal.
- [ ] Publicar con uno o varios destinatarios y una descripción no vacía cierra el modal.
- [ ] Publicar con datos válidos no añade una tarjeta, no modifica `Home.razor`, no realiza solicitudes de API y no persiste datos.
- [ ] Cancelar, pulsar el fondo y Escape cierran el modal.
- [ ] Pulsar dentro del panel no cierra el modal.
- [ ] Al reabrir, no hay destinatarios seleccionados, `Comida` está seleccionado, la descripción está vacía y no se muestran errores anteriores.
- [ ] A 1440px, el modal reproduce la composición del mockup sobre una ruta con `MainLayout`.
- [ ] A 390px, el modal permanece legible, navegable y sin desbordamiento horizontal.
- [ ] Al abrir el modal, recibe el foco de teclado para que Escape pueda cerrarlo.

## Decisions

- **Sí:** `CreatePostModal.razor` separado. Mantiene el layout y la barra lateral centrados en su responsabilidad de composición y navegación.
- **Sí:** apertura desde `Nueva publicación` de `Sidebar.razor`. Es el disparador solicitado y está disponible en las rutas con `MainLayout`.
- **No:** abrir desde `Compartí un momento…` de `Home.razor`. Se excluye expresamente de este alcance.
- **Sí:** todos los niños de `KidCatalog.All`. Evita mantener una segunda lista de destinatarios y conserva los avatares del catálogo.
- **Sí:** selección múltiple de niños y `Toda la sala` exclusiva. Expresa correctamente publicaciones individuales o generales sin combinaciones redundantes.
- **Sí:** ningún destinatario inicial y validación obligatoria. Evita publicar accidentalmente para una familia o toda la sala.
- **Sí:** `Comida` como tipo inicial. Conserva el estado visual definido por el mockup.
- **Sí:** descripción vacía con `Cuenta cómo le fue hoy…`. Es un flujo de creación y usa español de España.
- **Sí:** fotos solo visuales. El mockup se reproduce sin ampliar el alcance a carga de archivos.
- **Sí:** publicar válido solo cierra y descarta. Mantiene el patrón temporal de los modales anteriores.
- **No:** actualizar el feed. Requiere un modelo de publicación y persistencia fuera de alcance.

## Risks

| Risk | Mitigation |
| --- | --- |
| Los chips de todos los niños desbordan en móvil | Permitir que los chips envuelvan líneas y comprobarlo a 390px. |
| El usuario interpreta Publicar como una publicación real | Mantener `Home.razor` sin cambios y no realizar solicitudes ni persistencia. |
| El modal global no recibe eventos de teclado | Declarar el layout con renderizado interactivo compatible con servidor y enfocar el fondo al abrir. |

## What is **not** in this spec

- Persistir, crear o añadir publicaciones al feed.
- API, base de datos, almacenamiento local o notificaciones.
- Carga, previsualización o gestión de fotografías.
- Apertura desde `Compartí un momento…`.
- Borradores, programación o edición de publicaciones.
