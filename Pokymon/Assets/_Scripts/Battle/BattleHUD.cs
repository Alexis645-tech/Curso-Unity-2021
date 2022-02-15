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
        //Si con el update se ve mal, actualizar la vida al iniciar la batalla
        UpdatePokemonData(pokemon.HP);
    }

    public void UpdatePokemonData(int oldHPValeu)
    {
        StartCoroutine(healthBar.SetSmoothHp((float)_pokemon.HP/_pokemon.MaxHp));
        StartCoroutine(DecreaseHealthPoints(oldHPValeu));
    }

    IEnumerator DecreaseHealthPoints(int oldHpValeu)
    {
        while (oldHpValeu > _pokemon.HP)
        {
            oldHpValeu--;
            pokemonHealth.text = $"{_pokemon.HP}/{_pokemon.MaxHp}";
            yield return new WaitForSeconds(0.1f);
        }
        pokemonHealth.text = $"{_pokemon.HP}/{_pokemon.MaxHp}";
    }
    
}
