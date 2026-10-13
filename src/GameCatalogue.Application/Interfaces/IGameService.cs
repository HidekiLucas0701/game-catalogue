using GameCatalogue.Domain.Entities;

namespace GameCatalogue.Application.Interfaces;

public interface IGameService
{
    Task<List<Game>> GetAllGamesAsync();
    Task<Game?> GetGameByIdAsync(string id);
    Task CreateGameAsync(Game game);
    Task<Game> UpdateGameAsync(string id, Game updatedGame);
    Task DeleteGameAsync(string id);
    Task<List<Game>> GetGamesByGenreAsync(string genre);
    Task<List<Game>> GetGamesByPlatformAsync(string platform);
    Task<Dictionary<string, decimal>> GetAveragePriceByGenreAsync();
}
