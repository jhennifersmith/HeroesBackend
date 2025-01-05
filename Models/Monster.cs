using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class Monster
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Frodo";
        public int Health { get; set; } = 100;
        public int Strength { get; set; } = 1;
        public int Intelligence { get; set; } = 1;
        public int Defense { get; set; } = 1;
        public Mission? Mission {get; set;} 
    }
}