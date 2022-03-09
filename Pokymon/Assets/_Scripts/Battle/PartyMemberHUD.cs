using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberHUD : MonoBehaviour
{
    [SerializeField] private Text nameText, lvlText, typeText;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Image pokemonImage;

    private Pokemon _pokemon;

    public void SetPokemonData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        lvlText.text = $"Lv {pokemon.Level}";
        if (pokemon.Base.Type2 == PokemonType.None)
        {
            typeText.text = pokemon.Base.Type1.ToString();
        }
        else
        {
            typeText.text = $"{pokemon.Base.Type1.ToString()} - {pokemon.Base.Type2.ToString()}";
        }
        healthBar.SetHP(pokemon);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
    }

    public void SetSelectedPokemon(bool selected)
    {
        if (selected)
        {
            nameText.color = ColorManager.SharedInstance.selectedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }
}
