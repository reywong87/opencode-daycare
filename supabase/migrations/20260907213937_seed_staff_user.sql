do $$
declare
    staff_user_id uuid;
    staff_daycare_id uuid;
    staff_user_count integer;
    staff_daycare_count integer;
begin
    select count(*)
    into staff_user_count
    from auth.users
    where email = 'rey@google.com';

    if staff_user_count <> 1 then
        raise exception 'Expected exactly one Auth user for rey@google.com, found %', staff_user_count;
    end if;

    select id
    into staff_user_id
    from auth.users
    where email = 'rey@google.com';

    select count(*)
    into staff_daycare_count
    from public.daycares
    where name = 'Guardería Soles';

    if staff_daycare_count <> 1 then
        raise exception 'Expected exactly one daycare named Guardería Soles, found %', staff_daycare_count;
    end if;

    select id
    into staff_daycare_id
    from public.daycares
    where name = 'Guardería Soles';

    insert into public.users (id, daycare_id, role, status, full_name)
    values (staff_user_id, staff_daycare_id, 'staff', 'active', 'Rey');
end;
$$;
