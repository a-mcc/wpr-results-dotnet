namespace WprResults.Models;

public class Race
{
    public string Name { get; set; } = null!;

    public string Id { get; set; } = null!;

    public IEnumerable<Result> Results { get; set; } = null!;
}