using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public Category? Category { get; set; }
        public Duration? Duration { get; set; }
        public bool Status { get; set; }
        public DateTime CreationDate { get; set; }

    }
}