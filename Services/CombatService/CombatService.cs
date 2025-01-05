
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CombatService
{
    public class CombatService : ICombatService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CombatService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<CombatResultDto>> StartCombat(int missionId)
        {
            var serviceResponse = new ServiceResponse<CombatResultDto>();
            var combatResult = new CombatResultDto { CombatLog = new List<string>() };

            var character = await _context.Characters
                .Include(c => c.User) // Certifica-se de carregar o User
                .FirstOrDefaultAsync(c => c.Id == GetUserId());
            if (character == null || character.User == null)
            {
                return new ServiceResponse<CombatResultDto>
                {
                    Success = false,
                    Message = "Character not found or user is invalid."
                };
            }

            var mission = await _context.Missions
                .Include(m => m.Monsters)
                .FirstOrDefaultAsync(m => m.Id == missionId);
            if (mission == null)
            {
                return new ServiceResponse<CombatResultDto>
                {
                    Success = false,
                    Message = "Mission not found."
                };
            }

            if (mission.Monsters == null || !mission.Monsters.Any())
            {
                return new ServiceResponse<CombatResultDto>
                {
                    Success = false,
                    Message = "No monsters associated with this mission."
                };
            }

            combatResult.CombatLog.Add($"Starting combat for mission: {mission.Name}");

            var monster = mission.Monsters.First();
            combatResult.Monsters = new List<Monster> { monster };

            var userMission = await _context.UserMissions
                .FirstOrDefaultAsync(um => um.MissionId == missionId && um.UserId == character.User.Id);

            if (userMission == null)
            {
                userMission = new UserMission
                {
                    UserId = character.User.Id,
                    MissionId = missionId,
                    Progress = 0,
                    IsCompleted = false,
                    CurrentMonsterHealth = monster.Health
                };

                await _context.UserMissions.AddAsync(userMission);
                await _context.SaveChangesAsync();

                combatResult.CombatLog.Add($"UserMission created for user {character.User.Id} and mission {missionId}.");
            }
            else
            {
                combatResult.CombatLog.Add($"UserMission already exists for user {character.User.Id} and mission {missionId}.");
            }

            serviceResponse.Data = combatResult;
            serviceResponse.Success = true;
            return serviceResponse;
        }

        public async Task<ServiceResponse<CombatResultDto>> PerformCharacterAttack(int characterId, int missionId)
        {
            var serviceResponse = new ServiceResponse<CombatResultDto>();
            var combatResult = new CombatResultDto { CombatLog = new List<string>() };

            var character = await _context.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null || character.User == null)
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "Character or User not found." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var mission = await _context.Missions
                .Include(m => m.Monsters)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null || mission.Monsters == null || !mission.Monsters.Any())
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "Mission or Monsters not found." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var monster = mission.Monsters.FirstOrDefault(m => m.Mission?.Id == mission.Id);

            if (monster == null)
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "No valid monster found for the mission." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var userMission = await _context.UserMissions
                .FirstOrDefaultAsync(um => um.MissionId == missionId && um.UserId == character.User.Id);

            if (userMission == null)
            {
                userMission = new UserMission
                {
                    UserId = character.User.Id,
                    MissionId = missionId,
                    Progress = 0,
                    IsCompleted = false,
                    CurrentCharacterHealth = character.HitPoints,
                    CurrentMonsterHealth = monster.Health
                };

                await _context.UserMissions.AddAsync(userMission);
                await _context.SaveChangesAsync();
            }

            int damage = CalculateDamage(character.Strenght, monster.Defense);
            userMission.CurrentMonsterHealth -= damage;
            await _context.SaveChangesAsync();

            combatResult.CombatLog.Add($"{character.Name} atacou {monster.Name} e concedeu {damage} de dano!");

            if (userMission.CurrentMonsterHealth <= 0)
            {
                combatResult.CombatLog.Add($"{character.Name} derrotou {monster.Name}!");

                userMission.IsCompleted = true;
                userMission.Progress = 100;

                combatResult.CombatLog.Add($"{mission.Name} completada!");
            }
            else
            {
                int monsterDamage = CalculateDamage(monster.Strength, character.Defense);
                userMission.CurrentCharacterHealth -= monsterDamage;
                combatResult.CombatLog.Add($"{monster.Name} atacou {character.Name} e concedeu {monsterDamage} de dano!");
                await _context.SaveChangesAsync();
                if (userMission.CurrentCharacterHealth <= 0)
                {
                    combatResult.CombatLog.Add($"{monster.Name} te derrotou {character.Name}!");
                    combatResult.CombatLog.Add($"{mission.Name} não completada, tente novamente quando estiver mais forte!");
                    userMission.MisssionFailed = true;
                    userMission.CurrentCharacterHealth = character.HitPoints;
                    userMission.CurrentMonsterHealth = monster.Health;
                }
            }

            await _context.SaveChangesAsync();

            serviceResponse.Data = combatResult;
            serviceResponse.Success = true;
            return serviceResponse;
        }

        public async Task<ServiceResponse<CombatResultDto>> PerformCharacterMagicAttack(int characterId, int missionId)
        {
            var serviceResponse = new ServiceResponse<CombatResultDto>();
            var combatResult = new CombatResultDto { CombatLog = new List<string>() };

            var character = await _context.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null || character.User == null)
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "Character or User not found." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var mission = await _context.Missions
                .Include(m => m.Monsters)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null || mission.Monsters == null || !mission.Monsters.Any())
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "Mission or Monsters not found." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var monster = mission.Monsters.FirstOrDefault(m => m.Mission?.Id == mission.Id);

            if (monster == null)
            {
                serviceResponse.Data = new CombatResultDto { CombatLog = new List<string> { "No valid monster found for the mission." } };
                serviceResponse.Success = false;
                return serviceResponse;
            }

            var userMission = await _context.UserMissions
                .FirstOrDefaultAsync(um => um.MissionId == missionId && um.UserId == character.User.Id);

            if (userMission == null)
            {
                userMission = new UserMission
                {
                    UserId = character.User.Id,
                    MissionId = missionId,
                    Progress = 0,
                    IsCompleted = false,
                    CurrentCharacterHealth = character.HitPoints,
                    CurrentMonsterHealth = monster.Health
                };

                await _context.UserMissions.AddAsync(userMission);
                await _context.SaveChangesAsync();
            }

            int damage = CalculateDamage(character.Intelligence, monster.Defense);
            userMission.CurrentMonsterHealth -= damage;
            await _context.SaveChangesAsync();

            combatResult.CombatLog.Add($"{character.Name} atacou {monster.Name} e concedeu {damage} de dano mágico!");

            if (userMission.CurrentMonsterHealth <= 0)
            {
                combatResult.CombatLog.Add($"{character.Name} derrotou {monster.Name}!");

                userMission.IsCompleted = true;
                userMission.Progress = 100;

                combatResult.CombatLog.Add($"{mission.Name} completada!");
            }
            else
            {
                int monsterDamage = CalculateDamage(monster.Strength, character.Defense);
                userMission.CurrentCharacterHealth -= monsterDamage;
                combatResult.CombatLog.Add($"{monster.Name} atacou {character.Name} e concedeu {monsterDamage} de dano!");
                await _context.SaveChangesAsync();
                if (userMission.CurrentCharacterHealth <= 0)
                {
                    combatResult.CombatLog.Add($"{monster.Name}te derrotou {character.Name}!");
                    combatResult.CombatLog.Add($"{mission.Name} não completada, tente novamente quando estiver mais forte!");
                    userMission.MisssionFailed = true;
                    userMission.CurrentCharacterHealth = character.HitPoints;
                    userMission.CurrentMonsterHealth = monster.Health;
                }
            }

            await _context.SaveChangesAsync();

            serviceResponse.Data = combatResult;
            serviceResponse.Success = true;
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<Mission>>> GetMissions()
        {
            var serviceResponse = new ServiceResponse<List<Mission>>();

            var missions = await _context.Missions.ToListAsync();

            if (missions.Count == 0)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "No missions found.";
                return serviceResponse;
            }

            serviceResponse.Data = missions;
            serviceResponse.Success = true;
            return serviceResponse;
        }

        public int CalculateDamage(int attack, int defense, int critChance = 10, int critMultiplier = 2)
        {

            int adjustedDefense = (int)(defense * 0.2);

            int baseDamage = attack - adjustedDefense;

            baseDamage = Math.Max(baseDamage, 1);

            Random rand = new Random();
            bool isCriticalHit = rand.Next(0, 100) < critChance;

            if (isCriticalHit)
            {
                baseDamage = (int)(baseDamage * critMultiplier);
            }

            double blockChance = Math.Min(defense / (double)(attack + defense), 0.3);
            bool isBlocked = rand.NextDouble() < blockChance;

            if (isBlocked)
            {
                baseDamage = (int)(baseDamage * 0.5);
            }

            return baseDamage;
        }


        public async Task<ServiceResponse<MissionDto>> GetMission(int missionId)
        {
            var serviceResponse = new ServiceResponse<MissionDto>();

            var mission = await _context.Missions
                .Include(m => m.Monsters)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Mission not found.";
                return serviceResponse;
            }

            var missionDto = new MissionDto
            {
                Id = mission.Id,
                Name = mission.Name,
                LevelRequirement = mission.LevelRequirement,
                Monsters = mission.Monsters.Select(monster => new MonsterDto
                {
                    Id = monster.Id,
                    Name = monster.Name,
                    Health = monster.Health
                }).ToList()
            };

            serviceResponse.Data = missionDto;
            serviceResponse.Success = true;
            return serviceResponse;
        }


        public async Task<ServiceResponse<Monster>> GetMonster(int monsterId)
        {
            var serviceResponse = new ServiceResponse<Monster>();

            var monster = await _context.Monsters
                .FirstOrDefaultAsync(m => m.Id == monsterId);

            if (monster == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Monster not found.";
                return serviceResponse;
            }

            serviceResponse.Data = monster;
            return serviceResponse;
        }

        public void UpdateCharacterAfterCombat(Character character, int experienceGained, List<string> combatLog)
        {
            character.Experience += experienceGained;

            if (character.Experience >= character.Level * 100)
            {
                LevelUp(character, combatLog);
            }
        }

        public void LevelUp(Character character, List<string> combatLog)
        {
            character.Level++;
            character.Strenght += 2;
            character.Defense += 2;
            character.Intelligence += 2;
            character.HitPoints += 10;

            combatLog.Add($"Character leveled up! Now at Level {character.Level}.");
        }

        public bool IsCombatFinished(List<Monster> monsters)
        {
            return monsters.All(m => m.Health <= 0);
        }

        public async Task<ServiceResponse<string>> GrantMissionRewards(int characterId, int missionId)
        {
            var response = new ServiceResponse<string>();

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
            if (character == null)
            {
                response.Success = false;
                response.Message = "Character not found.";
                return response;
            }

            var mission = await _context.Missions.FirstOrDefaultAsync(m => m.Id == missionId);
            if (mission == null)
            {
                response.Success = false;
                response.Message = "Mission not found.";
                return response;
            }

            if (character.Level < mission.LevelRequirement)
            {
                response.Success = false;
                response.Message = "Character does not meet the level requirement for this mission.";
                return response;
            }

            var userMission = await _context.UserMissions.FirstOrDefaultAsync(um => um.UserId == character.User.Id && um.MissionId == missionId);
            if (userMission != null)
            {
                userMission.IsCompleted = true;
                userMission.Progress = 100;
            }

            await _context.SaveChangesAsync();
            response.Data = $"{character.Name} successfully completed {mission.Name} and gained {mission.RewardExperience} XP!";

            return response;
        }

        public async Task<List<Monster>> GetMonstersByMissionId(int missionId)
        {
            return await _context.Monsters
                .Where(m => m.Mission!.Id == missionId)
                .ToListAsync();
        }
    }
}
