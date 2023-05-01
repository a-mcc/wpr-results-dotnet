namespace WprResults.Models;

public class Result
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Club { get; set; } = null!;
    public string Bib { get; set; } = null!;
    public int Position { get; set; }
    public TimeOnly FinishTime { get; set; }
    public TimeOnly ChipTime { get; set; }
}