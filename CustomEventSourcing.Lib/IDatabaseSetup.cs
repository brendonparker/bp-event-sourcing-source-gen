namespace CustomEventSourcing.Lib;

public interface IDatabaseSetup
{
    void EnsureDeleted();
    void EnsureCreated();
}