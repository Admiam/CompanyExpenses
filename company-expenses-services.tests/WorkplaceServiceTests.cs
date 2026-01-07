using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using CompanyExpenses.Services.Common;
using CompanyExpenses.Services.DTOs;
using CompanyExpenses.Services.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CompanyExpenses.Services.Tests;

/// <summary>
/// Unit testy pro WorkplaceService - business logika pro vytváření a správu projektů (workplaces)
/// </summary>
public class WorkplaceServiceTests
{
    private readonly Mock<IWorkplaceRepository> _mockWorkplaceRepository;
    private readonly Mock<ILogger<WorkplaceService>> _mockLogger;
    private readonly WorkplaceService _workplaceService;

    public WorkplaceServiceTests()
    {
        _mockWorkplaceRepository = new Mock<IWorkplaceRepository>();
        _mockLogger = new Mock<ILogger<WorkplaceService>>();
        _workplaceService = new WorkplaceService(
            _mockWorkplaceRepository.Object,
            _mockLogger.Object);
    }

    #region CreateWorkplaceAsync Tests

    [Fact]
    public async Task CreateWorkplaceAsync_WithValidData_ReturnsSuccessWithWorkplace()
    {
        // Arrange
        var createDto = new CreateWorkplaceDto
        {
            Name = "Test Projekt",
            Code = "TP001",
            IsActive = true
        };
        var userId = "user-123";

        _mockWorkplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<Workplace>()))
            .Returns(Task.CompletedTask);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.CreateWorkplaceAsync(createDto, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Projekt");
        result.Data.Code.Should().Be("TP001");
        result.Data.IsActive.Should().BeTrue();
        result.Data.CreatedBy.Should().Be(userId);
        result.Data.Id.Should().NotBeEmpty();

        _mockWorkplaceRepository.Verify(r => r.AddAsync(It.Is<Workplace>(w =>
            w.Name == "Test Projekt" &&
            w.Code == "TP001" &&
            w.IsActive == true &&
            w.CreatedBy == userId)), Times.Once);
        _mockWorkplaceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateWorkplaceAsync_WithMinimalData_ReturnsSuccessWithDefaults()
    {
        // Arrange
        var createDto = new CreateWorkplaceDto
        {
            Name = "Minimal Project"
        };
        var userId = "user-456";

        _mockWorkplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<Workplace>()))
            .Returns(Task.CompletedTask);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.CreateWorkplaceAsync(createDto, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Minimal Project");
        result.Data.Code.Should().BeNull();
        result.Data.Members.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateWorkplaceAsync_GeneratesUniqueId()
    {
        // Arrange
        var createDto = new CreateWorkplaceDto { Name = "Unique ID Test" };
        var userId = "user-789";
        Workplace? capturedWorkplace = null;

        _mockWorkplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<Workplace>()))
            .Callback<Workplace>(w => capturedWorkplace = w)
            .Returns(Task.CompletedTask);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.CreateWorkplaceAsync(createDto, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedWorkplace.Should().NotBeNull();
        capturedWorkplace!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateWorkplaceAsync_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var createDto = new CreateWorkplaceDto { Name = "Timestamp Test" };
        var userId = "user-test";
        var beforeCreate = DateTime.UtcNow;

        _mockWorkplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<Workplace>()))
            .Returns(Task.CompletedTask);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.CreateWorkplaceAsync(createDto, userId);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.Data.CreatedAt.Should().BeOnOrBefore(afterCreate);
    }

    #endregion

    #region GetWorkplaceByIdAsync Tests

    [Fact]
    public async Task GetWorkplaceByIdAsync_WhenWorkplaceExists_ReturnsSuccess()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace
        {
            Id = workplaceId,
            Name = "Existing Workplace",
            Code = "EW001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "creator-user",
            Members = new List<WorkplaceMember>(),
            Limits = new List<WorkplaceLimit>()
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdWithDetailsAsync(workplaceId))
            .ReturnsAsync(workplace);

        // Act
        var result = await _workplaceService.GetWorkplaceByIdAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(workplaceId);
        result.Data.Name.Should().Be("Existing Workplace");
    }

    [Fact]
    public async Task GetWorkplaceByIdAsync_WhenWorkplaceNotFound_ReturnsNotFound()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        _mockWorkplaceRepository
            .Setup(r => r.GetByIdWithDetailsAsync(workplaceId))
            .ReturnsAsync((Workplace?)null);

        // Act
        var result = await _workplaceService.GetWorkplaceByIdAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ServiceErrorType.NotFound);
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetWorkplaceByIdAsync_IncludesMembersAndLimits()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace
        {
            Id = workplaceId,
            Name = "Workplace With Relations",
            Members = new List<WorkplaceMember>
            {
                new() { Id = Guid.NewGuid(), UserId = "member-1", PositionName = "Developer", IsManager = false },
                new() { Id = Guid.NewGuid(), UserId = "member-2", PositionName = "Manager", IsManager = true }
            },
            Limits = new List<WorkplaceLimit>
            {
                new() { Id = Guid.NewGuid(), LimitAmount = 10000, Currency = "CZK" }
            }
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdWithDetailsAsync(workplaceId))
            .ReturnsAsync(workplace);

        // Act
        var result = await _workplaceService.GetWorkplaceByIdAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Members.Should().HaveCount(2);
        result.Data.Limits.Should().HaveCount(1);
    }

    #endregion

    #region UpdateWorkplaceAsync Tests

    [Fact]
    public async Task UpdateWorkplaceAsync_WhenWorkplaceExists_ReturnsSuccess()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var existingWorkplace = new Workplace
        {
            Id = workplaceId,
            Name = "Old Name",
            Code = "OLD",
            IsActive = true
        };
        var updateDto = new UpdateWorkplaceDto
        {
            Name = "New Name",
            Code = "NEW",
            IsActive = false
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(existingWorkplace);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.UpdateWorkplaceAsync(workplaceId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingWorkplace.Name.Should().Be("New Name");
        existingWorkplace.Code.Should().Be("NEW");
        existingWorkplace.IsActive.Should().BeFalse();
        _mockWorkplaceRepository.Verify(r => r.Update(existingWorkplace), Times.Once);
    }

    [Fact]
    public async Task UpdateWorkplaceAsync_WhenWorkplaceNotFound_ReturnsNotFound()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var updateDto = new UpdateWorkplaceDto { Name = "Any" };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync((Workplace?)null);

        // Act
        var result = await _workplaceService.UpdateWorkplaceAsync(workplaceId, updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ServiceErrorType.NotFound);
    }

    #endregion

    #region DeleteWorkplaceAsync Tests

    [Fact]
    public async Task DeleteWorkplaceAsync_WhenNoDependencies_ReturnsSuccess()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace { Id = workplaceId, Name = "To Delete" };
        var noDeps = new WorkplaceDependencies
        {
            MembersCount = 0,
            LimitsCount = 0,
            InvitationsCount = 0,
            ExpensesCount = 0
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(workplace);
        _mockWorkplaceRepository
            .Setup(r => r.GetDependenciesAsync(workplaceId))
            .ReturnsAsync(noDeps);
        _mockWorkplaceRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workplaceService.DeleteWorkplaceAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockWorkplaceRepository.Verify(r => r.Remove(workplace), Times.Once);
        _mockWorkplaceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteWorkplaceAsync_WhenHasMembers_ReturnsBadRequest()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace { Id = workplaceId, Name = "With Members" };
        var deps = new WorkplaceDependencies
        {
            MembersCount = 5,
            LimitsCount = 0,
            InvitationsCount = 0,
            ExpensesCount = 0
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(workplace);
        _mockWorkplaceRepository
            .Setup(r => r.GetDependenciesAsync(workplaceId))
            .ReturnsAsync(deps);

        // Act
        var result = await _workplaceService.DeleteWorkplaceAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ServiceErrorType.BadRequest);
        result.ErrorMessage.Should().Contain("Members: 5");
        _mockWorkplaceRepository.Verify(r => r.Remove(It.IsAny<Workplace>()), Times.Never);
    }

    [Fact]
    public async Task DeleteWorkplaceAsync_WhenHasExpenses_ReturnsBadRequest()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace { Id = workplaceId, Name = "With Expenses" };
        var deps = new WorkplaceDependencies
        {
            MembersCount = 0,
            LimitsCount = 0,
            InvitationsCount = 0,
            ExpensesCount = 10
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(workplace);
        _mockWorkplaceRepository
            .Setup(r => r.GetDependenciesAsync(workplaceId))
            .ReturnsAsync(deps);

        // Act
        var result = await _workplaceService.DeleteWorkplaceAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ServiceErrorType.BadRequest);
        result.ErrorMessage.Should().Contain("Expenses: 10");
    }

    [Fact]
    public async Task DeleteWorkplaceAsync_WhenWorkplaceNotFound_ReturnsNotFound()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync((Workplace?)null);

        // Act
        var result = await _workplaceService.DeleteWorkplaceAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ServiceErrorType.NotFound);
    }

    #endregion

    #region GetDependenciesAsync Tests

    [Fact]
    public async Task GetDependenciesAsync_WhenWorkplaceExists_ReturnsCorrectCounts()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace { Id = workplaceId };
        var deps = new WorkplaceDependencies
        {
            MembersCount = 3,
            LimitsCount = 2,
            InvitationsCount = 1,
            ExpensesCount = 15
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(workplace);
        _mockWorkplaceRepository
            .Setup(r => r.GetDependenciesAsync(workplaceId))
            .ReturnsAsync(deps);

        // Act
        var result = await _workplaceService.GetDependenciesAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.WorkplaceId.Should().Be(workplaceId);
        result.Data.MembersCount.Should().Be(3);
        result.Data.LimitsCount.Should().Be(2);
        result.Data.InvitationsCount.Should().Be(1);
        result.Data.ExpensesCount.Should().Be(15);
        result.Data.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GetDependenciesAsync_WhenNoDependencies_CanDeleteIsTrue()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace { Id = workplaceId };
        var deps = new WorkplaceDependencies
        {
            MembersCount = 0,
            LimitsCount = 0,
            InvitationsCount = 0,
            ExpensesCount = 0
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetByIdAsync(workplaceId))
            .ReturnsAsync(workplace);
        _mockWorkplaceRepository
            .Setup(r => r.GetDependenciesAsync(workplaceId))
            .ReturnsAsync(deps);

        // Act
        var result = await _workplaceService.GetDependenciesAsync(workplaceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CanDelete.Should().BeTrue();
    }

    #endregion

    #region GetAllWorkplacesAsync Tests

    [Fact]
    public async Task GetAllWorkplacesAsync_ReturnsAllWorkplaces()
    {
        // Arrange
        var workplaces = new List<Workplace>
        {
            new() { Id = Guid.NewGuid(), Name = "Workplace 1", Members = new List<WorkplaceMember>() },
            new() { Id = Guid.NewGuid(), Name = "Workplace 2", Members = new List<WorkplaceMember>() },
            new() { Id = Guid.NewGuid(), Name = "Workplace 3", Members = new List<WorkplaceMember>() }
        };

        _mockWorkplaceRepository
            .Setup(r => r.GetAllWithMembersAsync())
            .ReturnsAsync(workplaces);

        // Act
        var result = await _workplaceService.GetAllWorkplacesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllWorkplacesAsync_WhenNoWorkplaces_ReturnsEmptyList()
    {
        // Arrange
        _mockWorkplaceRepository
            .Setup(r => r.GetAllWithMembersAsync())
            .ReturnsAsync(new List<Workplace>());

        // Act
        var result = await _workplaceService.GetAllWorkplacesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion
}
