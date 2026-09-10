grant select on table public.daycares to authenticated;

create policy "Authenticated users can read their own daycare"
on public.daycares
for select
to authenticated
using (
    id in (
        select daycare_id
        from public.users
        where id = (select auth.uid())
    )
);
