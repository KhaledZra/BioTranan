using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BioTrananDomain.Models;

public class Movie
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? MovieName { get; set; }
    public int MovieShowingsLeft { get; set; }
    public int MovieLengthInMinutes { get; set; }
    public string? MovieEmbeddedTrailerUrl { get; set; }
}