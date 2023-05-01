using Microsoft.AspNetCore.Mvc;
using WprResults.Models;
using WprResults.ResultsProviders.ChampionChipIreland;

namespace WprResults.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChampionChipIrelandController : ControllerBase
{
    private readonly IChampionChipIrelandProvider championChipIrelandProvider;

    public ChampionChipIrelandController(IChampionChipIrelandProvider championChipIrelandProvider)
    {
        this.championChipIrelandProvider = championChipIrelandProvider;
    }

    [HttpGet]
    public async Task<IEnumerable<Race>> GetRacesAsync()
    {
        return await this.championChipIrelandProvider.GetRacesAsync();
    }

    [HttpGet("{raceName}/{raceId}")]
    public async Task<Race> GetRaceResultsAsync(string raceName, string raceId)
    {
        return await this.championChipIrelandProvider.GetRaceResultsAsync(raceName,
                                                                          raceId);
    }
}