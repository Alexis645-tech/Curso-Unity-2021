using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField]private Text pokemonName;
    [SerializeField]private Text pokemonLevel;
    [SerializeField]private HealthBar healthBar;
    [SerializeField]private Text pokemonHealth;

    public void SetPokemonData(Pokemon pokemon)
    {
        pokemonName.text = pokemon.Base.Name;
        pokemonLevel.text = $"Lv {pokemon.Level}";
        healthBar.SetHP(pokemon.HP/pokemon.MaxHp);
        pokemonHealth.text = $"{pokemon.HP}/{pokemon.MaxHp}";
    }
}
