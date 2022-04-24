namespace CollectIt.Database.Infrastructure;

public interface IResourceFileManager
{
    public Stream GetContent(string filename);
    public void Delete(string filename);
    
    public Task<FileInfo> CreateAsync(string filename, Stream content);
}