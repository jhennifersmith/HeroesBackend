using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class UserMission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int MissionId { get; set; }
        public Mission? Mission { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public int CurrentMonsterHealth { get; set; } 
        public int CurrentCharacterHealth { get; set; } 
        public bool MisssionFailed { get; set; }
    }
}