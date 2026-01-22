<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/85c43113-d7c8-425a-9747-6de7344546ac" /># A Twitter-like web application using ASP.NET Core
The web application uses Entity Framework and SQL Server along with other packages. It is a collaborative project between me and [darius006](https://github.com/darius006/SocialMediaApp). The workload has been split evenly ensuring both of us have worked frontend and backend. Foder more details checkout `Proiecte_Laborator (1).pdf`.

## Install
```bash
git clone https://github.com/a-violeta/SocialMediaApp.git
cd SocialMediaApp
```
Next setup your local database and make your first migration.

## Dependencies
### Frameworks
 - ASP.NET Core 9.0
 - .NET Core Runtime
### Entity Framework Core
 - Microsoft.EntityFrameworkCore.SqlServer
 - Microsoft.EntityFrameworkCore.Sqlite
 - Microsoft.EntityFrameworkCore.Tools
 - Microsoft.EntityFrameworkCore.Design
### Identity and UI
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.UI
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
### Code Generationg
 - Microsoft.VisualStudio.Web.CodeGeneration.Design

## Use
We recommend using VisualStudio.

## Functionalities
 - user registration
 - profile editing
 - prohibition of actions for non-authenticated users
 - profile searching
 - profile visibility public or private
 - follow other accounts
 - posting text, images or video
 - liking posts and commenting on posts
 - custom feed
 - responsive design
 - clean and intuitive design using buttons and bootstrap icons
 - administation through admin accounts
 - groups with moderators
 - user group joining with pending join
 - group messages
 - group searching by name or id
