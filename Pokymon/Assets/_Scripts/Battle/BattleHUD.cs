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

    private Pokemon _pokemon;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        pokemonName.text = pokemon.Base.Name;
        pokemonLevel.text = $"Lv {pokemon.Level}";
        healthBar.SetHP((float)_pokemon.HP/_pokemon.MaxHp);
        UpdatePokemonData(pokemon.HP);
    }

    public IEnumerator UpdatePokemonData(int oldHPValeu)
    {
        yield return healthBar.SetSmoothHp((float)_pokemon.HP/_pokemon.MaxHp);
        yield return DecreaseHealthPoints(oldHPValeu);
    }

    IEnumerator DecreaseHealthPoints(int oldHpValeu)
    {
        while (oldHpValeu > _pokemon.HP)
        {
            oldHpValeu--;
            pokemonHealth.text = $"{oldHpValeu}/{_pokemon.MaxHp}";
            yield return new WaitForSeconds(0.1f);
        }
        pokemonHealth.text = $"{_pokemon.HP}/{_pokemon.MaxHp}";
    }
    
}
