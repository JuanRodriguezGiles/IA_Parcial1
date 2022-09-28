using Unity.Mathematics;

using UnityEngine;

using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public GameObject boid;
    
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 position = new Vector2(Random.Range(0, 100), Random.Range(0, 100));
            Instantiate(boid, position, quaternion.identity);
        }
    }
}