using Microsoft.AspNetCore.Mvc;
using WprResults.Models;
using WprResults.ResultsProviders.Parkrun;

namespace WprResults.Controllers;

[ApiController]
[Route("api/[controller]/{parkrun}")]
public class ParkrunController : ControllerBase
{
    private readonly IParkrunProvider parkrunProvider;

    public ParkrunController(IParkrunProvider parkrunProvider)
    {
        this.parkrunProvider = parkrunProvider;
    }

    [HttpGet]
    public async Task<IEnumerable<Race>> GetRacesAsync(string parkrun)
    {
        return await this.parkrunProvider.GetRacesAsync(parkrun);
    }

    [HttpGet("{eventId}")]
    public async Task<Race> GetRaceResultsAsync(string parkrun, string eventId)
    {
        return await this.parkrunProvider.GetRaceResultsAsync(parkrun, eventId);
    }
}