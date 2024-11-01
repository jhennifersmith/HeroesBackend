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
                .Select( c => _mapper.Map<GetUserTaskDto>(c))
                .ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetUserTaskDto>>> DeleteUserTask(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();

            try{
                var task = _context.UserTasks
                .FirstOrDefault(c => c.Id == id && c.User!.Id == GetUserId());
                if (task is null){
                    throw new Exception ($"Task with Id '{id}' not found.");
                }
                _context.UserTasks.Remove(task);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _context.UserTasks
                .Where(c => c.User!.Id == GetUserId())
                .Select(c => _mapper.Map<GetUserTaskDto>(c)).ToList();
            }
            catch (Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        // public async Task<ServiceResponse<List<GetUserTaskDto>>> GetAllUserTasks()
        // {
        //     var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();
        //     var dbTasks = await _context.UserTasks
        //     .Where(c => c.User!.Id == GetUserId()).ToListAsync();
        //     serviceResponse.Data = dbTasks.Select(c => _mapper.Map<GetUserTaskDto>(c)).ToList();
        //     return serviceResponse;
        // }

        public async Task<ServiceResponse<List<GetUserTaskDto>>> GetAllUserTasks()
        {
            var serviceResponse = new ServiceResponse<List<GetUserTaskDto>>();

            // Obt�m todas as tarefas do usu�rio atual
            var dbTasks = await _context.UserTasks
                .Where(c => c.User!.Id == GetUserId())
                .ToListAsync();

            // Verifica se cada tarefa precisa ser resetada com base na dura��o
            foreach (var task in dbTasks)
            {
                if (NeedsReset(task))
                {
                    task.Status = false; // Marca como incompleta
                    task.LastCompletedDate = null; // Reseta a data de conclus�o
                }
            }

            // Salva as altera��es se houver alguma tarefa que foi resetada
            await _context.SaveChangesAsync();

            // Mapeia as tarefas para o DTO
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
            try{
                var task = await _context.UserTasks
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == updatedUserTask.Id);
                if (task is null || task.User!.Id != GetUserId()){
                    throw new Exception ($"Task with Id '{updatedUserTask.Id}' not found.");
                }

                task.Category = updatedUserTask.Category;
                task.CreationDate = updatedUserTask.CreationDate;
                task.Duration = updatedUserTask.Duration;
                task.Status = updatedUserTask.Status; 
                task.Title = updatedUserTask.Title;
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetUserTaskDto>(task);
            }
            catch (Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetUserTaskDto>> CompleteUserTask(int taskId)
        {
            var serviceResponse = new ServiceResponse<GetUserTaskDto>();
            try
            {
                var task = await _context.UserTasks
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.User!.Id == GetUserId());

                if (task == null)
                {
                    throw new Exception($"Task with Id '{taskId}' not found.");
                }

                // Verifique se a tarefa est� marcada como conclu�da
                if (!task.Status)
                {
                    throw new Exception($"Task '{task.Title}' has not been completed yet.");
                }

                // Atualizar o personagem do usu�rio
                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.User!.Id == GetUserId());

                if (character == null)
                {
                    throw new Exception("Character not found.");
                }

                // Aplicar pontua��o com base na categoria e frequ�ncia da tarefa
                switch (task.Category)
                {
                    case Category.Study:
                        character.Intelligence += CalculatePoints(task.Duration);
                        break;
                    case Category.Workout:
                        character.Strenght += CalculatePoints(task.Duration);
                        break;
                    case Category.Habit:
                        character.Defense += CalculatePoints(task.Duration);
                        break;
                }

                // Salvar as altera��es
                await _context.SaveChangesAsync();

                // Retornar a tarefa atualizada
                serviceResponse.Data = _mapper.Map<GetUserTaskDto>(task);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        // M�todo para calcular os pontos com base na frequ�ncia
        private int CalculatePoints(Duration? frequency)
        {
            return frequency switch
            {
                Duration.Day => 10,  // Exemplo de pontua��o para tarefa di�ria
                Duration.Week => 25, // Exemplo de pontua��o para tarefa semanal
                Duration.Month => 50,  // Exemplo de pontua��o para tarefa mensal
                _ => 0
            };
        }

        public bool NeedsReset(UserTask task)
        {
            if (task.LastCompletedDate == null)
            {
                return false; // Se nunca foi completada, n�o h� necessidade de resetar.
            }

            var now = DateTime.Now;

            switch (task.Duration)
            {
                case Duration.Day:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 1;
                case Duration.Week:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 7;
                case Duration.Month:
                    return (now - task.LastCompletedDate.Value).TotalDays >= 30;
                default:
                    return false;
            }
        }

    }
}