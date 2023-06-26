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
  - email - if used token (this is shitcode, we need to remove it)
  - role - string

#### 052

Get x Theads

Body:

- count - this is x

#### 053

Return 10 uuids of Theads

Body:

- threads - uuid[]

#### 062

Get Thead Info

Body:

- id

#### 063

Return Thread Info

Body:

- id
- author
- date
- title
- body
- karma_count

#### 070

Create thread

Body:

- token
- title
- content

#### 071

Return id created thread

Body:

- id

#### 082

Get comments

Body:

- thread

#### 083

Return comments

Body:

- comments - uuid[]

#### 092

Get comment info

Body:

- id

#### 093

Return comment info

Body:

- id
- thread
- author
- date
- content

#### 100

Create comment

Body:

- token
- thread
- content

#### 101

Return id created comment

Body:

- id

#### 110

Delete Thead

Body:

- id

#### 120

Delete Comment

Body:

- id

#### 130

Set role

Body:

- user
- role
