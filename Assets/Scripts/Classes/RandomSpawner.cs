using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public List<GameObject> prefabsToSpawn; // List of prefabs to spawn
    public int spawnCount = 10; // Number of prefabs to spawn

    [Header("Spawn Area Settings")]
    public float yOffset = 0.1f; // Offset to place objects slightly above the surface


    public bool hasSpawned = false;

    private void Update()
    {
        if (!GetComponent<ScaleUp>().isReady)
            return;

        if (hasSpawned)
            return;

        if (prefabsToSpawn.Count <= 0)
            return;
        
        SpawnObjects();
    }

    void SpawnObjects()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        if (meshCollider == null)
        {
            Debug.LogError("No MeshCollider found on the GameObject. Please add a MeshCollider.");
            return;
        }

        Bounds bounds = meshCollider.bounds;

        for (int i = 0; i < spawnCount; i++)
        {
            // Randomly pick a point on the surface
            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + yOffset,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Check if the point is actually on the surface
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(randomPosition.x, bounds.max.y + 10f, randomPosition.z), Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == gameObject) // Ensure it hits the correct object
                {
                    // Adjust position to match the hit point and offset
                    randomPosition = hit.point + new Vector3(0, yOffset, 0);

                    // Randomize rotation
                    Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                    // Randomly select a prefab from the list
                    GameObject prefabToSpawn = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];

                    // Spawn the prefab at the position with random rotation
                    Instantiate(prefabToSpawn, randomPosition, randomRotation);
                }
            }

            hasSpawned = true;
        }
    }
}
