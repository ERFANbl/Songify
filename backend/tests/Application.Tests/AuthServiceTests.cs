using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Domain.DbMpdels;
using Moq;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _authService = new AuthService(_userRepositoryMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task SignupAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var request = new SignupRequest
        {
            UserName = "testuser",
            Password = "password123"
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(request.UserName))
            .ReturnsAsync((User)null); // No existing user

        _userRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(service => service.GenerateToken(It.IsAny<User>()))
            .Returns("mocked-jwt-token");

        // Act
        var result = await _authService.SignupAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User registered successfully!", result.Message);
        Assert.Equal("mocked-jwt-token", result.Token);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task SignupAsync_ShouldReturnFailure_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new SignupRequest
        {
            UserName = "existinguser",
            Password = "password123"
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(request.UserName))
            .ReturnsAsync(new User { Name = request.UserName });

        // Act
        var result = await _authService.SignupAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Username already exists.", result.Message);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest
        {
            UserName = "testuser",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Name = request.UserName,
            PasswordHash = PasswordHasher.HashPassword(request.Password)
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(request.UserName))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(service => service.GenerateToken(user))
            .Returns("mocked-jwt-token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful!", result.Message);
        Assert.Equal("mocked-jwt-token", result.Token);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            UserName = "testuser",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Name = request.UserName,
            PasswordHash = PasswordHasher.HashPassword("password123")
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(request.UserName))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password.", result.Message);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Token = "mocked-jwt-token"
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(user))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        Assert.True(result);
        Assert.Null(user.Token);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 1;

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        Assert.False(result);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
