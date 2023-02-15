using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public GameObject[] prefabs;

    private int[] map;

    public GameObject sol;
    
    // Start is called before the first frame update
    void Start()
    {
        map = new[]
        {
            0, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 0
        };

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (map[i * 4 + j] != 0)
                    Instantiate(prefabs[map[i * 4 + j]],
                    new Vector3(i - 3.5f, 0.5f, j + 0.5f),
                    Quaternion.Euler(0, 0, 0));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
