﻿using CollectIt.Database.Abstractions.Resources;
using CollectIt.Database.Entities.Resources;
using Microsoft.EntityFrameworkCore;

namespace CollectIt.Database.Infrastructure.Resources.Repositories;

public class MusicRepository /*: IMusicRepository*/
{
    private readonly PostgresqlCollectItDbContext context;
    private readonly ResourceRepository resourceRepository;

    public MusicRepository(PostgresqlCollectItDbContext context)
    {
        this.context = context;
        resourceRepository = new ResourceRepository(context);
    }

    public async Task<int> AddAsync(Music item, Resource resource)
    {
        await context.Musics.AddAsync(item);
        await context.SaveChangesAsync();
        await resourceRepository.AddAsync(resource);
        return item.MusicId;
    }

    public async Task<Music> FindByIdAsync(int id)
    {
        return await context.Musics.Where(music => music.MusicId == id).SingleOrDefaultAsync();
    }

    public Task UpdateAsync(Music item)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(Music item, Resource resource)
    {
        context.Musics.Remove(item);
        await resourceRepository.RemoveAsync(resource);
        await context.SaveChangesAsync();
    }

    public IAsyncEnumerable<Music> GetAllByName(string name)
    {
        throw new NotImplementedException();
    }
}