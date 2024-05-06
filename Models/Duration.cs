using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Duration
    {
       Month =1,
       Week = 2,
       Day = 3,
    }
}