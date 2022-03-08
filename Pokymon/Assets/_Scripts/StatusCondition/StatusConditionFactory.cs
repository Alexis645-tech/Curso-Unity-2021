using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusConditionFactory
{
    public static Dictionary<StatusConditionID, StatusCondition> StatusConditions { get; set; } =
        new Dictionary<StatusConditionID, StatusCondition>()
        {
            {
                StatusConditionID.psn,
                new StatusCondition()
                {
                    Name = "Poison",
                    Description = "Hace que el Pokemon sufra daño en cada turno",
                    StartMessage = "ha sido envenenado",
                    OnFinishTurn = PoisonEffect
                }
            }
        };

    static void PoisonEffect(Pokemon pokemon)
    {
        pokemon.UpdateHp(pokemon.MaxHp/8);
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos del veneno");
    }
}

public enum StatusConditionID
{
    none, brn, frz, par, psn, slp
}
