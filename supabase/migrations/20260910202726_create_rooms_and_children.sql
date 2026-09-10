begin;

create type public.child_status as enum ('active', 'archived');

create table public.rooms (
    id uuid primary key default gen_random_uuid(),
    daycare_id uuid not null references public.daycares(id) on delete cascade,
    name text not null check (length(btrim(name)) > 0),
    created_at timestamptz not null default now(),
    unique (daycare_id, name)
);

create index rooms_daycare_id_idx on public.rooms (daycare_id);

create table public.children (
    id uuid primary key default gen_random_uuid(),
    room_id uuid not null references public.rooms(id) on delete restrict,
    full_name text not null check (length(btrim(full_name)) > 0),
    birth_date date not null,
    enrolled_at date not null default current_date,
    medical_notes text,
    allergy_tags text[],
    photo_consent boolean not null default true,
    status public.child_status not null default 'active',
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create index children_room_id_idx on public.children (room_id);
create index children_active_room_name_idx on public.children (room_id, full_name)
where status = 'active';

create function public.set_children_updated_at()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
    new.updated_at = now();
    return new;
end;
$$;

create trigger children_set_updated_at
before update on public.children
for each row
execute function public.set_children_updated_at();

alter table public.rooms enable row level security;
alter table public.children enable row level security;

revoke all privileges on table public.rooms from public;
revoke all privileges on table public.rooms from anon;
revoke all privileges on table public.rooms from authenticated;
revoke all privileges on table public.children from public;
revoke all privileges on table public.children from anon;
revoke all privileges on table public.children from authenticated;
revoke execute on function public.set_children_updated_at() from public;

grant select on table public.rooms to authenticated;
grant select, insert on table public.children to authenticated;

create policy "Operational users can read their daycare rooms"
on public.rooms
for select
to authenticated
using (
    exists (
        select 1
        from public.users
        where id = (select auth.uid())
          and daycare_id = rooms.daycare_id
          and status = 'active'
          and role in ('staff', 'admin')
    )
);

create policy "Operational users can read children in their daycare"
on public.children
for select
to authenticated
using (
    exists (
        select 1
        from public.rooms
        join public.users on users.daycare_id = rooms.daycare_id
        where rooms.id = children.room_id
          and users.id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

create policy "Operational users can add children to their daycare"
on public.children
for insert
to authenticated
with check (
    exists (
        select 1
        from public.rooms
        join public.users on users.daycare_id = rooms.daycare_id
        where rooms.id = children.room_id
          and users.id = (select auth.uid())
          and users.status = 'active'
          and users.role in ('staff', 'admin')
    )
);

insert into public.rooms (daycare_id, name)
select daycares.id, room_names.name
from public.daycares as daycares
cross join (values ('Soles'), ('Estrellas'), ('Arcoíris')) as room_names(name)
on conflict (daycare_id, name) do nothing;

insert into public.children (room_id, full_name, birth_date, enrolled_at, medical_notes)
select
    rooms.id,
    children.full_name,
    children.birth_date,
    children.enrolled_at,
    children.medical_notes
from public.rooms as rooms
join public.daycares as daycares on daycares.id = rooms.daycare_id
cross join (
    values
        ('Mateo Fernández', date '2022-03-12', date '2025-02-01', 'Alergia al maní. Evitar frutos secos. Lleva inhalador en la mochila.'),
        ('Sofía Méndez', date '2023-07-08', date '2025-03-01', null),
        ('Benjamín Ruiz', date '2022-09-20', date '2025-01-01', null),
        ('Valentina Soto', date '2023-02-17', date '2025-04-01', null),
        ('Tomás Díaz', date '2022-05-30', date '2025-02-01', 'Intolerancia a la lactosa. Enviar colaciones sin lácteos.'),
        ('Emma Castro', date '2023-10-04', date '2025-05-01', null),
        ('Lucas Romero', date '2022-01-25', date '2025-03-01', null),
        ('Olivia Vega', date '2023-04-11', date '2025-06-01', null)
) as children(full_name, birth_date, enrolled_at, medical_notes)
where daycares.name = 'Guardería Soles'
  and rooms.name = 'Soles';

commit;
