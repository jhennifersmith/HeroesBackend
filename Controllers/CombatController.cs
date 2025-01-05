using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_rpg.Models;
using dotnet_rpg.Services.CombatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CombatController : ControllerBase
    {
        private readonly ICombatService _combatService;
        private readonly DataContext _context;
        public CombatController(ICombatService combatService, DataContext context)
        {
            _combatService = combatService;
            _context = context;
        }

        [HttpPost("Start")]
        public async Task<ActionResult<ServiceResponse<CombatResultDto>>> StartCombat(int missionId)
        {
            var response = await _combatService.StartCombat(missionId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("CharacterAttack")]
        public async Task<IActionResult> CharacterAttack([FromBody] CombatAttackRequest request)
        {
            var result = await _combatService.PerformCharacterAttack(request.CharacterId, request.MissionId);
            return Ok(result);
        }

        [HttpPost("CharacterMagicAttack")]
        public async Task<IActionResult> CharacterMagicAttack([FromBody] CombatAttackRequest request)
        {
            var result = await _combatService.PerformCharacterMagicAttack(request.CharacterId, request.MissionId);
            return Ok(result);
        }

        // [HttpPost("MonsterAttack")]
        // public async Task<IActionResult> MonsterAttack([FromBody] CombatAttackRequest request)
        // {
        //     var result = await _combatService.PerformMonsterAttack(request.MonsterId, request.CharacterId);
        //     return Ok(result);
        // }


        [HttpPost("GrantRewards")]
        public async Task<IActionResult> GrantMissionRewards(int characterId, int missionId)
        {
            var response = await _combatService.GrantMissionRewards(characterId, missionId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetMission/{missionId}")]
        public async Task<IActionResult> GetMission(int missionId)
        {
            var response = await _combatService.GetMission(missionId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [HttpGet("GetMissions")]
        public async Task<IActionResult> GetMissions()
        {
            var response = await _combatService.GetMissions();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [HttpGet("GetMonster/{monsterId}")]
        public async Task<IActionResult> GetMonster(int monsterId)
        {
            var response = await _combatService.GetMonster(monsterId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response.Data);
        }

        // [HttpGet("NextMonster/{missionId}")]
        // public async Task<IActionResult> GetNextMonster(int missionId)
        // {
        //     var response = await _combatService.GetNextMonster(missionId);
        //     if (!response.Success)
        //     {
        //         return BadRequest(response);
        //     }
        //     return Ok(response.Data);
        // }

        [HttpGet("ByMission/{missionId}")]
        public async Task<IActionResult> GetMonstersByMissionId(int missionId)
        {
            var monsters = await _combatService.GetMonstersByMissionId(missionId);
            Console.WriteLine("MONSTROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOS",monsters);
            if (monsters == null || monsters.Count == 0)
            {
                return NotFound("No monsters found for this mission.");
            }
            return Ok(monsters);
        }

        [HttpPost("complete/{characterId}/{missionId}")]
        public async Task<ActionResult> CompleteMission(int characterId, int missionId)
        {
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound("Character not found.");
            }

            // Obtém o UserId a partir do personagem
            var userId = character.User!.Id;
            var userMission = await _context.UserMissions
                .FirstOrDefaultAsync(um => um.UserId == userId && um.MissionId == missionId);

            if (userMission == null)
            {
                return NotFound("User mission not found.");
            }

            userMission.IsCompleted = true;
            _context.UserMissions.Update(userMission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mission completed successfully!" });
        }


        [HttpGet("status/{characterId}/{missionId}")]
        public async Task<ActionResult<UserMission>> GetUserMissionStatus(int characterId, int missionId)
        {
            var character = await _context.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound("Character not found.");
            }

            if (character.User == null)
            {
                return NotFound("User not found for this character.");
            }

            var userId = character.User.Id;

            var userMission = await _context.UserMissions
                .FirstOrDefaultAsync(um => um.UserId == userId && um.MissionId == missionId);

            if (userMission == null)
            {
                return Ok(new { isCompleted = false });
            }
            else
            {
                return Ok(new { isCompleted = userMission.IsCompleted, missionFailed = userMission.MisssionFailed });
            }
        }

    }
}
