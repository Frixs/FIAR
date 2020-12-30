# Semester Project PIA 2020

You will be implementing an on-line version of the
'[Piškvorky](https://cs.wikipedia.org/wiki/Pi%C5%A1kvorky)' game,
also known as *Five in a row*.

If there's anything unclear, contact me as soon as possible, please.

## Scope

### Mandatory parts

* login screen
* registration screen
* lobby with the list of all logged-in users
* friends list with the possibility to add/remove users and showing the status
(on-line/off-line) of all user's friends
* possibility to ask any on-line (and not currently engaged) user to play a game
* gameplay
* one or a selection of limited size boards
* administration - user administration, password reset, tournament setup only for privileged
users
* log of all game results

All mandatory parts must be fully implemented and work without failures. Login and registration
screen must have all fields validated and must never end in an unhandled ISE 500.

**Total: 25 points**

### Bonus parts

* an unlimited board - **3 points**
* password strength evaluation - **2 points**
* OAuth2 authentication using Facebook, Google or any other OAuth2 IDM - **10 points**
* tournaments - set-up (define number of players), let players join, play, show
results to participants - **5 points**
  - list of open tournaments must be added to the lobby unless announcements 
	are implemented
* announcements - part of lobby - announce open tournaments, game and tournament results - **5 points**
* password reset using a security question - **2 points**
* password reset using an e-mail (reset link) - **5 points**
* in-game chat - **5 points**
* save games with all turns and allow replay - **5 points**
* feel free to come up with any other suggestion

## Technology

You may use any stack you like as long as the following technologies
are present.

Prefered stack is based on Java 11+, Spring 5, Spring Boot 2, Hibernate (JPA).

### Mandatory

* HTML, CSS
* responsive design - using of pre-defined templates is forbidden, but you
may use frameworks such as [Bootstrap](https://getbootstrap.com/),
[Foundation](https://get.foundation/) or other, don't worry about
the design, it has to be functional, not pretty
* IoC/DI
* ORM
* sdandalone database (MySQL, PostgreSQL or anything else you are able to
start in a Docker container)
* REST - there are many places where you can choose between REST and server-side, use REST
at least once
* web sockets
* docker
* docker compose

### Bonus

* HTML canvas for the gameplay (otherwise it can be implemented using an HTML table) - **2 points**
* [OpenAPI](https://swagger.io/specification/), Swagger, [RAML](https://raml.org/)
or any other API modeling/specification language with code generation - **10 points**
* Angular, React, any other frontend technology - **10 points**

## Solution details

### User Registration

* use e-mail address as the login name
* the e-mail address must be unique within the user database
* pasword and password validation must be provided by the user
* the user must be notified in case of any problems (e-mail address already
registered, passwords don't match, ...)
* users are stored in the database, passwords must be encrypted
* can be implemented as a REST call or a server-side logic

### Login

* e-mail and password
* user must be notified of authentication failures without hinting what field was incorrect
* can be implemented as a REST call or a server-side logic

### Lobby

* two panes or tabs with on-line users and friends with a status indicator
  - refresh periodically by REST, AJAX or by a web socket listener, only the lists, not the whole page
* non-engaged (not playing and not currently answering other user's game request) on-line players
can be contacted with a game request, implement as a REST call or server-side with AJAX to prevent
refreshing of the whole page
* contacted user is notified and allowed to either accept or reject the invitation, REST or server-side
with AJAX
* first user is notified about the decision
* user notifications using web socket

### Friends

* any user can ask any other on-line user to become friends - implemented as REST or server-side with AJAX
* contacted user is notified using web-socket and given a choice
* choice can be implemented as REST or server-side with AJAX
* any user can remove any of his friends from the list, they should
be asked for confirmation before the action is performed (using REST or AJAX), removal
must be effective for both sides

### Gameplay

* the player must see who's in turn at any time during the game
* can be implemented as an HTML table (JSF `<h:dataTable />` for example)
* the player in turn has the possibility to place his mark in any free square
* the turn can be implemented as a REST call or a server-side with AJAX
* the other player must be notified using web socket
* in-game chat message sending can be implemented either as a REST call or a server-side with AJAX
* in-game chat message notification must be implemented using web socket
* in-game chat message retrieval in any of the three technologies
* after one player wins or the board is full, both users are notified about the results
* on an exit button they return (separately) to the lobby
* the exit button is always available, when used during the game, the opponent is
notified to have won

## Submission

The project submission must consist of:

1. the source code
2. a Dockerfile to build the project
3. a Dockerfiles and docker compose file to run the entire project
4. a documentation describing how to run the whole project including a database
or any other containers necessary
5. a short document (A4, can be a single markdown file) on the solution

You can either provide an archive containing all the parts (upload to the courseware)
or (preferably) a link to a GIT repository of your choice (GitHub, BitBucket, ...).

### Points

All the bonus points listed are maximum values and can be reduced in case of incomplete
work or other problems. You can receive maximum of **50 points** for the whole project.
