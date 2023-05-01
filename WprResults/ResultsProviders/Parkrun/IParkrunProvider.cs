using WprResults.Models;

namespace WprResults.ResultsProviders.Parkrun;

public interface IParkrunProvider
{
    Task<IEnumerable<Race>> GetRacesAsync(string parkrun);
    Task<Race> GetRaceResultsAsync(string parkrun, string eventId);
}