using Moq;
using SupportWay.API.DTOs;
using SupportWay.API.Services;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Implementations;
using SupportWay.Data.Repositories.Interfaces;
using Xunit;

namespace SupportWay.Tests;

public class HelpRequestServiceTests
{
    private readonly Mock<IHelpRequestsRepository> _helpRepoMock;
    private readonly Mock<ILocationsRepository> _locationRepoMock;
    private readonly Mock<IPostLikesRepository> _postLikesRepoMock;
    private readonly HelpRequestService _sut;

    public HelpRequestServiceTests()
    {
        _helpRepoMock = new Mock<IHelpRequestsRepository>();
        _locationRepoMock = new Mock<ILocationsRepository>();
        _postLikesRepoMock = new Mock<IPostLikesRepository>();

        _sut = new HelpRequestService(
            _helpRepoMock.Object,
            _locationRepoMock.Object,
            _postLikesRepoMock.Object
        );
    }

    // ===================== GetHelpRequestByIdAsync =====================

    [Fact]
    public async Task GetHelpRequestByIdAsync_WhenRequestExists_ReturnsMappedDto()
    {
        var requestId = Guid.NewGuid();
        var helpRequest = BuildHelpRequest(requestId, "user-123", "Потрібна допомога");

        _helpRepoMock
            .Setup(r => r.GetHelpRequestByIdAsync(requestId))
            .ReturnsAsync(helpRequest);

        var result = await _sut.GetHelpRequestByIdAsync(requestId);

        Assert.NotNull(result);
        Assert.Equal(requestId, result.Id);
        Assert.Equal("Потрібна допомога", result.Content);
        Assert.Equal("user-123", result.UserId);
    }

    [Fact]
    public async Task GetHelpRequestByIdAsync_WhenRequestDoesNotExist_ReturnsNull()
    {
        _helpRepoMock
            .Setup(r => r.GetHelpRequestByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((HelpRequest?)null);

        var result = await _sut.GetHelpRequestByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ===================== Progress =====================

    [Theory]
    [InlineData(1000, 500, 50)]
    [InlineData(1000, 1000, 100)]
    [InlineData(1000, 1500, 100)]
    [InlineData(0, 500, 0)]
    public async Task GetHelpRequestDetailsAsync_CalculatesProgressCorrectly(
        decimal target, decimal collected, int expected)
    {
        var requestId = Guid.NewGuid();
        var helpRequest = BuildHelpRequest(requestId, "user-1");

        helpRequest.TargetAmount = target;
        helpRequest.CollectedAmount = collected;

        _helpRepoMock
            .Setup(r => r.GetHelpRequestByIdAsync(requestId))
            .ReturnsAsync(helpRequest);

        var result = await _sut.GetHelpRequestDetailsAsync(requestId);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ProgressPercent);
    }

    // ===================== Create =====================

    [Fact]
    public async Task CreateHelpRequestAsync_WithValidDto_ReturnsNewGuid()
    {
        var dto = new HelpRequestCreateDto
        {
            Content = "Test",
            LocationId = Guid.NewGuid()
        };

        _helpRepoMock
            .Setup(r => r.AddHelpRequestAsync(It.IsAny<HelpRequest>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateHelpRequestAsync(dto, "user-42");

        Assert.NotEqual(Guid.Empty, result);
        _helpRepoMock.Verify(r => r.AddHelpRequestAsync(It.IsAny<HelpRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateHelpRequestAsync_CreatesLocation_WhenNoLocationId()
    {
        var dto = new HelpRequestCreateDto
        {
            Content = "Test",
            Latitude = 49.8,
            Longitude = 24.0,
            Address = "Lviv"
        };

        _locationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Location>()))
            .Returns(Task.CompletedTask);

        _helpRepoMock
            .Setup(r => r.AddHelpRequestAsync(It.IsAny<HelpRequest>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateHelpRequestAsync(dto, "user-1");

        _locationRepoMock.Verify(r => r.AddAsync(It.IsAny<Location>()), Times.Once);
    }

    [Fact]
    public async Task CreateHelpRequestAsync_DoesNotCreateLocation_WhenLocationIdExists()
    {
        var dto = new HelpRequestCreateDto
        {
            Content = "Test",
            LocationId = Guid.NewGuid()
        };

        _helpRepoMock
            .Setup(r => r.AddHelpRequestAsync(It.IsAny<HelpRequest>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateHelpRequestAsync(dto, "user-1");

        _locationRepoMock.Verify(r => r.AddAsync(It.IsAny<Location>()), Times.Never);
    }

    // ===================== Delete =====================

    [Fact]
    public async Task DeleteHelpRequestAsync_CallsRepository()
    {
        var id = Guid.NewGuid();

        _helpRepoMock
            .Setup(r => r.DeleteHelpRequestAsync(id))
            .Returns(Task.CompletedTask);

        await _sut.DeleteHelpRequestAsync(id);

        _helpRepoMock.Verify(r => r.DeleteHelpRequestAsync(id), Times.Once);
    }

    // ===================== Feed =====================

    [Fact]
    public async Task GetFeedAsync_ReturnsFallback_WhenNoFollowed()
    {
        var userId = "user";

        _helpRepoMock
            .Setup(r => r.GetHelpRequestsByFollowedUsersAsync(userId, 1, 10))
            .ReturnsAsync(new List<HelpRequest>());

        _helpRepoMock
            .Setup(r => r.GetAllHelpRequestsAsync(1, 10))
            .ReturnsAsync(new List<HelpRequest>
            {
                BuildHelpRequest(Guid.NewGuid(), "other")
            });

        var result = (await _sut.GetFeedAsync(userId, 1, 10)).ToList();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetFeedAsync_DoesNotCallFallback_WhenFollowedExists()
    {
        var userId = "user";

        _helpRepoMock
            .Setup(r => r.GetHelpRequestsByFollowedUsersAsync(userId, 1, 10))
            .ReturnsAsync(new List<HelpRequest>
            {
                BuildHelpRequest(Guid.NewGuid(), "followed")
            });

        var result = (await _sut.GetFeedAsync(userId, 1, 10)).ToList();

        Assert.Single(result);

        _helpRepoMock.Verify(
            r => r.GetAllHelpRequestsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    // ===================== Helper =====================

    private static HelpRequest BuildHelpRequest(Guid id, string userId, string content = "test")
        => new()
        {
            Id = id,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            TargetAmount = 1000m,
            CollectedAmount = 0m,
            Likes = new List<PostLike>(),
            Comments = new List<PostComment>(),
            Payments = new List<Payment>(),
            RequestItems = new List<RequestItem>()
        };
}