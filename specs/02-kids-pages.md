# SPEC 02 — Páginas de niños

> **Status:** Approved
> **Depends on:** SPEC 01
> **Date:** 2026-08-18
> **Objective:** Añadir un listado y perfiles estáticos de niños en las rutas `/kids` y `/kids/[slug]` que reproduzcan los mockups proporcionados.

## Scope

**In:**

- Crear el componente reutilizable `Components/Shared/KidCard.razor` para renderizar cada tarjeta de niño.
- Crear la página `Components/Pages/Kids.razor` para la ruta `/kids` con el encabezado, botón visual de agregar, buscador y cuadrícula de ocho `KidCard`.
- Crear la página `Components/Pages/KidProfile.razor` para la ruta `/kids/[slug]` con el perfil completo de cada niño conocido.
- Centralizar los datos demo en `Components/Shared/KidCatalog.cs` para que listado y perfiles compartan nombres, slugs, avatares, edades, alertas, fechas y padres vinculados.
- Implementar el filtrado local, sin distinción entre mayúsculas y minúsculas, por nombre completo desde el buscador de `/kids`.
- Mostrar un estado vacío visible cuando la búsqueda no tenga coincidencias.
- Hacer que cada tarjeta del listado navegue al slug correspondiente.
- Reproducir para Mateo los datos y la jerarquía visual del mockup de perfil.
- Completar los otros siete perfiles con datos demo coherentes, incluida la fecha de nacimiento, fecha de ingreso, notas, padres y estados de invitación.
- Mostrar la tarjeta de alertas para todos los perfiles, con el texto `Sin alergias ni notas registradas` cuando el niño no tenga notas.
- Navegar desde el enlace de retorno de cada perfil hacia `/kids`.
- Actualizar `Components/Shared/Sidebar.razor` para que Feed enlace a `/`, Niños enlace a `/kids` y ambos indiquen la ruta activa.
- Mantener el diseño de escritorio y el menú móvil existentes de SPEC 01, usando Tailwind y SVG inline.
- Mostrar la página `NotFound` existente al visitar un slug que no esté en el catálogo.

**Out of scope (for future specs):**

- Base de datos, API, autenticación o persistencia local de niños y padres.
- Crear, editar o eliminar niños.
- Enviar invitaciones, vincular padres o modificar sus estados.
- Generar o mostrar un resumen funcional del día.
- Fotos reales de niños, carga de archivos o gestión de imágenes.
- Rutas y flujos para avisos, cuenta, publicaciones u otras áreas del sidebar.

## Data model

`Components/Shared/KidCatalog.cs` contendrá las estructuras estáticas compartidas:

```csharp
Kid
{
    string Slug,
    string FullName,
    string Initials,
    string AvatarBackgroundClass,
    string AvatarTextClass,
    int Age,
    string Room,
    DateOnly BirthDate,
    string EnrollmentLabel,
    string? Notes,
    IReadOnlyList<LinkedParent> Parents
}

LinkedParent
{
    string FullName,
    string Initials,
    string Relationship,
    string InvitationStatus,
    string AvatarBackgroundClass,
    string StatusBackgroundClass,
    string StatusTextClass
}
```

El catálogo declara exactamente ocho niños: Mateo Fernández, Sofía Méndez, Benjamín Ruiz, Valentina Soto, Tomás Díaz, Emma Castro, Lucas Romero y Olivia Vega.

`KidCatalog` expone la colección para el listado y una búsqueda por `Slug` para el perfil. Mateo conserva los datos explícitos del mockup. Los demás datos de detalle son demo estáticos y no se modifican durante la sesión.

## Implementation plan

1. Crear `Components/Shared/KidCatalog.cs` con `Kid`, `LinkedParent`, los ocho registros demo y el acceso por slug; verificar que el proyecto compila.
2. Crear `Components/Shared/KidCard.razor` para encapsular avatar, nombre, edad, resumen de padres, etiqueta y enlace de cada niño, y crear `Components/Pages/Kids.razor` con la ruta `/kids` que renderice ocho `KidCard`; verificar que `/kids` muestra los ocho niños.
3. Añadir el estado interactivo del buscador a `Kids.razor`, aplicar el filtro local por nombre y renderizar un `KidCard` por cada coincidencia, o un mensaje de estado vacío cuando no haya ninguna; verificar ambos resultados desde el navegador.
4. Crear `Components/Pages/KidProfile.razor` con la ruta `/kids/{Slug}`, la consulta a `KidCatalog`, el contenido completo de perfil y el retorno a `/kids`; verificar el detalle de Mateo y de otro niño conocido.
5. Conectar un slug no encontrado con la página `NotFound` existente; verificar que `/kids/no-existe` no muestra un perfil inventado.
6. Actualizar `Components/Shared/Sidebar.razor` para convertir Feed y Niños en enlaces y aplicar el estilo activo según la ruta actual; verificar la navegación desde escritorio y el panel móvil.
7. Ajustar las clases Tailwind de las nuevas páginas para reproducir ambos mockups a 1440px y mantener la legibilidad a 390px, sin alterar el marco responsivo de SPEC 01.

## Acceptance criteria

- [x] `dotnet build` termina sin errores.
- [x] La ruta `/kids` renderiza el título Niños, el buscador, el botón Agregar niño visual y las ocho tarjetas del mockup.
- [x] `KidCard.razor` recibe un `Kid` y encapsula el avatar, metadatos, etiqueta y enlace de cada tarjeta sin duplicar ese marcado en `Kids.razor`.
- [x] Cada tarjeta de `/kids` navega a `/kids/[slug]` del niño correspondiente.
- [x] Escribir una parte del nombre en el buscador filtra las tarjetas sin recargar la página.
- [x] Una búsqueda sin coincidencias muestra `No se encontraron niños` y no muestra tarjetas.
- [x] `/kids/mateo-fernandez` muestra el perfil de Mateo con sus datos del mockup, alerta de maní, notas y dos padres vinculados.
- [x] Cada uno de los otros siete slugs conocidos muestra nombre, edad, sala, fechas, tarjeta de notas y padres vinculados propios.
- [x] Los niños sin notas muestran `Sin alergias ni notas registradas` dentro de la tarjeta de alertas.
- [x] El enlace Volver a Niños de un perfil navega a `/kids`.
- [x] `/kids/no-existe` muestra la página NotFound y no muestra datos de ningún niño.
- [x] El sidebar enlaza Feed a `/` y Niños a `/kids`, y marca Feed como activo en `/` y Niños como activo en `/kids` y `/kids/[slug]`.
- [x] Agregar niño, Editar, Resumen del día y Vincular otro padre son elementos visuales sin navegación ni cambios de datos.
- [x] A 1440px, las páginas mantienen sidebar fijo, contenido centrado y la composición de los mockups.
- [x] A 390px, la lista, los detalles y el menú lateral siguen siendo legibles y navegables.
- [x] No se realizan solicitudes de API, autenticación ni persistencia al cargar, buscar o navegar entre perfiles.

## Decisions

- **Sí:** catálogo C# estático compartido en `Components/Shared/KidCatalog.cs`. Evita duplicar los datos entre listado y detalle sin introducir una capa de persistencia.
- **Sí:** `KidCard` reutilizable en `Components/Shared/`. Mantiene `Kids.razor` enfocada en la búsqueda y la composición del listado.
- **No:** datos declarados por separado en cada página Razor. Duplicarían información y permitirían que el listado y los perfiles difirieran.
- **Sí:** un perfil completo para cada uno de los ocho niños. La ruta dinámica representa un recurso concreto y no solo el detalle de Mateo.
- **Sí:** datos demo coherentes para la información no incluida en los mockups de los otros siete niños. Permiten completar la interfaz estática acordada.
- **Sí:** filtro local por nombre y estado vacío explícito. Hace funcional el buscador sin requerir API.
- **Sí:** página NotFound para slugs desconocidos. Evita presentar un perfil incorrecto o redirigir silenciosamente.
- **Sí:** conservar visibles las tarjetas de notas sin alertas. Mantiene una estructura uniforme en todos los perfiles.
- **Sí:** conectar Feed y Niños en el sidebar y calcular el estado activo. Integra las nuevas rutas con la navegación existente.
- **No:** implementar agregar, editar, resumen o vinculación de padres. Son flujos independientes que requieren modelo de escritura y persistencia.

## Risks

| Risk | Mitigation |
| --- | --- |
| Los datos demo se interpretan como información real | Mantener el alcance sin API, autenticación ni edición, y concentrar todos los registros estáticos en el catálogo. |
| Un slug dinámico desconocido intenta renderizar datos nulos | Resolver el niño por slug antes de renderizar el perfil y mostrar NotFound si no existe. |
| El listado de tarjetas pierde legibilidad en móvil | Usar una columna en pantallas estrechas y reutilizar el comportamiento móvil del sidebar de SPEC 01. |
| El estado activo del sidebar diverge de la ruta | Determinarlo desde la ruta actual, no desde un estado local del componente. |

## What is **not** in this spec

- Persistencia, API, base de datos, autenticación o permisos.
- Altas, edición, eliminación o cambios de los niños y padres.
- Invitaciones funcionales y vinculación de padres.
- Un resumen funcional del día.
- Imágenes reales, carga de archivos y nuevas áreas de producto.
