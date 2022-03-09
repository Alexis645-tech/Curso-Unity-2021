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
            },
            {
                StatusConditionID.brn,
                new StatusCondition()
                {
                    Name = "Burn",
                    Description = "Hace que el Pokemon sufra daño en cada turno",
                    StartMessage = "ha sido quemado",
                    OnFinishTurn = BurnEffect
                }
            },
            {
                StatusConditionID.par,
                new StatusCondition()
                {
                    Name = "Paralized",
                    Description = "Hace que el Pokemon pueda estar paralizado en el turno",
                    StartMessage = "ha sido paralizado",
                    OnStartTurn = ParalizedEffect
                }
            },
            {
                StatusConditionID.frz,
                new StatusCondition()
                {
                    Name = "Frozen",
                    Description = "Hace que el Pokemon este congelado, pero se puede curar aleatoriamente durante un turno",
                    StartMessage = "ha sido congelado",
                    OnStartTurn = FrozenEffect
                }
            }
        };

    static void PoisonEffect(Pokemon pokemon)
    {
        pokemon.UpdateHp(Mathf.CeilToInt((float)pokemon.MaxHp/8.0f));
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos del veneno");
    }
    static void BurnEffect(Pokemon pokemon)
    {
        pokemon.UpdateHp(Mathf.CeilToInt((float)pokemon.MaxHp/15.0f));
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sufre los efectos de la quemadura");
    }

    static bool ParalizedEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 25)
        {
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} esta paralizado y no puede moverse");
            return false;
        }
        return true;
    }
    static bool FrozenEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 25)
        {
            pokemon.CureStatusCondition();
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no esta congelado");
            return true;
        }
        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue congelado");
        return false;
    }
}

public enum StatusConditionID
{
    none, brn, frz, par, psn, slp
}
