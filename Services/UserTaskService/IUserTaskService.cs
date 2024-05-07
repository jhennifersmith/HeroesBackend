using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.UserTask;

namespace dotnet_rpg.Services.UserTaskService
{
    public interface IUserTaskService
    {
        Task<ServiceResponse<List<GetUserTaskDto>>> GetAllUserTasks();
        Task<ServiceResponse<GetUserTaskDto>> GetUserTaskById(int id);
        Task<ServiceResponse<List<GetUserTaskDto>>> AddUserTask(AddUserTaskDto newUserTask);
        Task<ServiceResponse<GetUserTaskDto>> UpdateUserTask(UpdateUserTaskDto updatedUserTask);
        Task<ServiceResponse<List<GetUserTaskDto>>> DeleteUserTask(int id);
    }
}