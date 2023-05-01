using System.Text.Json;
using System.Text.Json.Serialization;
using JorgeSerrano.Json;
using WprResults.Models;

namespace WprResults.ResultsProviders.ChampionChipIreland;

public class ChampionChipIrelandProvider : IChampionChipIrelandProvider
{
    private static readonly Uri Uri = new("https://api.championchipireland.com/v1/chip_events");

    private static DateTime resultsExpiry = DateTime.UtcNow;
    private static RaceModel[] results = null!;

    public async Task<IEnumerable<Race>> GetRacesAsync()
    {
        RaceModel[] races = await GetRaceModelsAsync();

        return races.Select(x => x.ToRace());
    }

    public async Task<Race> GetRaceResultsAsync(string name, string id)
    {
        RaceModel[] races = await GetRaceModelsAsync();

        return races
               .First(x => x.Name == name && x.Id == id)
               .ToRaceResults();
    }

    private static async Task<RaceModel[]> GetRaceModelsAsync()
    {
        if (resultsExpiry < DateTime.UtcNow)
        {
            Stream str = await new HttpClient().GetStreamAsync(Uri);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy() };

            ChipEventModel[]? chipEvents = await JsonSerializer.DeserializeAsync<ChipEventModel[]>(str, options);

            results = chipEvents!
                      .Where(evt => !evt.Name.EndsWith("Rankings"))
                      .SelectMany(evt => evt.Races.Select(race => new RaceModel
                                                                  {
                                                                      Id = race.Name,
                                                                      Name = evt.Name,
                                                                      Date = race.Date,
                                                                      CsvHeaders = race.CsvHeaders,
                                                                      CsvData = race.CsvData
                                                                  }))
                      .ToArray();

            resultsExpiry = DateTime.UtcNow.AddMinutes(5);
        }

        return results;
    }

    private class ChipEventModel
    {
        public string Name { get; set; } = null!;

        public IEnumerable<RaceModel> Races { get; set; } = null!;
    }

    private class RaceModel
    {
        [JsonIgnore]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Date { get; set; } = null!;

        public IList<string> CsvHeaders { get; set; } = null!;

        public string[][] CsvData { get; set; } = null!;

        public Race ToRace()
        {
            return new Race
                   {
                       Name = this.Name,
                       Id = this.Id
                   };
        }

        public Race ToRaceResults()
        {
            return new Race
                   {
                       Name = this.Name,
                       Results = this.ToResults()
                   };
        }

        private Result[] ToResults()
        {
            int firstNameIndex = GetIndex("Firstname");
            int lastNameIndex = GetIndex("Lastname");
            int bibIndex = GetIndex("Bib");
            int clubIndex = GetIndex("Club");
            int positionIndex = GetIndex("Pos");
            int finishTimeIndex = GetIndex("Finish");
            int chipTimeIndex = GetIndex("Chiptime");

            return this.CsvData
                       .Select(row => new Result
                                      {
                                          Bib = row[bibIndex],
                                          Club = row[clubIndex],
                                          Position = int.Parse(row[positionIndex]),
                                          ChipTime = TimeOnly.Parse(row[chipTimeIndex]),
                                          FinishTime = TimeOnly.Parse(row[finishTimeIndex]),
                                          FirstName = row[firstNameIndex],
                                          LastName = row[lastNameIndex]
                                      })
                       .ToArray();

            int GetIndex(string propName)
            {
                return this.CsvHeaders.IndexOf(propName);
            }
        }
    }
}