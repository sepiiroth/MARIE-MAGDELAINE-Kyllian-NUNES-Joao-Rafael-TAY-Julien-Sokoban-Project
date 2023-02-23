using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _singeleton;

    public static GameManager Instance()
    {
        return _singeleton;
    }
    
    public GameObject[] prefabs;

    public int nbBox = 0;

    public Vector2 size = new Vector2(4,4);
    
    private State[] state;
    private int[] map;
    private int[] playerState;
    private List<int[]> boxState;

    public GameObject[] grid;

    public List<State> deadlyCase;

    // Start is called before the first frame update
    void Start()
    {
        _singeleton = this;
        map = new int[(int)size.y * (int)size.x];
        for (int k = 0; k < nbBox; k++)
        {
            if (k == 0) boxState = new List<int[]>();
                
            var tempTabInt = new int[(int)size.y * (int)size.x];
            boxState.Add(tempTabInt);
        }
        playerState = new int[(int)size.y * (int)size.x];

        for (int i = 0; i < grid.Length; i++)
        {
            if(grid[i].CompareTag("Wall"))
                map[i] = 1;
            
            if(grid[i].CompareTag("Box"))
                map[i] = 2;
            
            if(grid[i].CompareTag("Final"))
                map[i] = 3;
            
            if(grid[i].CompareTag("Empty"))
                map[i] = 0;

            if (grid[i].CompareTag("Deadly"))
                map[i] = -1;
            
            //Player State
            playerState[i] = i;

            //Box State
            for (int k = 0; k < nbBox; k++)
            {
                boxState[k][i] = i;
            }
        }
        
        
        state = new State[(int)size.y * (int)size.x * (int)size.y * (int)size.x];
        deadlyCase = new List<State>();

        
        //Etat
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                if(map[i * (int)size.x + j] == 1)
                    Instantiate(prefabs[map[i * (int)size.x + j]],
                    new Vector3(j - 3.5f, 0.15f, i + 0.5f),
                    Quaternion.Euler(0, 0, 0));

                for (int yBox = 0; yBox < size.y; yBox++)
                {
                    for (int xBox = 0; xBox < size.x; xBox++)
                    {
                        var Vs = 0;
                        if (map[boxState[0][yBox * (int)size.x + xBox]] == 3) Vs = 1;
                        if (map[boxState[0][yBox * (int)size.x + xBox]] == -1) Vs = -1;
                        string name = String.Concat(playerState[i * (int) size.x + j].ToString(), " - ", boxState[0][yBox * (int)size.x + xBox].ToString());
                        State s = new State(Vs, null, name);
                        if (playerState[i * (int) size.x + j] == boxState[0][yBox * (int)size.x + xBox])
                        {
                            s = null;
                        }
                        state[i * (int) size.x + j + ((yBox * (int)size.x + xBox) * (int)(size.x * size.y))] = s;
                        //Deadly Case
                        if (map[i * (int) size.x + j] == -1)
                        {
                            deadlyCase.Add(s);
                        }
                    }
                }
                
                
                
                
            }
        }

        //Actions
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                for (int yBox = 0; yBox < size.y; yBox++)
                {
                    for (int xBox = 0; xBox < size.x; xBox++)
                    {
                        if (map[yBox * (int) size.x + xBox] != 3 && map[i * (int) size.x + j] != 1 &&
                            map[i * (int) size.x + j] != -1 && map[yBox * (int) size.x + xBox] != -1 && map[yBox * (int) size.x + xBox] != 1
                            && state[i * (int) size.x + j + ((yBox * (int)size.x + xBox) * (int)(size.x * size.y))] != null)
                        {
                            state[i * (int) size.x + j + ((yBox * (int) size.x + xBox) * (int) (size.x * size.y))]
                                .InitializeActions(map, state, playerState, boxState, j, i, xBox, yBox);
                        }
                    }
                }
            }
        }
        
    }

    // Update is called once per frame
    public GameObject[] GetGrid()
    {
        return grid;
    }
    
    public State[] GetStates()
    {
        return state;
    }
    
    public State[] GetDeadlyStates()
    {
        return deadlyCase.ToArray();
    }
}
