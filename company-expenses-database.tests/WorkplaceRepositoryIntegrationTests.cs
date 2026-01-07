using CompanyExpenses.Database.Data;
using CompanyExpenses.Database.Repositories;
using CompanyExpenses.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CompanyExpenses.Database.Tests;

/// <summary>
/// Integrační testy pro WorkplaceRepository - testování ukládání projektu do databáze
/// </summary>
public class WorkplaceRepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly WorkplaceRepository _repository;

    public WorkplaceRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new WorkplaceRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_NewWorkplace_SavesToDatabase()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "Test Projekt",
            Code = "TP001",
            IsActive = true,
            CreatedBy = "user-123",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(workplace);
        await _repository.SaveChangesAsync();

        // Assert
        var savedWorkplace = await _context.Workplaces.FindAsync(workplace.Id);
        savedWorkplace.Should().NotBeNull();
        savedWorkplace!.Name.Should().Be("Test Projekt");
        savedWorkplace.Code.Should().Be("TP001");
        savedWorkplace.IsActive.Should().BeTrue();
        savedWorkplace.CreatedBy.Should().Be("user-123");
    }

    [Fact]
    public async Task AddAsync_MultipleWorkplaces_AllAreSaved()
    {
        // Arrange
        var workplaces = new List<Workplace>
        {
            new() { Id = Guid.NewGuid(), Name = "Projekt 1", CreatedBy = "user-1" },
            new() { Id = Guid.NewGuid(), Name = "Projekt 2", CreatedBy = "user-2" },
            new() { Id = Guid.NewGuid(), Name = "Projekt 3", CreatedBy = "user-3" }
        };

        // Act
        foreach (var wp in workplaces)
        {
            await _repository.AddAsync(wp);
        }
        await _repository.SaveChangesAsync();

        // Assert
        var count = await _context.Workplaces.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task AddAsync_WorkplaceWithNullCode_SavesSuccessfully()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "Project Without Code",
            Code = null,
            IsActive = true,
            CreatedBy = "user-test"
        };

        // Act
        await _repository.AddAsync(workplace);
        await _repository.SaveChangesAsync();

        // Assert
        var saved = await _context.Workplaces.FindAsync(workplace.Id);
        saved.Should().NotBeNull();
        saved!.Code.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingWorkplace_ReturnsWorkplace()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "Existing Projekt",
            CreatedBy = "user-abc"
        };
        await _context.Workplaces.AddAsync(workplace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(workplace.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workplace.Id);
        result.Name.Should().Be("Existing Projekt");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithDetailsAsync Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WorkplaceWithMembers_IncludesMembers()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace
        {
            Id = workplaceId,
            Name = "Projekt s členy",
            CreatedBy = "creator"
        };
        await _context.Workplaces.AddAsync(workplace);

        var members = new List<WorkplaceMember>
        {
            new() { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "user-1", PositionName = "Developer", IsManager = false, CreatedBy = "admin" },
            new() { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "user-2", PositionName = "Manager", IsManager = true, CreatedBy = "admin" }
        };
        await _context.WorkplaceMembers.AddRangeAsync(members);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(workplaceId);

        // Assert
        result.Should().NotBeNull();
        result!.Members.Should().HaveCount(2);
        result.Members.Should().Contain(m => m.UserId == "user-1");
        result.Members.Should().Contain(m => m.UserId == "user-2");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WorkplaceWithLimits_IncludesLimits()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var workplace = new Workplace
        {
            Id = workplaceId,
            Name = "Projekt s limity",
            CreatedBy = "creator"
        };
        await _context.Workplaces.AddAsync(workplace);

        var limits = new List<WorkplaceLimit>
        {
            new() { Id = Guid.NewGuid(), WorkplaceId = workplaceId, LimitAmount = 10000, Currency = "CZK", PeriodFrom = DateOnly.FromDateTime(DateTime.Today), PeriodTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), CreatedBy = "admin" },
            new() { Id = Guid.NewGuid(), WorkplaceId = workplaceId, LimitAmount = 5000, Currency = "EUR", PeriodFrom = DateOnly.FromDateTime(DateTime.Today), PeriodTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), CreatedBy = "admin" }
        };
        await _context.WorkplaceLimits.AddRangeAsync(limits);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(workplaceId);

        // Assert
        result.Should().NotBeNull();
        result!.Limits.Should().HaveCount(2);
        result.Limits.Should().Contain(l => l.Currency == "CZK");
        result.Limits.Should().Contain(l => l.Currency == "EUR");
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_NonExistingWorkplace_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ExistingWorkplace_UpdatesData()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Code = "ORIG",
            IsActive = true,
            CreatedBy = "user-test"
        };
        await _context.Workplaces.AddAsync(workplace);
        await _context.SaveChangesAsync();

        // Act
        workplace.Name = "Updated Name";
        workplace.Code = "UPDT";
        workplace.IsActive = false;
        _repository.Update(workplace);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _context.Workplaces.FindAsync(workplace.Id);
        updated!.Name.Should().Be("Updated Name");
        updated.Code.Should().Be("UPDT");
        updated.IsActive.Should().BeFalse();
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ExistingWorkplace_DeletesFromDatabase()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            CreatedBy = "user-del"
        };
        await _context.Workplaces.AddAsync(workplace);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(workplace);
        await _repository.SaveChangesAsync();

        // Assert
        var deleted = await _context.Workplaces.FindAsync(workplace.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region GetAllWithMembersAsync Tests

    [Fact]
    public async Task GetAllWithMembersAsync_MultipleWorkplacesWithMembers_ReturnsAll()
    {
        // Arrange
        var wp1Id = Guid.NewGuid();
        var wp2Id = Guid.NewGuid();

        await _context.Workplaces.AddRangeAsync(
            new Workplace { Id = wp1Id, Name = "Workplace 1", CreatedBy = "user" },
            new Workplace { Id = wp2Id, Name = "Workplace 2", CreatedBy = "user" }
        );

        await _context.WorkplaceMembers.AddRangeAsync(
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = wp1Id, UserId = "u1", CreatedBy = "admin" },
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = wp1Id, UserId = "u2", CreatedBy = "admin" },
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = wp2Id, UserId = "u3", CreatedBy = "admin" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllWithMembersAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.First(w => w.Id == wp1Id).Members.Should().HaveCount(2);
        result.First(w => w.Id == wp2Id).Members.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllWithMembersAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllWithMembersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetDependenciesAsync Tests

    [Fact]
    public async Task GetDependenciesAsync_WorkplaceWithNoDependencies_ReturnsZeroCounts()
    {
        // Arrange
        var workplace = new Workplace
        {
            Id = Guid.NewGuid(),
            Name = "Empty Projekt",
            CreatedBy = "user"
        };
        await _context.Workplaces.AddAsync(workplace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetDependenciesAsync(workplace.Id);

        // Assert
        result.MembersCount.Should().Be(0);
        result.LimitsCount.Should().Be(0);
        result.InvitationsCount.Should().Be(0);
        result.ExpensesCount.Should().Be(0);
        result.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task GetDependenciesAsync_WorkplaceWithMembers_ReturnsCorrectCount()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        await _context.Workplaces.AddAsync(new Workplace { Id = workplaceId, Name = "With Members", CreatedBy = "user" });

        await _context.WorkplaceMembers.AddRangeAsync(
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "u1", CreatedBy = "admin" },
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "u2", CreatedBy = "admin" },
            new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "u3", CreatedBy = "admin" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetDependenciesAsync(workplaceId);

        // Assert
        result.MembersCount.Should().Be(3);
        result.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GetDependenciesAsync_WorkplaceWithExpenses_ReturnsCorrectCount()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        await _context.Workplaces.AddAsync(new Workplace { Id = workplaceId, Name = "With Expenses", CreatedBy = "user" });
        await _context.ExpenseCategories.AddAsync(new ExpenseCategory { Id = categoryId, Name = "Category" });

        await _context.Expenses.AddRangeAsync(
            new Expense { Id = Guid.NewGuid(), WorkplaceId = workplaceId, CategoryId = categoryId, Amount = 100, Description = "E1", ExpenseDate = DateOnly.FromDateTime(DateTime.Today), CreatedBy = "user" },
            new Expense { Id = Guid.NewGuid(), WorkplaceId = workplaceId, CategoryId = categoryId, Amount = 200, Description = "E2", ExpenseDate = DateOnly.FromDateTime(DateTime.Today), CreatedBy = "user" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetDependenciesAsync(workplaceId);

        // Assert
        result.ExpensesCount.Should().Be(2);
        result.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GetDependenciesAsync_WorkplaceWithAllDependencies_CanDeleteIsFalse()
    {
        // Arrange
        var workplaceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        await _context.Workplaces.AddAsync(new Workplace { Id = workplaceId, Name = "Full Projekt", CreatedBy = "user" });
        await _context.ExpenseCategories.AddAsync(new ExpenseCategory { Id = categoryId, Name = "Category" });

        // Members
        await _context.WorkplaceMembers.AddAsync(new WorkplaceMember { Id = Guid.NewGuid(), WorkplaceId = workplaceId, UserId = "u1", CreatedBy = "admin" });

        // Limits
        await _context.WorkplaceLimits.AddAsync(new WorkplaceLimit { Id = Guid.NewGuid(), WorkplaceId = workplaceId, LimitAmount = 10000, Currency = "CZK", PeriodFrom = DateOnly.FromDateTime(DateTime.Today), PeriodTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), CreatedBy = "admin" });

        // Invitations
        await _context.Invitations.AddAsync(new Invitation { Id = Guid.NewGuid(), WorkplaceId = workplaceId, Email = "test@test.com", Token = "token123", CreatedBy = "admin", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        // Expenses
        await _context.Expenses.AddAsync(new Expense { Id = Guid.NewGuid(), WorkplaceId = workplaceId, CategoryId = categoryId, Amount = 100, Description = "E1", ExpenseDate = DateOnly.FromDateTime(DateTime.Today), CreatedBy = "user" });

        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetDependenciesAsync(workplaceId);

        // Assert
        result.MembersCount.Should().Be(1);
        result.LimitsCount.Should().Be(1);
        result.InvitationsCount.Should().Be(1);
        result.ExpensesCount.Should().Be(1);
        result.CanDelete.Should().BeFalse();
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task Query_CanFilterByIsActive()
    {
        // Arrange
        await _context.Workplaces.AddRangeAsync(
            new Workplace { Id = Guid.NewGuid(), Name = "Active 1", IsActive = true, CreatedBy = "user" },
            new Workplace { Id = Guid.NewGuid(), Name = "Active 2", IsActive = true, CreatedBy = "user" },
            new Workplace { Id = Guid.NewGuid(), Name = "Inactive 1", IsActive = false, CreatedBy = "user" }
        );
        await _context.SaveChangesAsync();

        // Act
        var activeWorkplaces = await _repository.Query()
            .Where(w => w.IsActive)
            .ToListAsync();

        // Assert
        activeWorkplaces.Should().HaveCount(2);
        activeWorkplaces.Should().OnlyContain(w => w.IsActive);
    }

    [Fact]
    public async Task FindAsync_ByCode_ReturnsMatchingWorkplaces()
    {
        // Arrange
        await _context.Workplaces.AddRangeAsync(
            new Workplace { Id = Guid.NewGuid(), Name = "Project A", Code = "PRJ-A", CreatedBy = "user" },
            new Workplace { Id = Guid.NewGuid(), Name = "Project B", Code = "PRJ-B", CreatedBy = "user" },
            new Workplace { Id = Guid.NewGuid(), Name = "Project C", Code = "PRJ-A", CreatedBy = "user" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(w => w.Code == "PRJ-A");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion
}
