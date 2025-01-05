public class CombatResultDto
{
    public List<string> CombatLog { get; set; } = new List<string>(); // Para log de combate
    public int DamageDealt { get; set; } // Danos causados
    public List<Monster> Monsters { get; set; } = new List<Monster>(); // Lista de monstros no combate
}
