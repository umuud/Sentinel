namespace Sentinel.API.Options;

public class SerilogOptions
{
    public const string SectionName = "Serilog";

    public string ElasticsearchUrl { get; set; } = string.Empty;
    public int NumberOfReplicas { get; set; } = 0;
    public int NumberOfShards { get; set; } = 1;
}