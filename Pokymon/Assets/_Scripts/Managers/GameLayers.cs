using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    public static GameLayers SharedInstance;
    
    [SerializeField] private LayerMask solidObjectsLayer, pokemonLayer, interactableLayer, playerLayer, fovLayer;
    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask PokemonLayer => pokemonLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FOVLayer => fovLayer;

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
    }

    public LayerMask CollisionLayers => (SolidObjectsLayer | InteractableLayer | PlayerLayer);
}
