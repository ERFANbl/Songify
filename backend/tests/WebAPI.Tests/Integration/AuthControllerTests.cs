using Application.DTOs.Auth;
using EntityFrameworkCore.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebAPI.Tests.Integration;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        // Configure the factory to use an in-memory database for testing
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SongifyDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a DbContext using an in-memory database for testing
                services.AddDbContext<SongifyDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryAuthTestDb");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<SongifyDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Signup_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var signupRequest = new SignupRequest
        {
            UserName = "testuser",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/signup", signupRequest);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("testuser", result.UserName);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Signup_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        var signupRequest = new SignupRequest
        {
            UserName = "existinguser",
            Password = "Password123!"
        };

        // Create user first
        await _client.PostAsJsonAsync("/api/Auth/signup", signupRequest);

        // Act - try to create the same user again
        var response = await _client.PostAsJsonAsync("/api/Auth/signup", signupRequest);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("exists", result.Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange - create a user first
        var userName = "loginuser";
        var password = "Password123!";
        
        var signupRequest = new SignupRequest
        {
            UserName = userName,
            Password = password
        };
        await _client.PostAsJsonAsync("/api/Auth/signup", signupRequest);

        // Act - now try to login
        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(userName, result.UserName);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "nonexistentuser",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsSuccess()
    {
        // Arrange - create user and login to get token
        var userName = "logoutuser";
        var password = "Password123!";
        
        var signupRequest = new SignupRequest
        {
            UserName = userName,
            Password = password
        };
        await _client.PostAsJsonAsync("/api/Auth/signup", signupRequest);

        var loginRequest = new LoginRequest
        {
            UserName = userName,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AuthResponse>(loginContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Set token in the Authorization header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var logoutResponse = await _client.PostAsync("/api/Auth/logout", null);
        var logoutContent = await logoutResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.Contains("success", logoutContent.ToLower());
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsForbidden()
    {
        // Arrange - clear any authorization headers
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsync("/api/Auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
} 