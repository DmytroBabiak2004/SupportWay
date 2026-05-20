using Moq;
using Microsoft.AspNetCore.Http;
using SupportWay.API.DTOs;
using SupportWay.API.Repositories.Interfaces;
using SupportWay.API.Services;
using SupportWay.Data.Models;
using Xunit;

namespace SupportWay.Tests;

/// <summary>
/// Юніт-тести для BadgeService.
/// Покривають: отримання нагород, маппінг зображення в base64,
/// створення, оновлення та видалення з обробкою виняткових ситуацій.
/// </summary>
public class BadgeServiceTests
{
    private readonly Mock<IBadgeRepository> _badgeRepoMock;
    private readonly BadgeService           _sut;

    public BadgeServiceTests()
    {
        _badgeRepoMock = new Mock<IBadgeRepository>();
        _sut           = new BadgeService(_badgeRepoMock.Object);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetAllAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAllAsync_WhenBadgesExist_ReturnsMappedDtos()
    {
        // Arrange
        var badges = new List<Badge>
        {
            BuildBadge(Guid.NewGuid(), "Медик-початківець", "За 5 медичних допомог", 5m),
            BuildBadge(Guid.NewGuid(), "Волонтер",          "За 10 гуманітарних",   10m)
        };

        _badgeRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(badges);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Медик-початківець", result[0].Name);
        Assert.Equal("Волонтер",          result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBadgesExist_ReturnsEmptyList()
    {
        // Arrange
        _badgeRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Badge>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetAllAsync — маппінг зображення в base64
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAllAsync_WhenBadgeHasImage_ReturnsBase64String()
    {
        // Arrange
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        var badge      = BuildBadge(Guid.NewGuid(), "З фото", "Опис", 1m, image: imageBytes);

        _badgeRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Badge> { badge });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.NotNull(result[0].ImageBase64);
        Assert.Equal(Convert.ToBase64String(imageBytes), result[0].ImageBase64);
    }

    [Fact]
    public async Task GetAllAsync_WhenBadgeHasNoImage_ImageBase64IsNull()
    {
        // Arrange
        var badge = BuildBadge(Guid.NewGuid(), "Без фото", "Опис", 1m, image: null);

        _badgeRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Badge> { badge });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Null(result[0].ImageBase64);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetByIdAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetByIdAsync_WhenBadgeExists_ReturnsMappedDto()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var badge   = BuildBadge(badgeId, "Логіст", "За логістику", 15m);

        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(badgeId))
            .ReturnsAsync(badge);

        // Act
        var result = await _sut.GetByIdAsync(badgeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(badgeId,  result.Id);
        Assert.Equal("Логіст", result.Name);
        Assert.Equal(15m,      result.Threshold);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBadgeDoesNotExist_ReturnsNull()
    {
        // Arrange
        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Badge?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CreateBadgeAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateBadgeAsync_WhenBadgeTypeExists_AddsAndSaves()
    {
        // Arrange
        var badgeTypeId = Guid.NewGuid();

        _badgeRepoMock
            .Setup(r => r.BadgeTypeExistsAsync(badgeTypeId))
            .ReturnsAsync(true);

        _badgeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Badge>()))
            .Returns(Task.CompletedTask);

        _badgeRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new CreateBadgeRequest
        {
            Name        = "Нова нагорода",
            Description = "Опис нагороди",
            Threshold   = 10m,
            BadgeTypeId = badgeTypeId,
            Image       = BuildFakeFormFile()
        };

        // Act
        var resultId = await _sut.CreateBadgeAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        _badgeRepoMock.Verify(r => r.AddAsync(It.IsAny<Badge>()), Times.Once);
        _badgeRepoMock.Verify(r => r.SaveChangesAsync(),           Times.Once);
    }

    [Fact]
    public async Task CreateBadgeAsync_WhenBadgeTypeDoesNotExist_ThrowsException()
    {
        // Arrange
        _badgeRepoMock
            .Setup(r => r.BadgeTypeExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        var request = new CreateBadgeRequest
        {
            Name        = "Нагорода",
            Description = "Опис",
            Threshold   = 1m,
            BadgeTypeId = Guid.NewGuid(),
            Image       = BuildFakeFormFile()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _sut.CreateBadgeAsync(request));

        Assert.Contains("Тип нагороди не знайдено", ex.Message);
        _badgeRepoMock.Verify(r => r.AddAsync(It.IsAny<Badge>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UpdateBadgeAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateBadgeAsync_WhenBadgeExists_UpdatesFields()
    {
        // Arrange
        var badgeId     = Guid.NewGuid();
        var badgeTypeId = Guid.NewGuid();
        var existing    = BuildBadge(badgeId, "Стара назва", "Старий опис", 5m);

        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(badgeId))
            .ReturnsAsync(existing);

        _badgeRepoMock
            .Setup(r => r.BadgeTypeExistsAsync(badgeTypeId))
            .ReturnsAsync(true);

        _badgeRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new UpdateBadgeRequest
        {
            Id          = badgeId,
            Name        = "Нова назва",
            Description = "Новий опис",
            Threshold   = 20m,
            BadgeTypeId = badgeTypeId,
            Image       = null
        };

        // Act
        await _sut.UpdateBadgeAsync(request);

        // Assert — поля оновились безпосередньо на tracked entity
        Assert.Equal("Нова назва", existing.Name);
        Assert.Equal("Новий опис", existing.Description);
        Assert.Equal(20m,          existing.Threshold);
        _badgeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateBadgeAsync_WhenBadgeDoesNotExist_ThrowsException()
    {
        // Arrange
        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Badge?)null);

        var request = new UpdateBadgeRequest
        {
            Id          = Guid.NewGuid(),
            Name        = "Тест",
            Description = "Тест",
            BadgeTypeId = Guid.NewGuid()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _sut.UpdateBadgeAsync(request));

        Assert.Contains("Нагороду не знайдено", ex.Message);
    }

    [Fact]
    public async Task UpdateBadgeAsync_WhenBadgeTypeDoesNotExist_ThrowsExceptionAndDoesNotSave()
    {
        // Arrange
        var badgeId = Guid.NewGuid();

        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(badgeId))
            .ReturnsAsync(BuildBadge(badgeId, "Назва", "Опис", 1m));

        _badgeRepoMock
            .Setup(r => r.BadgeTypeExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        var request = new UpdateBadgeRequest
        {
            Id          = badgeId,
            Name        = "Нова назва",
            Description = "Новий опис",
            BadgeTypeId = Guid.NewGuid()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _sut.UpdateBadgeAsync(request));

        Assert.Contains("Тип нагороди не знайдено", ex.Message);
        _badgeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════
    // DeleteBadgeAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteBadgeAsync_WhenBadgeExists_DeletesAndSaves()
    {
        // Arrange
        var badgeId = Guid.NewGuid();
        var badge   = BuildBadge(badgeId, "Для видалення", "Опис", 1m);

        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(badgeId))
            .ReturnsAsync(badge);

        _badgeRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteBadgeAsync(badgeId);

        // Assert
        _badgeRepoMock.Verify(r => r.Delete(badge),      Times.Once);
        _badgeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBadgeAsync_WhenBadgeDoesNotExist_ThrowsExceptionAndDoesNotDelete()
    {
        // Arrange
        _badgeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Badge?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _sut.DeleteBadgeAsync(Guid.NewGuid()));

        Assert.Contains("Нагороду не знайдено", ex.Message);
        _badgeRepoMock.Verify(r => r.Delete(It.IsAny<Badge>()), Times.Never);
        _badgeRepoMock.Verify(r => r.SaveChangesAsync(),         Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GetByProfileIdAsync
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetByProfileIdAsync_ReturnsOnlyBadgesForGivenProfile()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var badges    = new List<Badge>
        {
            BuildBadge(Guid.NewGuid(), "Нагорода профілю", "Опис", 3m)
        };

        _badgeRepoMock
            .Setup(r => r.GetByProfileIdAsync(profileId))
            .ReturnsAsync(badges);

        // Act
        var result = await _sut.GetByProfileIdAsync(profileId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Нагорода профілю", result[0].Name);
        _badgeRepoMock.Verify(r => r.GetByProfileIdAsync(profileId), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Допоміжні Builder-методи
    // ══════════════════════════════════════════════════════════════════════

    private static Badge BuildBadge(
        Guid    id,
        string  name,
        string  description,
        decimal threshold,
        byte[]? image = null)
    {
        return new Badge
        {
            Id          = id,
            Name        = name,
            Description = description,
            Threshold   = threshold,
            Image       = image ?? Array.Empty<byte>(),
            BadgeTypeId = Guid.NewGuid(),
            BadgeType   = new BadgeType
            {
                Id   = Guid.NewGuid(),
                Name = "Тестовий тип"
            }
        };
    }

    /// <summary>Фейковий IFormFile з байтами для тестів CreateBadge.</summary>
    private static IFormFile BuildFakeFormFile()
    {
        var content  = new byte[] { 1, 2, 3, 4 };
        var stream   = new MemoryStream(content);
        var formFile = new FormFile(stream, 0, content.Length, "Image", "badge.png")
        {
            Headers     = new HeaderDictionary(),
            ContentType = "image/png"
        };
        return formFile;
    }
}
