using CityInfo.API.Entities;
using CityInfo.API.Metadata;

namespace CityInfo.API.Services;

public interface ICityInfoRepository
{
    Task<IEnumerable<City>> GetCitiesAsync();
    Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageSize, int pageNumber);
    Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
    Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId);
    Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId);
    Task<bool> CityExistsAsync(int cityId);
    Task<bool> CityNameMatchesCityId(string? cityName, int cityId);
    Task CreatePointOfInterestAsync(int cityId, PointOfInterest newPointOfInterest);
    void DeletePointOfInterest(PointOfInterest pointOfInterest);
    Task<bool> SaveChangesAsync();
}
