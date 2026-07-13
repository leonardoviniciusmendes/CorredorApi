using Corretor.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Corretor.Api.Tests;

public sealed class CorretorApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"corretor-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ExternalDocumentos:Enabled", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CorretorDbContext>>();
            services.AddDbContext<CorretorDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CorretorDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
