using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace dotnet_rpg.Services.CombatService
{
    public interface ICombatService
    {
        Task<ServiceResponse<CombatResultDto>> StartCombat(int missionId);
        Task<ServiceResponse<CombatResultDto>> PerformCharacterAttack(int characterId, int missionId);
        Task<ServiceResponse<CombatResultDto>> PerformCharacterMagicAttack(int characterId, int missionId);
        Task<ServiceResponse<List<Mission>>> GetMissions();
        int CalculateDamage(int attack, int defense, int critChance = 10, int critMultiplier = 2);
        Task<ServiceResponse<MissionDto>> GetMission(int missionId);
        Task<ServiceResponse<Monster>> GetMonster(int monsterId);
        void UpdateCharacterAfterCombat(Character character, int experienceGained, List<string> combatLog);
        void LevelUp(Character character, List<string> combatLog);
        bool IsCombatFinished(List<Monster> monsters);
        Task<ServiceResponse<string>> GrantMissionRewards(int characterId, int missionId);
        Task<List<Monster>> GetMonstersByMissionId(int missionId);
    }
}