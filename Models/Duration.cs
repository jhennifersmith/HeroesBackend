using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Duration
    {
       Mes =1,
       Semana = 2,
       Dia = 3,
    }
}