create table public.users (
    id uuid primary key references auth.users(id) on delete cascade,
    daycare_id uuid references public.daycares(id) on delete restrict,
    role public.user_role not null,
    status public.user_status not null default 'active',
    full_name text not null check (length(btrim(full_name)) > 0),
    avatar_url text,
    notify_on_post boolean not null default true,
    daily_summary_enabled boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create index users_daycare_id_idx on public.users (daycare_id);

create function public.set_users_updated_at()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
    new.updated_at = now();
    return new;
end;
$$;

create trigger users_set_updated_at
before update on public.users
for each row
execute function public.set_users_updated_at();

alter table public.users enable row level security;

revoke all privileges on table public.users from public;
revoke all privileges on table public.users from anon;
revoke all privileges on table public.users from authenticated;
revoke execute on function public.set_users_updated_at() from public;

grant select on table public.users to authenticated;
grant update (full_name, avatar_url, notify_on_post, daily_summary_enabled)
on table public.users to authenticated;

create policy "Authenticated users can read their own profile"
on public.users
for select
to authenticated
using ((select auth.uid()) = id);

create policy "Authenticated users can update their own profile"
on public.users
for update
to authenticated
using ((select auth.uid()) = id)
with check ((select auth.uid()) = id);
