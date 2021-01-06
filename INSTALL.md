# INSTALATION INSTRUCTIONS
### DOCKER (App + PostgreSQL)
1. Download the repository to your local storage
2. Download and install [Docker Desktop](https://www.docker.com) for your platform
    - The system is predefined to run in Linux based Docker containers (it set set by default after Docker installation)
3. Download and install [Node.js](https://nodejs.org) to access `npm` package manager and install app dependencies
    1. Go to the app's root `/Fiar/Fiar` where the `package.json` file is located
    2. Use command `npm install` in the command prompt to install app dependencies
4. Build Docker compose and run the system
    1. Go to the app's root `/Fiar/Fiar` where the `package.json` file is located
    2. Use command `docker-compose build` in the command prompt
    3. Run the system by using the command `docker-compose up`
    
#### DOCKER SYSTEM CONFIGURATION
*- Default settings are set to work with default PostgreSQL settings.*  
*- Default port is set to 5000.*

- Docker-compose file requires to specify database connection details (`/Fiar/Fiar/appsettings.config`)
- The app configuration can be set via configuration file: `/Fiar/Fiar/appsettings.config`
    - Mandatory settings to set:
        - `DatabaseConnection:Technology` - database technology to use
        - `ConnectionStrings:DefaultConnection` - connection string to the database
        - `UseUrlsString` - contains port number to run the app on
        - `MessageService:Email:*` - mail server settings
