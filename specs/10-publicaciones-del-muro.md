# SPEC 10 — Publicaciones del muro

> **Status:** Approved
> **Depends on:** SPEC 06, SPEC 07
> **Date:** 2026-09-17
> **Objective:** Permitir que staff y administradores publiquen, editen y eliminen entradas con texto obligatorio e imágenes opcionales en el muro de su guardería.

## Scope

**In:**

- Reemplazar el flujo visual de `CreatePostModal.razor` por la creación persistida de una entrada del muro de la guardería autenticada.
- Mostrar en `/` las entradas persistidas de la guardería, ordenadas por `published_at` descendente.
- Mantener los tipos `Comida`, `Siesta`, `Actividad`, `Logro`, `Ánimo`, `Foto` y `Anuncio`.
- Exigir texto no vacío de hasta 1.000 caracteres para crear o editar una entrada.
- Permitir adjuntar de cero a cinco imágenes JPG, PNG o WebP de hasta 5 MB cada una.
- Guardar imágenes en un bucket privado de Supabase Storage y mostrarlas solo a perfiles activos autorizados de la misma guardería.
- Mostrar en cada entrada el nombre del autor y la fecha y hora de publicación.
- Permitir a `staff` y `admin` crear entradas, y a su autor editar texto, tipo e imágenes o eliminar la entrada.
- Solicitar confirmación antes de eliminar una entrada y borrar sus archivos de Storage junto con sus registros persistidos.
- Permitir leer el muro a perfiles activos `staff`, `admin` y `parent` de la misma guardería.
- Mostrar estados de carga, muro vacío, error recuperable, envío en curso y errores de validación o persistencia.

**Out of scope (for future specs):**

- Destinatarios por niño, sala, familia o relaciones `post_children`.
- Reacciones, comentarios, notificaciones, borradores, programación o moderación.
- Publicar, editar o eliminar entradas para perfiles `parent`.
- Álbumes, edición de imágenes, vídeos, documentos o imágenes de más de 5 MB.
- Publicaciones entre guarderías, URLs públicas de imágenes o compartir archivos fuera de la aplicación.

## Data model

La migración sustituirá el uso futuro del modelo de referencia de publicaciones por entradas de muro asociadas directamente a una guardería. No creará `post_children` ni una relación con `rooms`.

```sql
alter type public.post_type add value 'mood';

create table public.posts (
    id uuid primary key default gen_random_uuid(),
    daycare_id uuid not null references public.daycares(id) on delete cascade,
    author_id uuid not null references public.users(id) on delete restrict,
    type public.post_type not null,
    body text not null check (length(btrim(body)) between 1 and 1000),
    published_at timestamptz not null default now(),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table public.post_photos (
    id uuid primary key default gen_random_uuid(),
    post_id uuid not null references public.posts(id) on delete cascade,
    storage_path text not null unique,
    position smallint not null check (position between 0 and 4),
    created_at timestamptz not null default now(),
    unique (post_id, position)
);
```

La migración creará el bucket privado `post-images`. Cada archivo usará la ruta `{daycare_id}/{post_id}/{photo_id}.{extension}`. `post_photos.storage_path` será la única referencia al archivo. No se persistirán URLs públicas ni URLs firmadas.

La aplicación añadirá los contratos siguientes:

```csharp
CreatePostRequest
{
    string Type,
    string Body,
    IReadOnlyList<IBrowserFile> NewPhotos
}

UpdatePostRequest
{
    Guid Id,
    string Type,
    string Body,
    IReadOnlyList<Guid> RetainedPhotoIds,
    IReadOnlyList<IBrowserFile> NewPhotos
}

PostFeedItem
{
    Guid Id,
    string AuthorName,
    Guid AuthorId,
    string Type,
    string Body,
    DateTimeOffset PublishedAt,
    IReadOnlyList<PostPhotoItem> Photos
}

PostPhotoItem
{
    Guid Id,
    string SignedUrl,
    int Position
}
```

`PostService` validará localmente tipo, texto, cantidad, MIME y tamaño antes de crear o modificar. Tras las comprobaciones, persistirá las filas mediante el cliente C# de Supabase y resolverá URLs firmadas de corta duración solo al cargar el feed autorizado. La base de datos y las políticas RLS serán la fuente de autorización; la interfaz no decidirá permisos.

Las políticas RLS de `posts` y `post_photos` permitirán `SELECT` a perfiles activos de la misma `daycare_id`. Permitirán `INSERT` solo si `author_id = auth.uid()` y el perfil activo propio tiene rol `staff` o `admin` en la `daycare_id` indicada. Permitirán `UPDATE` y `DELETE` exclusivamente cuando el autor autenticado sigue siendo `staff` o `admin` activo de esa guardería. Las políticas de `storage.objects` del bucket `post-images` aplicarán el mismo aislamiento por primer segmento de ruta (`daycare_id`) y restringirán insertar, actualizar y borrar al autor de la entrada relacionada; la lectura quedará limitada a perfiles activos de esa guardería.

## Implementation plan

1. Crear una migración imperativa en `supabase/migrations/` que añada `mood` a `public.post_type`, cree `public.posts`, `public.post_photos`, sus índices de lectura por guardería y fecha, el trigger `updated_at`, RLS, privilegios y políticas de acceso por guardería y autor.
2. Añadir en la misma migración el bucket privado `post-images` y las políticas de `storage.objects` que validen la guardería de la ruta, la pertenencia de la entrada y la autoría en operaciones de escritura.
3. Crear `Models/Post.cs` y `Models/PostPhoto.cs` con sus mapeos PostgREST, y añadir `Dtos/CreatePostRequest.cs`, `Dtos/UpdatePostRequest.cs`, `Dtos/PostFeedItem.cs` y `Dtos/PostPhotoItem.cs`.
4. Crear `Services/PostService.cs` para cargar el feed autorizado, crear entradas con imágenes, editar una entrada propia y borrar una entrada propia con sus archivos, registrando errores técnicos sin exponerlos al usuario.
5. Registrar `PostService` en `Program.cs` y mantener el cliente Supabase autenticado para que las políticas RLS y de Storage se evalúen con la sesión actual.
6. Actualizar `CreatePostModal.razor` para seleccionar, previsualizar y quitar hasta cinco imágenes válidas, validar texto y archivos, bloquear dobles envíos y conservar el borrador ante un fallo recuperable.
7. Añadir o actualizar el modal de edición para cargar texto, tipo e imágenes existentes de la entrada propia, conservar las imágenes marcadas y reemplazar o eliminar imágenes hasta el máximo de cinco.
8. Actualizar `PostComposer.razor` y `MainLayout.razor` para abrir la creación o edición y recargar el muro tras una operación correcta sin recargar el navegador.
9. Actualizar `Home.razor` para cargar el feed desde `PostService`, renderizar entradas reales con autor, fecha, tipo, texto y galería de imágenes, y presentar los estados de carga, vacío y error con `Reintentar`.
10. Actualizar `PostCard.razor` para mostrar imágenes firmadas, mostrar `Editar` y `Eliminar` solo al autor, y emitir acciones a la página sin exponer operaciones directas desde el componente de presentación.
11. Añadir un diálogo de confirmación de eliminación que mantenga la entrada visible si falla la eliminación y recargue el feed tras borrar correctamente sus fotos y registro.
12. Ejecutar `dotnet build`, verificar las políticas con usuarios de la misma y distinta guardería, y comprobar los flujos de creación, edición, eliminación y lectura a 1440px y 390px.

## Acceptance criteria

- [ ] `dotnet build` termina sin errores.
- [ ] La migración añade `mood` al enum `public.post_type` sin eliminar los valores existentes.
- [ ] La migración crea `public.posts` y `public.post_photos` con RLS habilitada, restricciones de texto, orden de foto y claves foráneas indicadas.
- [ ] `post-images` es un bucket privado y no se almacena ninguna URL pública en la base de datos.
- [ ] Un perfil activo `staff` o `admin` puede crear una entrada en su propia guardería.
- [ ] Un perfil `parent`, inactivo o de otra guardería no puede crear entradas mediante RLS.
- [ ] Un perfil activo `staff`, `admin` o `parent` solo puede leer entradas e imágenes de su propia guardería.
- [ ] Un usuario no puede leer por Data API ni por Storage una entrada o imagen de otra guardería.
- [ ] Solo el autor activo `staff` o `admin` puede actualizar o eliminar su propia entrada e imágenes.
- [ ] El formulario rechaza texto vacío o de más de 1.000 caracteres.
- [ ] El formulario rechaza más de cinco imágenes, archivos que no sean JPG, PNG o WebP, y archivos de más de 5 MB.
- [ ] Una entrada válida sin imágenes se publica y aparece arriba del muro sin recargar el navegador.
- [ ] Una entrada válida con entre una y cinco imágenes se publica, conserva su orden y muestra las imágenes a los lectores autorizados.
- [ ] Cada entrada muestra el nombre de su autor y su fecha y hora de publicación.
- [ ] El autor puede editar texto, tipo y las imágenes de su entrada sin superar cinco imágenes.
- [ ] El autor ve `Editar` y `Eliminar`; los demás lectores no ven esas acciones.
- [ ] Eliminar solicita confirmación y, al confirmarse, elimina la entrada y sus archivos de Storage.
- [ ] Un fallo al cargar, publicar, editar o eliminar muestra un error recuperable y no descarta silenciosamente el contenido ni elimina visualmente una entrada no borrada.
- [ ] El muro muestra un estado de carga, un estado vacío y `Reintentar` ante un error inicial.
- [ ] A 1440px y 390px, el muro, las galerías, los modales y sus estados no producen desbordamiento horizontal y siguen siendo operables por teclado.

## Decisions

- **Sí:** entradas asociadas directamente a `daycare_id`. El muro es común a todo el centro y no requiere destinatarios por niño o sala.
- **No:** `post_children` y `room_id`. Son necesarios para comunicaciones selectivas, que no forman parte de este flujo.
- **Sí:** `staff` y `admin` crean, y el autor gestiona solo sus propias entradas. Conserva la trazabilidad e impide modificar comunicaciones de otro miembro del equipo.
- **Sí:** familias activas del mismo centro leen el muro. Cumple la visibilidad definida sin divulgar datos entre guarderías.
- **Sí:** texto obligatorio de hasta 1.000 caracteres. Evita publicaciones vacías y mantiene los anuncios breves.
- **Sí:** máximo de cinco imágenes JPG, PNG o WebP de 5 MB. Equilibra una galería útil con carga y almacenamiento controlados.
- **Sí:** bucket privado, RLS y URLs firmadas efímeras. Las fotos no deben ser accesibles por URL fuera de una sesión autorizada.
- **Sí:** `mood` como valor persistido para `Ánimo`. Conserva los siete tipos visibles sin usar un valor ambiguo de otro tipo.
- **Sí:** eliminación confirmada y física de registros y archivos. El usuario solicitó borrar la entrada y no conservar un archivo oculto.
- **No:** reacciones, comentarios, borradores y notificaciones. Requieren sus propios modelos, permisos y pantallas.

## Risks

| Riesgo | Mitigación |
| --- | --- |
| Storage y Postgres no comparten una transacción | Si falla una operación, mostrar un error recuperable y limpiar los archivos subidos que no tengan fila `post_photos`; no retirar la entrada de la interfaz hasta confirmar el borrado. |
| Una política de Storage permite inferir rutas de otra guardería | Validar el bucket, el primer segmento `daycare_id`, el perfil activo propio y la entrada relacionada en cada política. |
| Un archivo declara un MIME falso | Validar tipo y tamaño en la interfaz y limitar el bucket a las extensiones admitidas; una validación de contenido estricta queda fuera de este cliente Blazor. |
| Una URL firmada caduca mientras se consulta el muro | Generar URLs firmadas al cargar el feed y permitir recargarlo mediante `Reintentar`. |

## What is **not** in this spec

- Entradas dirigidas a niños, salas o familias concretas.
- Reacciones, comentarios, notificaciones, borradores, programación o moderación.
- Creación o gestión de publicaciones por perfiles `parent`.
- Vídeos, documentos, edición de imagen, URLs públicas o archivos fuera de JPG, PNG y WebP.
