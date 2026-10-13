
using GameCatalogue.Application.Interfaces;
using GameCatalogue.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GameCatalogue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetAll()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetById(string id)
    {
        var game = await _gameService.GetGameByIdAsync(id);

        if (game is null)
            return NotFound();

        return Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Game game)
    {
        await _gameService.CreateGameAsync(game);

        return CreatedAtAction(
            nameof(GetById),
            new { id = game.Id },
            game);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Game>> Update(
        string id,
        Game updatedGame)
    {
        var existingGame = await _gameService.GetGameByIdAsync(id);

        if (existingGame is null)
            return NotFound();

        updatedGame.Id = id;

        var game = await _gameService.UpdateGameAsync(id, updatedGame);

        return Ok(game);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existingGame = await _gameService.GetGameByIdAsync(id);

        if (existingGame is null)
            return NotFound();

        await _gameService.DeleteGameAsync(id);

        return NoContent();
    }

    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<List<Game>>> GetByGenre(string genre)
    {
        var games = await _gameService.GetGamesByGenreAsync(genre);
        return Ok(games);
    }

    [HttpGet("platform/{platform}")]
    public async Task<ActionResult<List<Game>>> GetByPlatform(string platform)
    {
        var games = await _gameService.GetGamesByPlatformAsync(platform);
        return Ok(games);
    }

    [HttpGet("average-price-by-genre")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetAveragePriceByGenre()
    {
        var result = await _gameService.GetAveragePriceByGenreAsync();
        return Ok(result);
    }
}
