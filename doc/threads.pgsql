CREATE TABLE Threads(
    id uuid PRIMARY KEY,
    author uuid,
    data_create timestamp,
    title varchar(64),
    body text,
    karma uuid[]
);