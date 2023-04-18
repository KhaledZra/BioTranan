using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BioTrananDomain.Models;

public class Salon
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? SalonName { get; set; }
    public int SalonSeatAmount { get; set; }
}