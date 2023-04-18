using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BioTrananDomain.Models;

public class Showing
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public DateTime ShowingDateAndTime { get; set; }
    public Movie? ShowingMovie { get; set; }
    public Salon? ShowingSalon { get; set; }
    public int SeatsLeft { get; set; }
}