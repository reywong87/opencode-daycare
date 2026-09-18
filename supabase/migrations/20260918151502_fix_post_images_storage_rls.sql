create schema if not exists private;

revoke all on schema private from public;

create or replace function private.can_read_post_image(
    p_bucket_id text,
    p_name text
)
returns boolean
language plpgsql
stable
security definer
set search_path = ''
as $$
declare
    v_path text[];
    v_daycare_id uuid;
    v_post_id uuid;
begin
    if p_bucket_id <> 'post-images'
       or p_name !~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\.(jpg|jpeg|png|webp)$' then
        return false;
    end if;

    v_path := pg_catalog.string_to_array(p_name, '/');
    v_daycare_id := v_path[1]::uuid;
    v_post_id := v_path[2]::uuid;

    return exists (
        select 1
        from public.posts as p
        join public.users as u on u.id = (select auth.uid())
        where p.id = v_post_id
          and p.daycare_id = v_daycare_id
          and u.status = 'active'
          and u.daycare_id = p.daycare_id
    );
end;
$$;

create or replace function private.can_manage_post_image(
    p_bucket_id text,
    p_name text
)
returns boolean
language plpgsql
stable
security definer
set search_path = ''
as $$
declare
    v_path text[];
    v_daycare_id uuid;
    v_post_id uuid;
begin
    if p_bucket_id <> 'post-images'
       or p_name !~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\.(jpg|jpeg|png|webp)$' then
        return false;
    end if;

    v_path := pg_catalog.string_to_array(p_name, '/');
    v_daycare_id := v_path[1]::uuid;
    v_post_id := v_path[2]::uuid;

    return exists (
        select 1
        from public.posts as p
        join public.users as u on u.id = (select auth.uid())
        where p.id = v_post_id
          and p.daycare_id = v_daycare_id
          and p.author_id = (select auth.uid())
          and u.status = 'active'
          and u.role in ('staff', 'admin')
          and u.daycare_id = p.daycare_id
    );
end;
$$;

revoke all on function private.can_read_post_image(text, text) from public;
revoke all on function private.can_manage_post_image(text, text) from public;
grant execute on function private.can_read_post_image(text, text) to authenticated;
grant execute on function private.can_manage_post_image(text, text) to authenticated;

drop policy "Active daycare users can read post images" on storage.objects;
drop policy "Post authors can insert post images" on storage.objects;
drop policy "Post authors can update post images" on storage.objects;
drop policy "Post authors can delete post images" on storage.objects;

create policy "Active daycare users can read post images"
on storage.objects
for select
to authenticated
using ((select private.can_read_post_image(bucket_id, name)));

create policy "Post authors can insert post images"
on storage.objects
for insert
to authenticated
with check ((select private.can_manage_post_image(bucket_id, name)));

create policy "Post authors can update post images"
on storage.objects
for update
to authenticated
using ((select private.can_manage_post_image(bucket_id, name)))
with check ((select private.can_manage_post_image(bucket_id, name)));

create policy "Post authors can delete post images"
on storage.objects
for delete
to authenticated
using ((select private.can_manage_post_image(bucket_id, name)));
