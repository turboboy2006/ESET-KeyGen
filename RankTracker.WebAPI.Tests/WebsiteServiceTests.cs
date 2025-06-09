using Microsoft.EntityFrameworkCore;
using Moq;
using RankTracker.Core.Data;
using RankTracker.Core.Models;
using RankTracker.WebAPI.DTOs;
using RankTracker.WebAPI.Services;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Collections.Generic;

public class WebsiteServiceTests
{
    private RankTrackerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<RankTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique name for each test run
            .Options;
        var dbContext = new RankTrackerDbContext(options);
        // Seed data if necessary for specific tests
        return dbContext;
    }

    [Fact]
    public async Task CreateWebsiteAsync_ShouldAddWebsiteAndReturnDto()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var service = new WebsiteService(dbContext);
        var createDto = new CreateWebsiteDto { Name = "Test Site", Url = "https://test.com" };

        // Act
        var resultDto = await service.CreateWebsiteAsync(createDto);

        // Assert
        Assert.NotNull(resultDto);
        Assert.Equal("Test Site", resultDto.Name);
        Assert.Equal("https://test.com", resultDto.Url);
        Assert.True(resultDto.Id > 0); // Id should be generated

        var websiteInDb = await dbContext.Websites.FindAsync(resultDto.Id);
        Assert.NotNull(websiteInDb);
        Assert.Equal("Test Site", websiteInDb.Name);
    }

    [Fact]
    public async Task GetWebsiteByIdAsync_ShouldReturnCorrectWebsiteDto_WhenExists()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var seedWebsite = new Website { Id = 1, Name = "Seed Site", Url = "https://seed.com" };
        dbContext.Websites.Add(seedWebsite);
        await dbContext.SaveChangesAsync();

        var service = new WebsiteService(dbContext);

        // Act
        var result = await service.GetWebsiteByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(seedWebsite.Name, result.Name);
        Assert.Equal(seedWebsite.Url, result.Url);
    }

    [Fact]
    public async Task GetWebsiteByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var service = new WebsiteService(dbContext);

        // Act
        var result = await service.GetWebsiteByIdAsync(99); // Non-existent ID

        // Assert
        Assert.Null(result);
    }

    // Add more tests for UpdateWebsiteAsync, DeleteWebsiteAsync, GetAllWebsitesAsync
}
