using WprResults.Models;

namespace WprResults.ResultsProviders.ChampionChipIreland;

public interface IChampionChipIrelandProvider
{
    Task<IEnumerable<Race>> GetRacesAsync();

    Task<Race> GetRaceResultsAsync(string name, string id);
}