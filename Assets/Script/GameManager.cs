using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    private List<State> finalState;
    private State startState;

    public int startPlayer;
    public List<int> startBox;

    public GameObject[] grid;

    public List<State> deadlyCase;

    // Start is called before the first frame update
    void Awake()
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

        startBox = new List<int>();

        for (int i = 0; i < grid.Length; i++)
        {
            if(grid[i].CompareTag("Wall"))
                map[i] = 1;

            if (grid[i].CompareTag("Box"))
            {
                map[i] = 2;
                startBox.Add(i);
            }
            
            if(grid[i].CompareTag("Final"))
                map[i] = 3;
            
            if(grid[i].CompareTag("Empty"))
                map[i] = 0;

            if (grid[i].CompareTag("Deadly"))
                map[i] = -1;

            if (grid[i].CompareTag("Start"))
            {
                map[i] = 0;
                startPlayer = i;
            }
               
            
            //Player State
            playerState[i] = i;

            //Box State
            for (int k = 0; k < nbBox; k++)
            {
                boxState[k][i] = i;
            }
        }
        
        
        state = new State[(int)size.y * (int)size.x * (int)Math.Pow((int)size.y * (int)size.x,nbBox) ];
        finalState = new List<State>();
        deadlyCase = new List<State>();

        
        //Etat
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                if(map[i * (int)size.x + j] == 1)
                    //print($"{j} - {i} - {i * (int)size.x + j} - {map[i * (int)size.x + j]}");
                    Instantiate(prefabs[1],
                    new Vector3(i - 3.5f, 0.15f, j + 0.5f),
                    Quaternion.Euler(0, 0, 0));

                List<Vector2Int> boxPosition = new List<Vector2Int>();
                if (nbBox > 0)
                {
                    for (int yBox = 0; yBox < size.y; yBox++)
                    {
                        for (int xBox = 0; xBox < size.x; xBox++)
                        {
                            boxPosition.Add(new Vector2Int(xBox, yBox));
                            CreateState(new Vector2Int(j, i), boxPosition);
                            boxPosition.Clear();
                        }
                    }
                }
                else
                {
                    CreateState(new Vector2Int(j, i), boxPosition);
                }
            }
        }

        //Actions
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                List<Vector2Int> boxPosition = new List<Vector2Int>();
                if (nbBox > 0)
                {
                    for (int yBox = 0; yBox < size.y; yBox++)
                    {
                        for (int xBox = 0; xBox < size.x; xBox++)
                        {
                            boxPosition.Add(new Vector2Int(xBox, yBox));
                            InitializeActions(new Vector2Int(j, i), boxPosition);
                            boxPosition.Clear();
                        }
                    }
                }
                else
                {
                    InitializeActions(new Vector2Int(j, i), boxPosition);
                }
            }
        }
        
    }


    private void CreateState(Vector2Int playerPosition, List<Vector2Int> boxPosition)
    {
        int x = playerPosition.x;
        int y = playerPosition.y;

        int offset = (y * (int) size.x + x) * (int)Math.Pow((int)(size.x * size.y), nbBox);
        for (int i = 0; i < nbBox; i++)
        {
            offset += ((boxPosition[i].y * (int) size.x + boxPosition[i].x) * (int)Math.Pow((int)(size.x * size.y), nbBox - (i +1)));
        }
        
        //print($"{x} - {y} ==> {offset} / {boxPosition[0].x} - {boxPosition[0].y}");

        if (boxPosition.Contains(playerPosition))
        {
            state[offset] = null;
            return;
        }
        
        var Vs = 0;
        if (boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 3)) Vs = 2;
        if (nbBox == 0 && map[playerState[y * (int)size.x + x]] == 3) Vs = 2;
        if (boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == -1)) Vs = -1;
        
        string stringBox = "";
        if (nbBox > 0)
        {
            stringBox = String.Concat(stringBox, " - ", boxState[0][boxPosition[0].y * (int) size.x + boxPosition[0].x]);
            for (int i = 1; i < nbBox; i++)
            {
                stringBox = String.Concat(stringBox, " - ", boxState[i][boxPosition[i].y * (int) size.x + boxPosition[i].x]);
            }
        }
        
        string name = String.Concat(playerState[y * (int) size.x + x].ToString(), stringBox);
        //print(Vs);
        State s = new State(Vs, null, name);
        
        state[offset] = s;
                        
        //Final Case
        if (boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 3))
        {
            finalState.Add(s);
        }
        
        if (nbBox == 0 && map[playerState[y * (int)size.x + x]] == 3)
        {
            finalState.Add(s);
        }
                        
        //Deadly Case
        if (map[y * (int) size.x + x] == -1 || boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == -1))
        {
            deadlyCase.Add(s);
        }
    }

    private void InitializeActions(Vector2Int playerPosition, List<Vector2Int> boxPosition)
    {
        int x = playerPosition.x;
        int y = playerPosition.y;
        
        //print(boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 3) || boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 1) || boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == -1));

        if (nbBox == 0 && map[playerState[y * (int)size.x + x]] == 3)
        {
            return;
        }
        
        if (boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 3) || boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == 1) || boxPosition.Any(x => map[boxState[0][x.y * (int)size.x + x.x]] == -1))
        {
            return;
        }

        if (map[y * (int) size.x + x] == 1 || map[y * (int) size.x + x] == -1)
        {
            return;
        }
        
        int offset = (y * (int) size.x + x) * (int)Math.Pow((int)(size.x * size.y), nbBox);
        for (int i = 0; i < nbBox; i++)
        {
            offset += ((boxPosition[i].y * (int) size.x + boxPosition[i].x) * (int)Math.Pow((int)(size.x * size.y), nbBox - (i +1)));
        }

        if (state[offset] == null)
        {
            return;
        }
        
        state[offset].InitializeActions(map, state, playerState, boxState, x, y, boxPosition);
        //print($"{state[offset].name} - {state[offset].actions.Count}");
    }
    
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
    
    public State[] GetFinalStates()
    {
        return finalState.ToArray();
    }
}
