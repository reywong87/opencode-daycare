begin;

do $$
begin
    if not exists (
        select 1
        from pg_type
        join pg_namespace on pg_namespace.oid = pg_type.typnamespace
        where pg_namespace.nspname = 'public'
          and pg_type.typname = 'post_type'
    ) then
        create type public.post_type as enum (
            'meal',
            'nap',
            'activity',
            'achievement',
            'photo',
            'announcement',
            'mood'
        );
    elsif not exists (
        select 1
        from pg_enum
        join pg_type on pg_type.oid = pg_enum.enumtypid
        join pg_namespace on pg_namespace.oid = pg_type.typnamespace
        where pg_namespace.nspname = 'public'
          and pg_type.typname = 'post_type'
          and pg_enum.enumlabel = 'mood'
    ) then
        alter type public.post_type add value 'mood';
    end if;
end;
$$;

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

create index posts_daycare_published_at_idx
on public.posts (daycare_id, published_at desc);

create index post_photos_post_position_idx
on public.post_photos (post_id, position);

create function public.set_posts_updated_at()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
    new.updated_at = now();
    return new;
end;
$$;

create trigger posts_set_updated_at
before update on public.posts
for each row
execute function public.set_posts_updated_at();

alter table public.posts enable row level security;
alter table public.post_photos enable row level security;

revoke all privileges on table public.posts from public;
revoke all privileges on table public.posts from anon;
revoke all privileges on table public.posts from authenticated;
revoke all privileges on table public.post_photos from public;
revoke all privileges on table public.post_photos from anon;
revoke all privileges on table public.post_photos from authenticated;
revoke execute on function public.set_posts_updated_at() from public;

grant select, insert, update, delete on table public.posts to authenticated;
grant select, insert, update, delete on table public.post_photos to authenticated;

create policy "Active daycare users can read posts"
on public.posts
for select
to authenticated
using (
    exists (
        select 1
        from public.users
        where users.id = (select auth.uid())
          and users.daycare_id = posts.daycare_id
          and users.status = 'active'
          and users.role in ('staff', 'admin', 'parent')
    )
);

create policy "Active staff can create posts"
on public.posts
for insert
to authenticated
with check (
    author_id = (select auth.uid())
    and exists (
        select 1
        from public.users
        where users.id = (select auth.uid())
          and users.daycare_id = posts.daycare_id
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Authors can update their active posts"
on public.posts
for update
to authenticated
using (
    author_id = (select auth.uid())
    and exists (
        select 1
        from public.users
        where users.id = (select auth.uid())
          and users.daycare_id = posts.daycare_id
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
)
with check (
    author_id = (select auth.uid())
    and exists (
        select 1
        from public.users
        where users.id = (select auth.uid())
          and users.daycare_id = posts.daycare_id
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Authors can delete their active posts"
on public.posts
for delete
to authenticated
using (
    author_id = (select auth.uid())
    and exists (
        select 1
        from public.users
        where users.id = (select auth.uid())
          and users.daycare_id = posts.daycare_id
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Active daycare users can read post photos"
on public.post_photos
for select
to authenticated
using (
    exists (
        select 1
        from public.posts
        join public.users on users.daycare_id = posts.daycare_id
        where posts.id = post_photos.post_id
          and users.id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin', 'parent')
    )
);

create policy "Post authors can create photos"
on public.post_photos
for insert
to authenticated
with check (
    exists (
        select 1
        from public.posts
        join public.users on users.id = posts.author_id
        where posts.id = post_photos.post_id
          and posts.author_id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Post authors can update photos"
on public.post_photos
for update
to authenticated
using (
    exists (
        select 1
        from public.posts
        join public.users on users.id = posts.author_id
        where posts.id = post_photos.post_id
          and posts.author_id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
)
with check (
    exists (
        select 1
        from public.posts
        join public.users on users.id = posts.author_id
        where posts.id = post_photos.post_id
          and posts.author_id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Post authors can delete photos"
on public.post_photos
for delete
to authenticated
using (
    exists (
        select 1
        from public.posts
        join public.users on users.id = posts.author_id
        where posts.id = post_photos.post_id
          and posts.author_id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

commit;
