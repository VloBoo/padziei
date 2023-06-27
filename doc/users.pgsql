CREATE TABLE Users(
    id uuid PRIMARY KEY,
    username varchar(32) UNIQUE,
    password varchar(64),
    data_create timestamp,
    email varchar(64) UNIQUE,
    role varchar(8)
);