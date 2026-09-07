create table public.daycares (
    id uuid primary key default gen_random_uuid(),
    name text not null check (length(btrim(name)) > 0),
    created_at timestamptz not null default now()
);

alter table public.daycares enable row level security;

revoke all privileges on table public.daycares from public;
revoke all privileges on table public.daycares from anon;
revoke all privileges on table public.daycares from authenticated;
