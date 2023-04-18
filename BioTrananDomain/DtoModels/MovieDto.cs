namespace BioTrananDomain.DtoModels;

public class MovieDto
{
    public string? MovieName { get; set; }
    public int MovieShowingsLeft { get; set; }
    public int MovieLengthInMinutes { get; set; }
    public string? MovieEmdeddedTrailerUrl { get; set; }
}