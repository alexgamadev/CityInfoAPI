using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Metadata;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services;

public class CityInfoRepository : ICityInfoRepository
{
    private readonly CityInfoContext _context;

    public CityInfoRepository(CityInfoContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> CityExistsAsync(int cityId)
    {
        return await _context.Cities.AnyAsync(c => c.Id == cityId);
    }

    public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
    {
        return await _context.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
    }

    public async Task CreatePointOfInterestAsync(int cityId, PointOfInterest newPointOfInterest)
    {
        var city = await GetCityAsync(cityId, false);
        if (city != null)
        {
            city.PointsOfInterest.Add(newPointOfInterest);
        }
    }

    public void DeletePointOfInterest(PointOfInterest pointOfInterest)
    {
        _context.PointsOfInterest.Remove(pointOfInterest);
    }

    public async Task<IEnumerable<City>> GetCitiesAsync()
    {
        return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageSize, int pageNumber)
    {
        var collection = _context.Cities as IQueryable<City>;

        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            collection = collection.Where(c => c.Name == name);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            searchQuery = searchQuery.Trim();
            searchQuery = searchQuery.ToLower();
            collection = collection.Where(c => c.Name.ToLower().Contains(searchQuery) 
            || (c.Description != null && c.Description.ToLower().Contains(searchQuery)));
        }

        var totalItemCount = await collection.CountAsync();

        var paginationMetadata = new PaginationMetadata(
            totalItemCount, pageSize, pageNumber);

        var result = await collection
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync();

        return (result, paginationMetadata);
    }

    public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
    {
        if (includePointsOfInterest)
        {
            return await _context.Cities
                .Include(c => c.PointsOfInterest)
                .Where(c => c.Id == cityId)
                .FirstOrDefaultAsync();
        }
        else
        {
            return await _context.Cities
                .Where(c => c.Id == cityId)
                .FirstOrDefaultAsync();
        }
    }

    public async Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId)
    {
        return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId)
    {
        return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId)
                .ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync() >= 0);
    }
}
