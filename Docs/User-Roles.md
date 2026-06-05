# User Roles

CineScope uses ASP.NET Core Identity with role-based authorization.

## Guest

A Guest is any visitor who is not logged in.

Allowed:

- Browse the homepage.
- View movie listings.
- Search and filter movies.
- View movie details.

Not allowed:

- Add reviews or ratings.
- Add movies to favorites.
- Access the favorites page.
- Create, edit, or delete movies.
- Import movies from TMDB.
- Access the Admin dashboard.

## Member

A Member is a registered user. New accounts are assigned to the `Member` role automatically during registration.

Allowed:

- All Guest permissions.
- Add movie reviews.
- Rate movies from 1 to 10.
- Add movies to favorites.
- Remove movies from favorites.
- View the personal favorites page.

Not allowed:

- Create, edit, or delete movies.
- Import movies from TMDB.
- Delete other users' reviews.
- Access the Admin dashboard.

## Admin

Admin users manage the catalog and platform content.

Allowed:

- All browsing features.
- Create movies.
- Edit movies.
- Delete movies.
- Import movies from TMDB.
- View trending TMDB movies.
- Delete inappropriate reviews.
- View Admin dashboard analytics.

Default Admin:

```text
Email: admin@cinescope.com
Password: Admin123!
```

## Authorization Examples

Movie management actions are restricted to Admin users:

```csharp
[Authorize(Roles = IdentitySeeder.AdminRole)]
```

Member-only features such as favorites and reviews use:

```csharp
[Authorize(Roles = IdentitySeeder.MemberRole)]
```
