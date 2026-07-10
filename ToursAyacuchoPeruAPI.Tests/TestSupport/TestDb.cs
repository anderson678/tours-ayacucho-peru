using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Tests.TestSupport;

internal static class TestDb
{
    public static ToursAyacuchoPeruDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ToursAyacuchoPeruDbContext>()
            .UseInMemoryDatabase($"ToursAyacuchoPeruTests-{Guid.NewGuid():N}")
            .Options;

        return new ToursAyacuchoPeruDbContext(options);
    }
}
