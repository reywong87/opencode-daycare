create schema if not exists private;

revoke all on schema private from public;

create or replace function private.current_active_user_daycare_id()
returns uuid
language sql
stable
security definer
set search_path = ''
as $$
    select u.daycare_id
    from public.users as u
    where u.id = (select auth.uid())
      and u.status = 'active';
$$;

revoke all on function private.current_active_user_daycare_id() from public;
grant execute on function private.current_active_user_daycare_id() to authenticated;

drop policy "Active users can read active profiles in own daycare"
on public.users;

create policy "Active users can read active profiles in own daycare"
on public.users
for select
to authenticated
using (
    status = 'active'
    and daycare_id = (select private.current_active_user_daycare_id())
);
