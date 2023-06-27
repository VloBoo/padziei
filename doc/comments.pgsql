CREATE TABLE Comments(
    id uuid PRIMARY KEY,
    thread_id uuid,
    author uuid,
    data_create timestamp,
    body text
);