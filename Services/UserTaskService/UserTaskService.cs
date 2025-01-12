using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.UserTask;

namespace dotnet_rpg.Services.UserTaskService
{
    public class UserTaskService : IUserTaskService
    {

        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserTaskService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<List<GetUserTaskDto>>> AddUserTask(AddUserTaskDto newUserTask)
        {
            var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();
            var task = _mapper.Map<UserTask>(newUserTask);
            task.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync();
            serviceResponse.Data =
                await _context.UserTasks
                .Where(c => c.User!.Id == GetUserId())
                .Select(c => _mapper.Map<GetUserTaskDto>(c))
                .ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetUserTaskDto>>> DeleteUserTask(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();

            try
            {
                var task = _context.UserTasks
                .FirstOrDefault(c => c.Id == id && c.User!.Id == GetUserId());
                if (task is null)
                {
                    throw new Exception($"Task with Id '{id}' not found.");
                }
                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _context.UserTasks
                .Where(c => c.User!.Id == GetUserId())
                .Select(c => _mapper.Map<GetUserTaskDto>(c)).ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetUserTaskDto>>> GetAllUserTasks()
        {
            var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();

            var dbTasks = await _context.UserTasks
                .Where(c => c.User!.Id == GetUserId())
                .ToListAsync();

            foreach (var task in dbTasks)
            {
                if (NeedsReset(task))
                {
                    task.Status = false;
                    task.LastCompletedDate = null;
                }
            }

            await _context.SaveChangesAsync();

            serviceResponse.Data = dbTasks.Select(c => _mapper.Map<GetUserTaskDto>(c)).ToList();

            return serviceResponse;
        }


        public async Task<ServiceResponse<GetUserTaskDto>> GetUserTaskById(int id)
        {
            var serviceResponse = new ServiceResponse<GetUserTaskDto>();
            var dbTask = await _context.UserTasks.FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetUserTaskDto>(dbTask);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetUserTaskDto>> UpdateUserTask(UpdateUserTaskDto updatedUserTask)
        {
            var serviceResponse = new ServiceResponse<GetUserTaskDto>();
            try
            {
                var task = await _context.UserTasks
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == updatedUserTask.Id);
                if (task is null || task.User!.Id != GetUserId())
                {
                    throw new Exception($"Task with Id '{updatedUserTask.Id}' not found.");
                }

                task.Category = updatedUserTask.Category;
                task.CreationDate = updatedUserTask.CreationDate;
                task.Duration = updatedUserTask.Duration;
                task.Status = updatedUserTask.Status;
                task.Title = updatedUserTask.Title;
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetUserTaskDto>(task);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<CompleteTaskResponseDto>> CompleteUserTask(int taskId)
        {
            var serviceResponse = new ServiceResponse<CompleteTaskResponseDto>();
            try
            {
                var task = await _context.UserTasks
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.User!.Id == GetUserId());

                if (task == null)
                {
                    throw new Exception($"Task with Id '{taskId}' not found.");
                }

                if (task.Status)
                {
                    throw new Exception($"Task '{task.Title}' has already been completed.");
                }

                task.Status = true;
                await _context.SaveChangesAsync();

                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.User!.Id == GetUserId());

                if (character == null)
                {
                    throw new Exception("Character not found.");
                }

                int points = CalculatePoints(task.Duration);
                int expGained = points;
                int levelUpCount = 0;

                var attributesGained = new AttributesDto { Strength = 0, Defense = 0, Intelligence = 0, HitPoints = 0 };

                switch (task.Category)
                {
                    case Category.Study:
                        attributesGained.Intelligence = points;
                        character.Intelligence += points;
                        break;
                    case Category.Workout:
                        attributesGained.Strength = points;
                        attributesGained.HitPoints = points;
                        character.Strenght += points;
                        character.HitPoints += points;
                        break;
                    case Category.Habit:
                        attributesGained.Defense = points;
                        attributesGained.HitPoints = points;
                        character.Defense += points;
                        character.HitPoints += points;
                        break;
                }

                character.Experience += expGained;

                while (character.Experience >= 100)
                {
                    character.Level += 1;
                    character.Experience -= 100;
                    levelUpCount++;
                }

                await _context.SaveChangesAsync();

                serviceResponse.Data = new CompleteTaskResponseDto
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    AttributesGained = attributesGained,
                    ExperienceGained = expGained,
                    LevelUpCount = levelUpCount
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        private int CalculatePoints(Duration? frequency)
        {
            return frequency switch
            {
                Duration.Dia => 10,
                Duration.Semana => 25,
                Duration.Mes => 50,
                _ => 0
            };
        }

        public bool NeedsReset(UserTask task)
        {
            if (task.LastCompletedDate == null)
            {
                return false;
            }

            var now = DateTime.Now;

            switch (task.Duration)
            {
                case Duration.Dia:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 1;
                case Duration.Semana:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 7;
                case Duration.Mes:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 30;
                default:
                    return false;
            }
        }

    }
}