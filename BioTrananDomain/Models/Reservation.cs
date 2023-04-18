using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BioTrananDomain.Models;

public class Reservation
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public Showing? ReservationShowing { get; set; }
    public string Email { get; set; }
    public int AmountOfSeats { get; set; }
}