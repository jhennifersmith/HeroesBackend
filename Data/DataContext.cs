using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnet_rpg.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Character> Characters => Set<Character>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserTask> UserTasks => Set<UserTask>();
        public DbSet<Mission> Missions => Set<Mission>();
        public DbSet<Monster> Monsters => Set<Monster>(); 
        public DbSet<UserMission> UserMissions => Set<UserMission>(); 
    }
}