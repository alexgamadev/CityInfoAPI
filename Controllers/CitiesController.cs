using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/cities")]
public class CitiesController : ControllerBase
{
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;
    const int MAX_CITIES_PAGE_SIZE = 20;

    public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
    {
        _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
     
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CityNoPointsOfInterestDto>>> GetCities(
        string? name, string? searchQuery,
        int pageSize = 5, int pageNumber = 0
        )
    {
        if (pageSize > MAX_CITIES_PAGE_SIZE)
        {
            pageSize = MAX_CITIES_PAGE_SIZE;
        }
        var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageSize, pageNumber);

        Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(_mapper.Map<IEnumerable<CityNoPointsOfInterestDto>>(cityEntities));
    }

    /// <summary>
    /// Get a city by id
    /// </summary>
    /// <param name="id">The id of the city</param>
    /// <param name="includePointsOfInterest">Whether to include the city's points of interest</param>
    /// <returns>An IActionResult</returns>
    /// <response code="200">Returns the requested city</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
    {
        var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

        if (city is null)
        {
            return NotFound();
        } 

        if (includePointsOfInterest)
        {
            return Ok(_mapper.Map<CityDto>(city));
        }
        else
        {
            return Ok(_mapper.Map<CityNoPointsOfInterestDto>(city));
        }
    }
}
