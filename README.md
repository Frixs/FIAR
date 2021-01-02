# Project FIAR
The project is developed according to the attached [assignment](/ASSIGNMENT.md).

## Developed Features
* login screen
* registration screen
* lobby with the list of all logged-in users, friend list, and list of request to the user
* possibility to ask any on-line (and not currently engaged) user to play a game
* gameplay
* one or a selection of limited size boards
* administration: ACL, user administration, password reset
* log of all game results + dev logs
---
* an unlimited board
* password strength evaluation
* password reset using an e-mail (reset link) + email verification
* in-game chat
* save games with all turns and allow replay

## Solution Details
- `DI` (Dependency Injection) and `Logger` are based on Microsoft's base solution for ASP.NET and both are baked into my own separate core project called [Ixs.DNA](https://github.com/Frixs/DNA-Framework) that the project uses as the base
- `WebSockets` are implemented with help of the SignalR framework
- `ORM` is based on Microsoft's EF (Entity Framework)
---
- The gameplay page is fully implemented via `WebSockets`
- The lobby section uses `API` to manage data for the user
- Sign In/Up are implemented in the `API` and server-side
  - The web app uses server-side implementation
- `API` is fully implemented in `ApiController.cs` file and users can authenticate in via standard web sessions or JWT tokens
