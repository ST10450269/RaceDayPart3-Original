# RaceDay — original implementation workspace

This is a clean implementation workspace based on the course endpoint plan, database design, and Part 3 student guide in `docs/requirements/`. The source was written in this workspace rather than copied from the public reference project. Review each part and disclose AI assistance as your course requires.

## Current implementation

- ASP.NET Core 8 API with `/health`, public published-event browsing (`GET /api/events`, `GET /api/events/{id}`), and organiser-only event creation (`POST /api/events`).
- Session login and logout, role checks, password hashing, normalized email addresses, duplicate-email rejection, and a strong-password policy.
- EF Core models for roles, users, and events. Development startup creates the schema and seeds demo records.
- ASP.NET Core MVC and shared-contract projects are scaffolded; MVC-to-API screens, remaining domain tables/endpoints, Azure Blob uploads, and Docker deployment are not yet implemented.
- Integration/unit tests use SQLite and cover health, password policy, registration/login/session behavior, and event browsing.

## Build and test

```powershell
dotnet build RaceDay.sln -c Release
dotnet test RaceDay.sln -c Release
```

The API expects `ConnectionStrings__RaceDay` to point to a SQL Server database. The test suite substitutes an isolated SQLite database. Demo accounts are created with hashed passwords for local learning only; change them before any real deployment.

## Youtube
https://youtu.be/9pUSdDHq--Y

<img width="1352" height="624" alt="image" src="https://github.com/user-attachments/assets/edfe0abd-2028-4e66-8cc9-834dc1158f30" />

