using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.UserTask;

namespace dotnet_rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character,GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<UserTask,GetUserTaskDto>();
            CreateMap<AddUserTaskDto, UserTask>();
        }
    }
}