create or replace function TableNameFromRepoKey(repo_key text) returns text as $$
begin
    if strpos(repo_key, '/') = 0 then
        raise exception 'Invalid repo key: %', repo_key;
    end if;

    return 'SyncedIssue_' || replace(repo_key, '/', '_');
end;
$$ language plpgsql;

create or replace function CreateIssueSyncTable(repo_key text) returns text as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);

    execute format('CREATE TABLE IF NOT EXISTS %I ( like "SyncedIssueTemplate" INCLUDING DEFAULTS INCLUDING CONSTRAINTS INCLUDING INDEXES )', 'tmp_' || table_name);
    return 'tmp_' || table_name;
end;
$$ language plpgsql;

create or replace function InsertIntoIssueSyncTable(repo_key text, id int, number int, title text, url text, status text, excerpt text, repoConfigId uuid) returns int as $$
declare
    table_name text;
    id_exists bool;
begin

    table_name = 'tmp_' || TableNameFromRepoKey(repo_key);
    
    execute format('SELECT 1 FROM %I WHERE "Id" = %L', table_name, id) into id_exists;

    if id_exists then
        return 0;
    end if;

    execute format('INSERT INTO %I ("Id", "Number", "Title", "Url", "Status", "Excerpt", "RepoConfigId")' ||
                   'values (%L, %L, %L, %L, %L, %L, %L)',
                   table_name, id, number, title, url, status, excerpt, repoConfigId);
    return 1;
end;
$$ language plpgsql;

create or replace function SwapIssueSyncTable(repo_key text) returns int as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);

    execute format('alter table if exists %I rename to %I;', table_name, table_name || '_old');
    execute format('alter table %I rename to %I;', 'tmp_' || table_name, table_name);
    execute format('drop table if exists %I', table_name || '_old');

    return 1;
end;
$$ language plpgsql;

create or replace function CreateIssueTable(repo_key text) returns text as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);

    execute format('CREATE TABLE IF NOT EXISTS %I ( like "SyncedIssueTemplate" INCLUDING DEFAULTS INCLUDING CONSTRAINTS INCLUDING INDEXES )', table_name);
    return table_name;
end;
$$ language plpgsql;

create or replace function InsertIntoIssueTable(repo_key text, id int, number int, title text, url text, status text, excerpt text, repoConfigId uuid) returns int as $$
declare
    issue_id int;
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);

    execute format('SELECT "Id" FROM %I WHERE "Id" = %L', table_name, id) into issue_id;
    
    if issue_id is NULL then
        execute format('INSERT INTO %I ("Id", "Number", "Title", "Url", "Status", "Excerpt", "RepoConfigId")' ||
                       'values (%L, %L, %L, %L, %L, %L, %L)',
                       table_name, id, number, title, url, status, excerpt, repoConfigId);
    else
        execute format('UPDATE %I SET "Title" = %L, "Url" = %L, "Status" = %L, "Excerpt" = %L WHERE "Id" = %L',
                       table_name, title, url, status, excerpt, id);
    end if;
    
    return 1;
end;
$$ language plpgsql;

create or replace function GetIssue(repo_key text, issue_id int)
    returns setof "SyncedIssueTemplate" as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);
    return QUERY execute format('select * from %I where "Id" = %L', table_name, issue_id);
end;
$$ language plpgsql;

create or replace function GetIssues(repo_key text)
    returns setof "SyncedIssueTemplate" as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);
    return QUERY execute format('select * from %I', table_name);
end;
$$ language plpgsql;

create or replace function SearchIssues(repo_key text, term text, lmt int)
    returns setof "SyncedIssueTemplate" as $$
declare
    table_name text;
begin
    table_name = TableNameFromRepoKey(repo_key);
    SET pg_trgm.similarity_threshold = 0.3;
    return QUERY execute format('select * from %I where "Title" %% %L or to_tsvector(''english''::regconfig, "Title") @@ websearch_to_tsquery(''english''::regconfig, %L) order by similarity("Title", %L) desc limit %L', table_name, term, term, term, lmt);
end;
$$ language plpgsql;