public class MissionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<MonsterDto> Monsters { get; set; }
    public string Description { get; set; } = "";
    public int LevelRequirement { get; set; }
}