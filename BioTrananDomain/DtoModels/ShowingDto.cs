using MongoDB.Bson.Serialization.Attributes;

namespace BioTrananDomain.DtoModels;

public class ShowingDto
{
    [BsonElement]
    public DateTime ShowingDateAndTime { get; set; }
    public string? ShowingMovieId { get; set; }
    public string? ShowingSalonId { get; set; }
}