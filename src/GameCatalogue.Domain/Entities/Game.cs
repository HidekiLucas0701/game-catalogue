using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameCatalogue.Domain.Entities;

public class Game
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Title { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public string Publisher { get; set; } = null!;
    public decimal Price { get; set; }
}