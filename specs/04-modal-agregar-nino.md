# SPEC 04 — Modal para agregar niño

> **Status:** Approved
> **Depends on:** SPEC 02
> **Date:** 2026-08-22
> **Objective:** Añadir un modal temporal para registrar visualmente los datos de un niño desde `/kids`, validarlo y cerrarlo sin modificar el catálogo estático.

## Scope

**In:**

- Crear `Components/Shared/AddKidModal.razor` como modal interactivo basado en el mockup.
- Actualizar `Components/Pages/Kids.razor` para abrir el modal al pulsar `Agregar niño`.
- Mostrar campos para nombre completo, fecha de nacimiento, sala, alergias y notas médicas.
- Ofrecer `Soles`, `Estrellas` y `Arcoíris` en el selector de sala.
- Validar nombre, fecha de nacimiento y sala antes de permitir Guardar.
- Exigir una fecha de nacimiento válida y no futura.
- Cerrar el modal al guardar valores válidos, cancelar, pulsar el fondo o pulsar Escape.
- Reiniciar todos los valores y errores al cerrar el modal.
- Mantener alergias y notas médicas como datos temporales, sin guardarlos en el catálogo.
- Reproducir el modal con Tailwind, SVG inline y español de España.
- Mantener el modal legible y operable a 1440px y 390px.

**Out of scope (for future specs):**

- Crear, editar o persistir niños en `KidCatalog`.
- Actualizar el contador, la cuadrícula o los perfiles tras guardar.
- API, base de datos, localStorage o cualquier persistencia.
- Adjuntar documentos, fotografías o historiales médicos.
- Validar contenido clínico de alergias o notas médicas.
- Gestión real de salas.

## Data model

Esta funcionalidad no introduce modelos de dominio ni datos persistentes.

`AddKidModal.razor` mantendrá un borrador local:

```csharp
AddKidDraft
{
    string FullName,
    DateOnly? BirthDate,
    string Room,
    string Allergies,
    string MedicalNotes
}
```

El borrador solo existe mientras el modal está abierto y se restablece al cerrarse.

## Implementation plan

1. Crear `Components/Shared/AddKidModal.razor` con la estructura visual del diálogo, sus campos y acciones sin modificar `KidCatalog`.
2. Conectar `Kids.razor` con el estado de apertura y el botón `Agregar niño`; verificar que abre y cierra mediante Cancelar.
3. Añadir el borrador local, selector de fecha y selector de sala con Soles, Estrellas y Arcoíris.
4. Añadir validación local para nombre, fecha válida no futura y sala; Guardar solo cierra con datos válidos.
5. Añadir cierre por fondo y Escape, evitando que pulsar dentro del panel lo cierre, y restablecer borrador y errores al cerrar.
6. Ajustar responsividad, compilar Tailwind y verificar el flujo completo en navegador.

## Acceptance criteria

- [x] `dotnet build` termina sin errores.
- [x] Pulsar `Agregar niño` en `/kids` abre el modal sin navegar.
- [x] El modal muestra nombre completo, fecha de nacimiento, sala, alergias y notas médicas.
- [x] El selector Sala ofrece Soles, Estrellas y Arcoíris.
- [x] Nombre, fecha y sala son obligatorios.
- [x] Una fecha vacía, inválida o futura muestra un error local y mantiene el modal abierto.
- [x] Alergias y notas médicas son opcionales.
- [x] Guardar con datos válidos cierra el modal.
- [x] Guardar no añade tarjetas, no modifica el contador de niños y no cambia `KidCatalog`.
- [x] Cancelar, pulsar el fondo y Escape cierran el modal.
- [x] Pulsar dentro del panel no cierra el modal.
- [x] Al reabrir, los campos y errores están restablecidos.
- [x] A 1440px, el modal reproduce la composición del mockup sobre `/kids`.
- [x] A 390px, el modal permanece legible, navegable y sin desbordamiento horizontal.
- [x] No se realizan solicitudes de API ni se persisten datos.

## Decisions

- **Sí:** modal temporal sin alta de niños. El alcance acordado limita Guardar a validar y cerrar.
- **No:** modificar `KidCatalog`. El listado existente debe seguir con ocho niños.
- **Sí:** `AddKidModal.razor` separado. Mantiene `Kids.razor` centrada en el listado y el buscador.
- **Sí:** salas demo Soles, Estrellas y Arcoíris. Permiten ejercitar el selector sin modelo de salas.
- **Sí:** selector de fecha y fecha no futura. Evita analizar manualmente formatos de fecha.
- **Sí:** alergias separadas de notas, pero temporales. Mantiene los campos del mockup sin ampliar datos de dominio.
- **Sí:** Cancelar, fondo y Escape cierran y descartan. Son las acciones acordadas para un diálogo temporal.
- **No:** conservar el borrador al cerrar. Cada apertura empieza limpia.

## Risks

| Risk | Mitigation |
| --- | --- |
| El usuario interpreta Guardar como persistencia | Mantener la cuadrícula y contador sin cambios y documentar el alcance temporal. |
| El fondo cierra por un clic dentro del panel | Detener la propagación del evento del panel. |
| El modal se corta en móvil | Limitar altura, permitir desplazamiento interno y comprobarlo a 390px. |

## What is **not** in this spec

- Persistir, crear o editar niños.
- Modificar `KidCatalog`, tarjetas, contador o perfiles.
- API, base de datos o almacenamiento local.
- Fotos, documentos o datos médicos reales.
