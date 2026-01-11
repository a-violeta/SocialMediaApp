using System.Text.Json.Serialization;

public class OpenAiModerationResponse
{
    [JsonPropertyName("results")]
    public ModerationResult[] Results { get; set; }
}

public class ModerationResult
{
    [JsonPropertyName("flagged")]
    public bool Flagged { get; set; }

    [JsonPropertyName("categories")]
    public ModerationCategories Categories { get; set; }

    [JsonPropertyName("category_scores")]
    public ModerationCategoryScores CategoryScores { get; set; }
}

public class ModerationCategories
{
    [JsonPropertyName("sexual")]
    public bool Sexual { get; set; }

    [JsonPropertyName("hate")]
    public bool Hate { get; set; }

    [JsonPropertyName("harassment/threat")]
    public bool Harassment { get; set; }

    [JsonPropertyName("self-harm")]
    public bool SelfHarm { get; set; }

    [JsonPropertyName("violence")]
    public bool Violence { get; set; }

    [JsonPropertyName("sexual/minors")]
    public bool SexualMinors { get; set; }
}

public class ModerationCategoryScores
{
    [JsonPropertyName("sexual")]
    public float Sexual { get; set; }

    [JsonPropertyName("hate")]
    public float Hate { get; set; }

    [JsonPropertyName("harassment/threat")]
    public float Harassment { get; set; }

    [JsonPropertyName("self-harm")]
    public float SelfHarm { get; set; }

    [JsonPropertyName("violence")]
    public float Violence { get; set; }

    [JsonPropertyName("sexual/minors")]
    public float SexualMinors { get; set; }
}
