using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField][Tooltip("Prefab de enemigo a generar")]private GameObject prefab;
    [SerializeField][Tooltip("Tiempo de inicio y fin de la oleada")]private float startTime, endTime;
    [SerializeField][Tooltip("Tiempo entre la generación de enemigos")]private float spawnRate;
    
    // Start is called before the first frame update
    void Start()
    {
        WaveManager.SharedInstance.waves.Add(this);
        InvokeRepeating("SpawnEnemy", startTime, spawnRate);
        Invoke("EndWave", endTime);
    }

    void SpawnEnemy()
    {
        Instantiate(prefab, transform.position, transform.rotation);
    }

    void EndWave()
    {
        WaveManager.SharedInstance.waves.Remove(this);
        CancelInvoke();
    }
}
