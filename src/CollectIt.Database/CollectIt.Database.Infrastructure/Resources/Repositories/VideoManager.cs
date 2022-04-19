﻿using CollectIt.Database.Abstractions.Resources;
using CollectIt.Database.Entities.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CollectIt.Database.Infrastructure.Resources.Repositories;

public class VideoManager : IVideoManager
{
    private readonly PostgresqlCollectItDbContext context;

    public VideoManager(PostgresqlCollectItDbContext context)
    {
        this.context = context;
    }

    public async Task<int> AddAsync(Video item)
    {
        await context.Videos.AddAsync(item);
        await context.SaveChangesAsync();
        return item.Id;
    }

    public async Task<Video> FindByIdAsync(int id)
    {
        return await context.Videos.Where(video => video.Id == id).SingleOrDefaultAsync();
    }

    public Task Create(int ownerId, string address, string fileName, string name, string tags, Stream uploadedFile)
    {
        throw new NotImplementedException();
    }

    public Task Create(string address, string fileName, string name, string tags, IFormFile uploadedFile)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(Video item)
    {
        context.Videos.Remove(item);
        await context.SaveChangesAsync();
    }

    public IAsyncEnumerable<Video> GetAllByQuery(string query, int pageNumber = 1, int pageSize = 15)
    {
        throw new NotImplementedException();
    }

    public Task<List<Video>> GetAllPaged(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Video> GetAllByName(string name)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Video> GetAllByTag(string tag)
    {
        throw new NotImplementedException();
    }
}