# Songify Development Tips - April 19, 2025

## Database & Entity Framework Core Tips

### Clean Architecture Database Implementation
- **Tip**: Keep entity configurations in separate files for better maintainability
  - Example: `SongConfiguration.cs` for the `Song` entity
- **Tip**: Define all entity relationships in OnModelCreating method using Fluent API
- **Tip**: Use migrations for tracking schema changes, run `dotnet ef migrations add [Name]` after model changes

### Domain Model Design
- **Tip**: Keep domain entities clean of data access concerns
  - Avoid EF Core attributes in domain entities
  - Use configuration classes for database mapping
- **Tip**: Design relationships carefully:
  - One-to-many: User to Playlists
  - Many-to-many: Playlists to Songs via PlaylistSong

## Redis Caching Best Practices

### Implementation Guidelines
- **Tip**: Always inject `IRedisService` not the concrete implementation
- **Tip**: Use appropriate expiration times for different types of data:
  - User preferences: longer expiration (hours/days)
  - Playlist data: medium expiration (minutes/hours)
  - Real-time data: shorter expiration (seconds/minutes)
- **Tip**: Key naming convention: `{entity}:{id}:{property}`
  - Example: `song:1234:details` or `user:5678:playlists`

### Performance Optimization
- **Tip**: Cache frequently accessed data like popular playlists
- **Tip**: Use Redis for distributed locking when needed
- **Tip**: Consider batch operations for multiple related keys

## Dependency Injection & Application Structure

### Best Practices
- **Tip**: Register services in their respective infrastructure layers
- **Tip**: Use extension methods for organizing service registration
  - Example: `services.AddInfrastructureServices(configuration)`
- **Tip**: Configure services by environment (Development vs Production)

### API Development
- **Tip**: Use Swagger for API documentation and testing
- **Tip**: Implement consistent error handling across controllers
- **Tip**: Consider versioning your API from the beginning

## Docker & Configuration Tips
- **Tip**: Set up Redis in docker-compose for local development
- **Tip**: Store connection strings in user secrets during development
- **Tip**: Use environment-specific appsettings files
  - `appsettings.Development.json` vs `appsettings.Production.json`