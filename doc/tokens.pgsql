CREATE TABLE Tokens(
    id uuid PRIMARY KEY,
    user_id uuid,
    data_create timestamp,
    rental_period interval
);