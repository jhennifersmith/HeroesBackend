using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class Mission
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Frodo";
        public int LevelRequirement { get; set; } = 100;
        public string Description { get; set; } = "";
        public int RewardExperience { get; set; } = 1;
        public ICollection<Monster>? Monsters { get; set; }
    }
}