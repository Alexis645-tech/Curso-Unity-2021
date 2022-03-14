using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    public static GameLayers SharedInstance;
    
    [SerializeField] private LayerMask solidObjectsLayer, pokemonLayer, interactableLayer;
    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask PokemonLayer => pokemonLayer;
    public LayerMask InteractableLayer => interactableLayer;

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
    }
}
