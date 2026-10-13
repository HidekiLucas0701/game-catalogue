using GameCatalogue.Domain.Entities;
using GameCatalogue.Infrastructure.Data;
using MongoDB.Driver;
using GameCatalogue.Application.Interfaces;

namespace GameCatalogue.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly MongoDbContext _context;

    public GameRepository(MongoDbContext context)
    {
        _context = context;
    }

    public IMongoCollection<Game> GetGamesCollection()
    {
        return _context.GetCollection<Game>("Games");
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        var collection = GetGamesCollection();
        return await collection.Find(_ => true).ToListAsync();
    }

    public async Task<Game?> GetGameByIdAsync(string id)
    {
        var collection = GetGamesCollection();
        return await collection.Find(game => game.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateGameAsync(Game game)
    {
        var collection = GetGamesCollection();
        await collection.InsertOneAsync(game);
    }

    public async Task<Game> UpdateGameAsync(string id, Game updatedGame)
    {
        var collection = GetGamesCollection();
        await collection.ReplaceOneAsync(game => game.Id == id, updatedGame);

        return updatedGame;
    }

    public async Task DeleteGameAsync(string id)
    {
        var collection = GetGamesCollection();
        await collection.DeleteOneAsync(game => game.Id == id);
    }

    public async Task<List<Game>> GetGamesByGenreAsync(string genre)
    {
        var collection = GetGamesCollection();
        return await collection.Find(game => game.Genre == genre).ToListAsync();
    }

    public async Task<List<Game>> GetGamesByPlatformAsync(string platform)
    {
        var collection = GetGamesCollection();
        return await collection.Find(game => game.Platform == platform).ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetAveragePriceByGenreAsync()
    {
        var collection = GetGamesCollection();

        var result = await collection.Aggregate()
            .Group(
                game => game.Genre,
                group => new
                {
                    Genre = group.Key,
                    AveragePrice = group.Average(game => game.Price)
                })
            .ToListAsync();

        return result.ToDictionary(
            item => item.Genre,
            item => item.AveragePrice);
    }
}