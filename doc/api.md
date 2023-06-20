# API body

## Common part

All data as JSON:

- code
- body
  - ...

Also:

- 0 - Post
- 1 - O
- 2 - Get
- 3 - 000

### Codes and body

#### 000

Ping. (Pong is 001 from server)
Body: *empty*

#### 001

This code return server.
Body:

- status - OK or NOT
- message - optional if status is NOT

#### 010

Set hello.
Body:

- content - string

#### 012

Get hello.
Body: empty

#### 013

Return hello.
Body:

- content - string

#### 020

Registration
Body:

- username - string
- email - string
- password - string

#### 030

Auth
Body:

- username - string
- password - string

#### 042

Get info about users

Body:

- id or token - string

#### 043

Return info about users

Body:

- status
- user
  - id
  - username
  - date
  - email - if used token
  - role - string
