using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.UserTask;
using dotnet_rpg.Models;
using dotnet_rpg.Services.UserTaskService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserTaskController : ControllerBase
    {
        private readonly IUserTaskService _userTaskService;

        public UserTaskController(IUserTaskService userTaskService)
        {
            _userTaskService = userTaskService;
        }
        
        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<GetUserTaskDto>>> GetAll()
        {
            return Ok( await _userTaskService.GetAllUserTasks());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserTaskDto>>> Get(int id)
        {
            return Ok(await _userTaskService.GetUserTaskById(id));
        }
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetUserTaskDto>>>> AddUserTask (AddUserTaskDto newUserTask)
        {
           return Ok(await _userTaskService.AddUserTask(newUserTask));
        }

        [HttpPut]
        public async Task<ActionResult<ServiceResponse<List<GetUserTaskDto>>>> UpdateUserTask (UpdateUserTaskDto updatedUserTask)
        {
            var response = await _userTaskService.UpdateUserTask(updatedUserTask);
            if (response.Data is null){
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserTaskDto>>> DeleteUserTask(int id)
        {
             var response = await _userTaskService.DeleteUserTask(id);
            if (response.Data is null){
                return NotFound(response);
            }
            return Ok(response);
        }

    }
}