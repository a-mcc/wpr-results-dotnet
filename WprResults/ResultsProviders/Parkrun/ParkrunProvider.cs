using HtmlAgilityPack;
using WprResults.Models;

namespace WprResults.ResultsProviders.Parkrun;

public class ParkrunProvider : IParkrunProvider
{
    private const string ResultsUri = "https://www.parkrun.org.uk/{0}/results/{1}/";
    private const string HistoryUri = "https://www.parkrun.org.uk/{0}/results/eventhistory/";

    private static DateTime racesExpiry = DateTime.UtcNow;
    private static RaceModel[] raceModels = Array.Empty<RaceModel>();

    public async Task<IEnumerable<Race>> GetRacesAsync(string parkrun)
    {
        RaceModel[] races = await GetRaceModelsAsync(parkrun);

        return races
               .Select(x => x.ToRace())
               .ToArray();
    }

    public async Task<Race> GetRaceResultsAsync(string parkrun, string eventId)
    {
        ResultModel[] races = await GetRaceResultModelsAsync(parkrun, eventId);

        return new Race
               {
                   Name = parkrun,
                   Results = races.Select(x => x.ToResult())
               };
    }

    private static async Task<RaceModel[]> GetRaceModelsAsync(string parkrun)
    {
        if (racesExpiry < DateTime.UtcNow)
        {
            var url = string.Format(HistoryUri, parkrun);

            HtmlNodeCollection results = await GetResultsTable(url);

            raceModels = results.Take(10)
                            .Select(x => new RaceModel
                                         {
                                             Date = DateOnly.Parse(x.GetAttributeValue("data-date", string.Empty)),
                                             Id = int.Parse(x.GetAttributeValue("data-parkrun", string.Empty))
                                         })
                            .ToArray();

            racesExpiry = DateTime.UtcNow;
        }

        return raceModels;
    }

    private static async Task<ResultModel[]> GetRaceResultModelsAsync(string parkrun, string eventId)
    {
        var url = string.Format(ResultsUri, parkrun,
                                eventId);

        HtmlNodeCollection results = await GetResultsTable(url);

        return results.Select(row => new ResultModel
                                     {
                                         Club = GetValue(row, "data-club"),
                                         Name = GetValue(row, "data-name"),
                                         Position = int.Parse(GetValue(row, "data-position")),
                                         Time = TimeOnly.Parse(row.SelectNodes("//td[contains(@class, 'time')]/div")[0].InnerText)
                                     })
                      .ToArray();

        string GetValue(HtmlNode node, string attribute)
        {
            return node.GetAttributeValue(attribute, string.Empty);
        }
    }

    private static async Task<HtmlNodeCollection> GetResultsTable(string url)
    {
        string str = await new HttpClient().GetStringAsync(url);

        var html = new HtmlDocument();
        html.LoadHtml(str);

        return html.DocumentNode.SelectNodes("//tr[@class='Results-table-row']");
    }

    private class RaceModel
    {
        public int Id { get; init; }
        public DateOnly Date { get; init; }

        public Race ToRace()
        {
            return new Race
                   {
                       Name = this.Date.ToString(),
                       Id = this.Id.ToString()
                   };
        }
    }

    private class ResultModel
    {
        public int Position { get; init; }
        public string Name { get; init; } = null!;
        public string Club { get; init; } = null!;
        public TimeOnly Time { get; init; }

        public Result ToResult()
        {
            return new Result
                   {
                       Position = this.Position,
                       Club = this.Club,
                       FinishTime = this.Time,
                       FirstName = this.Name.Split(' ', 2).First(),
                       LastName = this.Name.Split(' ', 2).Last()
                   };
        }
    }
}