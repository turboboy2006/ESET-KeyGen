using Microsoft.AspNetCore.Mvc.Testing;
using RankTracker.WebAPI; // Your WebAPI project namespace
using RankTracker.WebAPI.DTOs;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json; // For PostAsJsonAsync, ReadFromJsonAsync
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory
using RankTracker.Core.Data; // For RankTrackerDbContext
using System.Linq; // For Linq queries
using Microsoft.EntityFrameworkCore; // For UseInMemoryDatabase

public class WebsitesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>> // Use Program (NET 6+) or Startup
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebsitesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RankTrackerDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add DbContext using an in-memory database for testing.
                services.AddDbContext<RankTrackerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForIntegrationTesting"); // Consistent name for shared InMem DB
                });

                // Optionally: Seed data or ensure clean state
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<RankTrackerDbContext>();
                    db.Database.EnsureCreated(); // Creates schema for InMem
                    // It's often better to ensure a clean state for each test or test class,
                    // rather than relying on a globally shared in-memory database that might carry over state.
                    // For this example, EnsureCreated is fine. Consider db.Database.EnsureDeleted() then EnsureCreated()
                    // or unique DB names per test if state becomes an issue.
                }
            });
        });
    }

    [Fact]
    public async Task PostWebsite_ShouldCreateWebsiteAndReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateWebsiteDto { Name = "Integration Test Site", Url = "https://integration.example.com" };

        // Act
        var response = await client.PostAsJsonAsync("/api/websites", createDto);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var returnedDto = await response.Content.ReadFromJsonAsync<WebsiteDto>();
        Assert.NotNull(returnedDto);
        Assert.Equal(createDto.Name, returnedDto.Name);
        Assert.Equal(createDto.Url, returnedDto.Url);
        Assert.True(returnedDto.Id > 0);

        // Verify in database (optional, but good for full integration check)
        // Note: This uses the same in-memory database instance due to WebApplicationFactory's service configuration.
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RankTrackerDbContext>();
            var websiteInDb = await dbContext.Websites.FindAsync(returnedDto.Id);
            Assert.NotNull(websiteInDb);
            Assert.Equal(createDto.Name, websiteInDb.Name);
        }
    }
}
