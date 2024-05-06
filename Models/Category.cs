using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Category
    {
       Study =1,
       Workout = 2,
       Habit = 3,
    }
}