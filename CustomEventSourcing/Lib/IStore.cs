namespace CustomEventSourcing;

public interface IStore
{
    IStream StartStream(Guid id);
    Task<IStream> LoadStreamAsync(Guid id);
    Task SaveChangesAsync();
}