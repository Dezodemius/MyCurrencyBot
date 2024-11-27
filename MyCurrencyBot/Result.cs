namespace MyCurrencyBot;

public class Result
{
  public bool Success { get; set; }
  public int Timestamp { get; set; }
  public string Base { get; set; }
  public string Date { get; set; }
  public Dictionary<string, double> Rates { get; set; }
}