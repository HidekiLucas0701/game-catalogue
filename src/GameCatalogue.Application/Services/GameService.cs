
using GameCatalogue.Domain.Entities;
using GameCatalogue.Application.Interfaces;


namespace GameCatalogue.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;

    public GameService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _repository.GetAllGamesAsync();
    }

    public async Task<Game?> GetGameByIdAsync(string id)
    {
        return await _repository.GetGameByIdAsync(id);
    }

    public async Task CreateGameAsync(Game game)
    {
        await _repository.CreateGameAsync(game);
    }

    public async Task<Game> UpdateGameAsync(string id, Game updatedGame)
    {
        return await _repository.UpdateGameAsync(id, updatedGame);
    }

    public async Task DeleteGameAsync(string id)
    {
        await _repository.DeleteGameAsync(id);
    }

    public async Task<List<Game>> GetGamesByGenreAsync(string genre)
    {
        return await _repository.GetGamesByGenreAsync(genre);
    }

    public async Task<List<Game>> GetGamesByPlatformAsync(string platform)
    {
        return await _repository.GetGamesByPlatformAsync(platform);
    }

    public async Task<Dictionary<string, decimal>> GetAveragePriceByGenreAsync()
    {
        return await _repository.GetAveragePriceByGenreAsync();
    }
}
