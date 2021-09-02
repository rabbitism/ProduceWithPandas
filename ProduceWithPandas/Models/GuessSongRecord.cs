namespace ProduceWithPandas.Models;

public class GuessSongRecord
{
    public int Id {  get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Answer {  get; set; } = string.Empty;
    public double Score {  get; set; }

}
