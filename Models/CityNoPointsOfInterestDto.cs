namespace CityInfo.API.Models;

/// <summary>
/// A DTO for a city without points of interest
/// </summary>
public class CityNoPointsOfInterestDto
{
    /// <summary>
    /// The id of the city
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The city name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of the city
    /// </summary>
    public string? Description { get; set; }
}
