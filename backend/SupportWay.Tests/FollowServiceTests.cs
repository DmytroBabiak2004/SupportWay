using Moq;
using SupportWay.API.DTOs;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Implementations;
using SupportWay.Services.Interfaces;
using Xunit;

namespace SupportWay.Tests;

/// <summary>
/// Юніт-тести для FollowService.
/// Покривають: підписку, відписку, перевірку статусу,
/// підрахунок підписників/підписок та маппінг до DTO.
/// </summary>
public class FollowServiceTests
{
    private readonly Mock<IFollowRepository> _followRepoMock;
    private readonly FollowService           _sut;

    public FollowServiceTests()
    {
        _followRepoMock = new Mock<IFollowRepository>();
        _sut            = new FollowService(_followRepoMock.Object);
    }

    // ══════════════════════════════════════════════════════════════════════
    // FollowUserAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FollowUserAsync_CallsRepositoryWithCorrectIds()
    {
        // Arrange
        var followerId = "user-A";
        var followedId = "user-B";

        _followRepoMock
            .Setup(r => r.FollowUserAsync(followerId, followedId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.FollowUserAsync(followerId, followedId);

        // Assert
        _followRepoMock.Verify(r => r.FollowUserAsync(followerId, followedId), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UnfollowUserAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UnfollowUserAsync_CallsRepositoryWithCorrectIds()
    {
        // Arrange
        var followerId = "user-A";
        var followedId = "user-B";

        _followRepoMock
            .Setup(r => r.UnfollowUserAsync(followerId, followedId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UnfollowUserAsync(followerId, followedId);

        // Assert
        _followRepoMock.Verify(r => r.UnfollowUserAsync(followerId, followedId), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════════
    // IsFollowingAsync
    // ══════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsFollowingAsync_ReturnsRepositoryResult(bool expected)
    {
        // Arrange
        _followRepoMock
            .Setup(r => r.IsFollowingAsync("user-A", "user-B"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.IsFollowingAsync("user-A", "user-B");

        // Assert
        Assert.Equal(expected, result);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetFollowersCountAsync / GetFollowingCountAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetFollowersCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var userId = "popular-user";

        _followRepoMock
            .Setup(r => r.GetFollowersCountAsync(userId))
            .ReturnsAsync(42);

        // Act
        var count = await _sut.GetFollowersCountAsync(userId);

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public async Task GetFollowingCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var userId = "active-user";

        _followRepoMock
            .Setup(r => r.GetFollowingCountAsync(userId))
            .ReturnsAsync(17);

        // Act
        var count = await _sut.GetFollowingCountAsync(userId);

        // Assert
        Assert.Equal(17, count);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetFollowersAsync — маппінг до DTO
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetFollowersAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        var targetUserId = "user-target";

        var follower = BuildUser("follower-1", "john_doe",
            fullName: "Іван Іванов", isVerified: true, photo: new byte[] { 1, 2, 3 });

        _followRepoMock
            .Setup(r => r.GetFollowersAsync(targetUserId))
            .ReturnsAsync(new List<Follow>
            {
                new() { FollowerId = follower.Id, FollowedId = targetUserId, Follower = follower }
            });

        // Act
        var result = (await _sut.GetFollowersAsync(targetUserId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("follower-1",  result[0].UserId);
        Assert.Equal("john_doe",    result[0].Username);
        Assert.Equal("Іван Іванов", result[0].FullName);
        Assert.True(result[0].IsVerified);
        Assert.NotNull(result[0].PhotoBase64);
    }

    [Fact]
    public async Task GetFollowersAsync_WhenUserHasNoPhoto_PhotoBase64IsNull()
    {
        // Arrange
        var targetUserId = "user-target";
        var follower     = BuildUser("follower-2", "no_photo_user", photo: null);

        _followRepoMock
            .Setup(r => r.GetFollowersAsync(targetUserId))
            .ReturnsAsync(new List<Follow>
            {
                new() { Follower = follower, FollowedId = targetUserId }
            });

        // Act
        var result = (await _sut.GetFollowersAsync(targetUserId)).ToList();

        // Assert
        Assert.Null(result[0].PhotoBase64);
    }

    [Fact]
    public async Task GetFollowersAsync_WhenNoFollowers_ReturnsEmptyCollection()
    {
        // Arrange
        _followRepoMock
            .Setup(r => r.GetFollowersAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Follow>());

        // Act
        var result = await _sut.GetFollowersAsync("lonely-user");

        // Assert
        Assert.Empty(result);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetFollowingAsync — маппінг до DTO
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetFollowingAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        var sourceUserId = "user-source";
        var followed     = BuildUser("followed-1", "jane_doe", fullName: "Олена Петренко");

        _followRepoMock
            .Setup(r => r.GetFollowingAsync(sourceUserId))
            .ReturnsAsync(new List<Follow>
            {
                new() { FollowerId = sourceUserId, FollowedId = followed.Id, Followed = followed }
            });

        // Act
        var result = (await _sut.GetFollowingAsync(sourceUserId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("followed-1",     result[0].UserId);
        Assert.Equal("jane_doe",        result[0].Username);
        Assert.Equal("Олена Петренко", result[0].FullName);
    }

    // ══════════════════════════════════════════════════════════════════════
    // RemoveFollowerAsync — перевіряє інверсію аргументів
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RemoveFollowerAsync_CallsUnfollowWithSwappedIds()
    {
        // Arrange
        var ownerId            = "owner";
        var followerToRemoveId = "follower-to-remove";

        _followRepoMock
            .Setup(r => r.UnfollowUserAsync(followerToRemoveId, ownerId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RemoveFollowerAsync(ownerId, followerToRemoveId);

        // Assert — видалення підписника: followerToRemove відписується від owner
        _followRepoMock.Verify(
            r => r.UnfollowUserAsync(followerToRemoveId, ownerId),
            Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Допоміжний Builder
    // ══════════════════════════════════════════════════════════════════════

    private static User BuildUser(
        string   id,
        string   userName,
        string?  fullName   = null,
        bool     isVerified = false,
        byte[]?  photo      = null)
    {
        return new User
        {
            Id       = id,
            UserName = userName,
            Profile  = new Profile
            {
                Id         = Guid.NewGuid(),
                UserId     = id,
                FullName   = fullName,
                IsVerified = isVerified,
                Photo      = photo
            }
        };
    }
}
