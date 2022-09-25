using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;
[Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
[Authorize(Policy = "MustBeFromNewYork")]
[ApiVersion("2.0")]
[ApiController]
public class PointsOfInterestController : ControllerBase
{
    private readonly ILogger<PointsOfInterestController> _logger;
    private readonly IMailService _mailService;
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;

    public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
        IMailService mailService,
        ICityInfoRepository cityInfoRepository,
        IMapper mapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        // Just for demo purposes
        /*var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

        if (!await _cityInfoRepository.CityNameMatchesCityId(cityName, cityId))
        {
            return Forbid();
        }*/


        var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestAsync(cityId);

        return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
    }

    [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
    public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

        if (pointOfInterest is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
    }

    [HttpPost]
    public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestCreateDto newPointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        var pointOfInterest = _mapper.Map<PointOfInterest>(newPointOfInterest);

        await _cityInfoRepository.CreatePointOfInterestAsync(cityId, pointOfInterest);
        await _cityInfoRepository.SaveChangesAsync();

        var createdPointOfInterest = _mapper.Map<PointOfInterestDto>(pointOfInterest);

        return CreatedAtRoute(
            nameof(GetPointOfInterest),
            new 
            { 
                cityId, 
                pointOfInterestId = createdPointOfInterest.Id 
            },
            createdPointOfInterest
        );
    }

    [HttpPut("{pointOfInterestId}")]
    public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestUpdateDto updatedPointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

        if (pointOfInterest is null)
        {
            return NotFound();
        }

        _mapper.Map(updatedPointOfInterest, pointOfInterest);

        await _cityInfoRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{pointOfInterestId}")]
    public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestUpdateDto> patchDocument)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

        if (pointOfInterest is null)
        {
            return NotFound();
        }

        var pointOfInterestPatch = _mapper.Map<PointOfInterestUpdateDto>(pointOfInterest);

        patchDocument.ApplyTo(pointOfInterestPatch, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(pointOfInterestPatch))
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(pointOfInterestPatch, pointOfInterest);
        await _cityInfoRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{pointOfInterestId}")]
    public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestAsync(cityId, pointOfInterestId);

        if (pointOfInterest is null)
        {
            return NotFound();
        }

        _cityInfoRepository.DeletePointOfInterest(pointOfInterest);
        await _cityInfoRepository.SaveChangesAsync();

        _mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterest.Name} with id {pointOfInterest.Id} has been deleted.");
       
        return NoContent();
    }
}
