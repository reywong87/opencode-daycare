# SPEC 05 — Modal para vincular padre

> **Status:** Approved
> **Depends on:** SPEC 02, SPEC 04
> **Date:** 2026-08-26
> **Objective:** Añadir un modal temporal para validar visualmente la invitación de un padre desde el perfil de un niño sin enviar correo ni modificar sus padres vinculados.

## Scope

**In:**

- Crear `Components/Shared/LinkParentModal.razor` como modal interactivo basado en `References/mockups/vincular-padre.dc.html`.
- Actualizar `Components/Pages/KidProfile.razor` para abrir el modal desde el botón `Vincular otro padre` de `/kids/{Slug}`.
- Mostrar en el encabezado el nombre dinámico del niño actual, por ejemplo, `a Mateo Fernández`.
- Mostrar campos para nombre del padre o madre y email, ambos inicialmente vacíos y con sus placeholders del mockup.
- Ofrecer `Mamá`, `Papá` y `Tutor/a` como parentesco, con `Mamá` seleccionado inicialmente.
- Mostrar el código fijo `7K4P9` y el texto `Vence en 7 días`.
- Validar localmente que nombre y email estén informados y que el email tenga formato válido antes de enviar.
- Cerrar el modal al enviar valores válidos, al pulsar X, al pulsar el fondo o al pulsar Escape.
- Restablecer campos, parentesco y errores al cerrar el modal.
- Reproducir el modal con Tailwind, SVG inline y español de España.
- Mantener el modal legible y operable a 1440px y 390px, conservando el foco de teclado al abrirlo.

**Out of scope (for future specs):**

- Enviar correo, crear códigos de invitación o integrar un servicio de email.
- Persistir invitaciones, padres o parentescos.
- Añadir una entrada pendiente o modificar `Kid.Parents` tras enviar.
- API, base de datos, localStorage o cualquier persistencia.
- Activar cuentas, validar códigos de invitación o aplicar permisos de acceso al feed.
- Editar o eliminar padres vinculados existentes.

## Data model

Esta funcionalidad no introduce modelos de dominio ni datos persistentes.

`LinkParentModal.razor` mantendrá un borrador local:

```csharp
LinkParentDraft
{
    string FullName,
    string Email,
    string Relationship
}
```

El borrador solo existe mientras el modal está abierto, `Relationship` inicia como `"Mamá"` y se restablece al cerrarse.

## Implementation plan

1. Crear `Components/Shared/LinkParentModal.razor` con el fondo, diálogo, encabezado dinámico, aviso informativo, formulario, selector de parentesco, código fijo y acción visual del mockup.
2. Añadir al modal el borrador local con nombre, email y parentesco, e iniciar el parentesco en `Mamá` en cada apertura.
3. Añadir validación local de nombre obligatorio, email obligatorio y formato válido; `Enviar invitación` solo cerrará al no haber errores.
4. Añadir cierre mediante X, fondo y Escape, impedir que un clic dentro del panel cierre el modal, restaurar el foco al abrir y restablecer borrador y errores al cerrarlo.
5. Actualizar `Components/Pages/KidProfile.razor` con renderizado interactivo, estado de apertura y el enlace del botón `Vincular otro padre` al modal, pasando el nombre de `CurrentKid` y sin modificar `kid.Parents`.
6. Ajustar la composición responsive del modal a escritorio y móvil, compilar los estilos y verificar el flujo en navegador.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] Pulsar `Vincular otro padre` en una ruta válida `/kids/{Slug}` abre el modal sin navegar.
- [ ] El encabezado del modal muestra `Vincular padre` y el nombre del niño de la ficha actual.
- [ ] El modal muestra el aviso sobre el correo de activación y que el padre solo verá el feed del niño actual.
- [ ] Nombre y email comienzan vacíos y muestran los placeholders `Ej. Diego Fernández` y `correo@ejemplo.com`.
- [ ] El selector ofrece exactamente `Mamá`, `Papá` y `Tutor/a`, y `Mamá` inicia seleccionado.
- [ ] El modal muestra el código fijo `7K4P9` y `Vence en 7 días`.
- [ ] Enviar con nombre vacío, email vacío o email inválido muestra errores locales y mantiene abierto el modal.
- [ ] Enviar con nombre no vacío y email válido cierra el modal.
- [ ] Enviar con datos válidos no envía correo, no realiza solicitudes de API y no persiste datos.
- [ ] Enviar con datos válidos no añade padres, invitaciones ni estados pendientes a `PADRES VINCULADOS`.
- [ ] Pulsar X, pulsar el fondo o pulsar Escape cierra el modal.
- [ ] Pulsar dentro del panel no cierra el modal.
- [ ] Al reabrir el modal, nombre y email están vacíos, `Mamá` está seleccionada y no se muestran errores anteriores.
- [ ] A 1440px, el modal reproduce la composición del mockup sobre el perfil del niño.
- [ ] A 390px, el modal permanece legible, navegable y sin desbordamiento horizontal.
- [ ] Al abrir el modal, recibe el foco de teclado para que Escape pueda cerrarlo.

## Decisions

- **Sí:** `LinkParentModal.razor` separado. Sigue la composición reutilizable del modal de alta de niño y mantiene `KidProfile.razor` centrada en la ficha.
- **Sí:** abrir desde `Vincular otro padre` en `/kids/{Slug}`. Es el punto de entrada existente en el perfil individual.
- **Sí:** nombre dinámico del niño en el subtítulo. El contexto del perfil determina a quién se vincula la invitación.
- **Sí:** código fijo `7K4P9`. El flujo es visual y no requiere generar ni verificar códigos.
- **Sí:** nombre y email obligatorios con validación local del formato de email. Proporciona feedback verificable sin backend.
- **Sí:** `Mamá` como parentesco inicial. Es la selección definida para cada apertura del modal.
- **Sí:** cerrar tras una validación correcta sin efectos persistentes. Conserva el alcance temporal del flujo.
- **No:** modificar `Kid.Parents` o mostrar una invitación pendiente. Requeriría un modelo y persistencia fuera de alcance.
- **No:** envío de email, API o generación de invitaciones. Corresponden a un flujo de invitación real posterior.
- **Sí:** cierre por X, fondo y Escape con restablecimiento. Descarta el borrador temporal de forma consistente con SPEC 04.

## Risks

| Risk | Mitigation |
| --- | --- |
| El usuario interpreta Enviar invitación como un envío real | No modificar padres ni invitaciones, y mantener el flujo sin solicitudes ni persistencia. |
| El modal no recibe eventos de teclado | Declarar el perfil y el modal con renderizado interactivo compatible con servidor y enfocar el fondo al abrir. |
| El modal se corta en móvil | Limitar el ancho, permitir desplazamiento vertical del fondo y comprobarlo a 390px. |

## What is **not** in this spec

- Enviar correos o integrar proveedores de email.
- Crear, validar o persistir códigos e invitaciones.
- Modificar `Kid.Parents`, mostrar estados pendientes o cambiar los padres vinculados.
- API, base de datos o almacenamiento local.
- Activación de cuentas, permisos o acceso real al feed.
