namespace Microsoft.Extensions.DependencyInjection;

public interface IDatabaseSetup
{
    void EnsureDeleted();
    void EnsureCreated();
}