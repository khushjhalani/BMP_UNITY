using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public GameObject gunMagazinePrefab; // Drag the submachine gun magazine prefab here.
    public float magazineLifeTime = 10.0f; // Time in seconds that the charger will remain in the scene.

    private bool isSpawning = false;

    public void SpawnMagazine(Vector3 spawnPosition)
    {
        // Instantiates the submachine gun magazine object at the agent's position.
        GameObject magazine = Instantiate(gunMagazinePrefab, spawnPosition+ new Vector3(1.5f,0f, 0f), Quaternion.identity);

        // Destroy the loader object after magazineLifeTime seconds.
        Destroy(magazine, magazineLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLife playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                playerLife.CollectMagazine(); // Llama al método de recogida del jugador.

                // Destruye el cargador una vez que se haya recogido.
                Destroy(gameObject);
            }
        }
    }
}
