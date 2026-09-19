create policy "Active users can read active profiles in own daycare"
on public.users
for select
to authenticated
using (
    status = 'active'
    and exists (
        select 1
        from public.users as viewer
        where viewer.id = (select auth.uid())
          and viewer.status = 'active'
          and viewer.daycare_id = users.daycare_id
    )
);
