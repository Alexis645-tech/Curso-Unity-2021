using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHUD : MonoBehaviour
{
    [SerializeField] private Text messageText;
    private PartyMemberHUD[] memberHuds;

    private List<Pokemon> pokemons;

    public void InitPartyHUD()
    {
        memberHuds = GetComponentsInChildren<PartyMemberHUD>();
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        messageText.text = "Selecciona un Pokemon";
        for (int i = 0; i < memberHuds.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberHuds[i].gameObject.SetActive(true);
                memberHuds[i].SetPokemonData(pokemons[i]);
            }
            else
            {
                memberHuds[i].gameObject.SetActive(false);
            }
        }
    }

    public void UdpdateSelectedPokemon(int selectedPokemon)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberHuds[i].SetSelectedPokemon(i == selectedPokemon);
        }
    }
}
