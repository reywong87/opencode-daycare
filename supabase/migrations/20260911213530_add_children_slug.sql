begin;

alter table public.children add column slug text;

do $$
declare
    child record;
    base_slug text;
    candidate_slug text;
    suffix integer;
begin
    for child in
        select id, full_name
        from public.children
        order by id
    loop
        base_slug := btrim(
            regexp_replace(
                translate(
                    lower(
                        array_to_string(
                            (regexp_split_to_array(btrim(child.full_name), '\s+'))[1:3],
                            '-'
                        )
                    ),
                    'áàäâãåÁÀÄÂÃÅéèëêÉÈËÊíìïîÍÌÏÎóòöôõÓÒÖÔÕúùüûÚÙÜÛñÑçÇýÿÝŸ',
                    'aaaaaaAAAAAAeeeeEEEEiiiiIIIIoooooOOOOOuuuuUUUUnNcCyyYY'
                ),
                '[^a-z0-9]+',
                '-',
                'g'
            ),
            '-'
        );
        base_slug := coalesce(nullif(base_slug, ''), 'nino');
        candidate_slug := base_slug;
        suffix := 2;

        while exists (select 1 from public.children where slug = candidate_slug) loop
            candidate_slug := base_slug || '-' || suffix;
            suffix := suffix + 1;
        end loop;

        update public.children
        set slug = candidate_slug
        where id = child.id;
    end loop;
end;
$$;

alter table public.children
    alter column slug set not null,
    add constraint children_slug_key unique (slug);

commit;
