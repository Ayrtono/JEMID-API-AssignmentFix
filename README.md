# JEMID-API-AssignmentFix
A mockup of JEM-id's backend test.

This repository contains 4 files of note:
- Program.cs to build and run the host
- Startup.cs to configure the services and middleware of the application
- In the models/ folder an Article.cs for an Article class which represents the data types being handled
- ArticleController.cs to handle the Endpoints from frontend calls to manipulate the database of Articles
- MyDbContext.cs to help interact with the database to ensure that each Article is unique.
