public class CompleteTaskResponseDto
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = "";
    public AttributesDto? AttributesGained { get; set; }
    public int ExperienceGained { get; set; }
    public int LevelUpCount { get; set; }
}
