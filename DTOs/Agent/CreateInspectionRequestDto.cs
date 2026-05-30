namespace Authentication.DTOs.Agent;

public class CreateInspectionRequestDto
{
    public DateTime ScheduledAtUtc { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}