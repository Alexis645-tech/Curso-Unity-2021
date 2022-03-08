using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField]private Text pokemonName;
    [SerializeField]private Text pokemonLevel;
    [SerializeField]private HealthBar healthBar;
    [SerializeField]private Text pokemonHealth;
    [SerializeField]private GameObject expBar;

    private Pokemon _pokemon;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        pokemonName.text = pokemon.Base.Name;
        SetLevelText();
        healthBar.SetHP((float)_pokemon.HP/_pokemon.MaxHp);
        SetExp();
        StartCoroutine(UpdatePokemonData(pokemon.HP));
    }

    public IEnumerator UpdatePokemonData(int oldHPValeu)
    {
        if (_pokemon.HasHpChanged)
        {
            StartCoroutine(healthBar.SetSmoothHp((float) _pokemon.HP / _pokemon.MaxHp));
            StartCoroutine(DecreaseHealthPoints(oldHPValeu));
            yield return null;
            _pokemon.HasHpChanged = false;
        }
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

    public void SetExp()
    {
        if (expBar == null)
        {
            return;
        }

        expBar.transform.localScale = new Vector3(NormalizedExp(), 1, 1);
    }

    public IEnumerator SetExpSmooth(bool needsToResetBar = false)
    {
        if (expBar == null)
        {
            yield break;
        }

        if (needsToResetBar)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        yield return expBar.transform.DOScaleX(NormalizedExp(), 2f).WaitForCompletion();
    }

    float NormalizedExp()
    {
        float currentLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level);
        float nextLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level + 1);
        float normalizedExp = (_pokemon.Experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void SetLevelText()
    {
        pokemonLevel.text = $"Lv {_pokemon.Level}";
    }
    
}
