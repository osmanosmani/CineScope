# Reflection Report

## What I Built

For this project, I built CineScope, a modern movie discovery and management platform using ASP.NET Core MVC. The application started as a basic CRUD system for movies and was developed step by step into a more complete platform with authentication, roles, reviews, favorites, external API integration, an admin dashboard, and simple real-time notifications.

The main idea was to create a project that shows both the fundamentals of MVC and more practical web application features that are common in real systems.

I also added an `About Me` page to present the project clearly during a school demo or portfolio review.

## What I Learned

I learned how an ASP.NET Core MVC application is structured and how different parts of the application work together. I practiced working with controllers, models, views, view models, services, and database context classes.

I also learned why it is important to build a project in levels. Starting with a working CRUD system made it easier to add more advanced features later without losing control of the code.

## MVC Structure

The MVC pattern helped separate responsibilities:

- Models represent the database entities.
- Views display the UI with Razor syntax.
- Controllers receive requests and decide what response to return.
- ViewModels prepare page-specific data for the UI.

This made the project easier to understand because each part has a clear purpose.

## EF Core and Database

Entity Framework Core was used to connect the application to SQL Server. I learned how to define entity models, create `ApplicationDbContext`, configure relationships, and use migrations to update the database.

The main entities are:

- Movie
- Review
- Favorite
- IdentityUser
- IdentityRole

Reviews and favorites are connected to both movies and users, which helped me understand one-to-many relationships.

## Identity and Roles

ASP.NET Core Identity was used for login, registration, and role-based authorization. I created Admin and Member roles, and a default Admin user is seeded when the application starts.

This helped me understand how different users can have different permissions:

- Guests can browse.
- Members can review and save favorites.
- Admins can manage movies and view the dashboard.

## External API Integration

TMDB was added as an external movie API. Admin users can search TMDB, view trending movies, and import selected movies into the local database.

I learned that API keys should not be hardcoded in service classes or committed to GitHub. For local development, user secrets are a safer option.

## Challenges

Some challenges during the project were:

- Keeping each level small enough to avoid breaking previous features.
- Combining Identity with the existing database context.
- Making sure Admin-only pages were protected.
- Preventing duplicate movie imports.
- Keeping the dark UI consistent across all pages.
- Adding SignalR without rewriting the review system.

## Future Improvements

Future improvements could include:

- More advanced review moderation.
- User profile pages.
- Pagination for large movie catalogs.
- Better dashboard charts.
- Unit and integration tests.
- Azure Key Vault for secrets.
- Full CI/CD deployment pipeline.
- Docker deployment.

## Conclusion

CineScope helped me practice building a real MVC application from the foundation to more advanced features. The final project demonstrates CRUD, database relationships, authentication, authorization, external API integration, and a modern Bootstrap-based UI.
