using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameStateMCTS : MonoBehaviour
{
    public State[] state;
    public State[] finalState;
    public State startState;
    public State[] deadlyState;
    public Transform agent;
    public Transform box;

    public int nbEpisode;
    [Range(0.0f, 1.0f)]
    public float epsilon;

    public bool FV;
    public bool Policy;
    public bool ES;
    
    public GameObject debugFloor;
    public GameObject debugText;

    private List<GameObject> floors;
    private List<GameObject> texts;
    private Vector2 size;
    // Start is called before the first frame update
    void Start()
    {
        deadlyState = GameManager.Instance().GetDeadlyStates();
        state = GameManager.Instance().GetStates();
        floors = new List<GameObject>();
        texts = new List<GameObject>();

        size = GameManager.Instance().size;

        startState = state[GameManager.Instance().startPlayer + ((int) size.x * (int) size.y) * GameManager.Instance().startBox[0]];
        print(startState.name);
        finalState = GameManager.Instance().GetFinalStates();
        
        //print(state[7].Vs);
        MCTS(nbEpisode, FV, Policy, ES);
        Debug();
        //print(state[7].Vs);
        StartCoroutine(Move());
    }


    bool EpsilonGreedyDecay(float pInit, float pFinal, int nbEpisodes, int currentEpisode)
    {
        float r = Mathf.Max((nbEpisodes - currentEpisode) / nbEpisodes, 0);
        float epsilon = (pInit - pFinal) * r + pFinal;
        float p = Random.Range(0.0f, 1.0f);

        return p < epsilon;
    }
    
    bool EpsilonGreedy()
    {
        float p = Random.Range(0.0f, 1.0f);

        return p < epsilon;
    }
    
    IEnumerator Move()
    {
        var current = startState;
        while (current.policy != null)
        {
            yield return new WaitForSeconds(1);
            //Debug.Log(current);
            var dir = Vector3.zero;
            var dirBox = Vector3.zero;
            switch (current.policy.dir)
            {
                case Direction.Up:
                    dir = new Vector3(-1, 0, 0);
                    break;
                case Direction.Down:
                    dir = new Vector3(1, 0, 0);
                    break;
                case Direction.Right:
                    dir = new Vector3(0, 0, 1);
                    break;
                case Direction.Left:
                    dir = new Vector3(0, 0, -1);
                    break;
                case Direction.PushUp:
                    dir = new Vector3(-1, 0, 0);
                    dirBox = new Vector3(-1, 0, 0);
                    break;
                case Direction.PushDown:
                    dir = new Vector3(1, 0, 0);
                    dirBox = new Vector3(1, 0, 0);
                    break;
                case Direction.PushRight:
                    dir = new Vector3(0, 0, 1);
                    dirBox = new Vector3(0, 0, 1);
                    break;
                case Direction.PushLeft:
                    dir = new Vector3(0, 0, -1);
                    dirBox = new Vector3(0, 0, -1);
                    break;
            }
            agent.position += dir;
            box.position += dirBox;
            current = current.policy.nextState;
        }
    }
    
    void Debug()
    {
        floors.ForEach(x => Destroy(x));
        floors.Clear();
        texts.ForEach(x => Destroy(x));
        texts.Clear();

        var grid = GameManager.Instance().GetGrid();
        
        for (int i = 0; i < grid.Length; i++)
        {
            var go = Instantiate(debugFloor, grid[i].transform.position, Quaternion.Euler(0, 0, 0));
            if (grid[i].CompareTag("Final"))
            {
                go.GetComponent<MeshRenderer>().material.color = Color.green;
            }

            foreach (var s in state)
            {
                if (s == null)
                {
                    continue;
                }
                
                print($"{s.name} - {s.Return} - {s.N} = {s.Vs}");
            }

            /*if (state[i] != null)
            {
                if (state[i].policy == null)
                {
                    go.GetComponent<MeshRenderer>().material.color = new Color(state[i].Vs, 0, 0);
                    floors.Add(go);
                    continue;
                }
                go.GetComponent<MeshRenderer>().material.color = new Color(0,state[i].policy.Qs, 0);
                floors.Add(go);
                go = Instantiate(debugText, grid[i].transform.position, Quaternion.Euler(90, 0, 90));
                go.GetComponent<TextMeshPro>().text = String.Format("{0:0.###}", state[i].policy.Qs);
                texts.Add(go);
            }*/
            
        }
    }

    void UpdatePolicy()
    {
        foreach (var x in state)
        {
            if (x == null) continue;
            if (x.policy == null)
            {
                continue;
            }
            if (x.N == 0 | x.Return == 0)
            {
                x.Vs = 0;
                continue;
            }
            x.Vs = x.Return / (float)x.N;
        }
        
        foreach (var x in state)
        {
            if (x == null) continue;
            if (x.actions.Count == 0)
            {
                continue;
            }
            x.policy = x.actions.OrderByDescending(y => y.nextState.Vs).First();
        }
    }

    int GenerateEpisode(int currentEpisode, State start, List<State> listS, List<Action> listA, List<int> listR)
    {
        int T = 0;
        State current = start;
        while (T < 10000)
        {
            if (finalState.Contains(current))
            {
                break;
            }
                
            if (deadlyState.Contains(current))
            {
                //print(listR.Last());
                break;
            }
                
            listS.Add(current);
            Action nextAction;
            if (EpsilonGreedy())
            {
                var otherAction = current.actions.Where(x => x != current.policy).ToList();
                if (otherAction.Count == 0)
                {
                    otherAction = current.actions;
                }

                int index = Random.Range(0, otherAction.Count);
                nextAction = otherAction[index];
                current = nextAction.nextState;
            }else
            {
                nextAction = current.policy;
                current = nextAction.nextState;
            }
                
            listA.Add(nextAction);
            listR.Add(nextAction.reward);

            T++;
        }
        
        return T;
    }

    void MCTS(int nbEpisode, bool MCTS_FV, bool Policy, bool ES)
    {
        foreach (var s in state)
        {
            if (s == null) continue;
            s.N = 0;
            s.Return = 0;
            if (s.actions.Count > 0)
            {
                s.policy = s.actions[Random.Range(0, s.actions.Count)];
            }
        }
        
        var stateWithES = state.Where(x => x != null && x.actions.Count > 0).ToList();
        for (int i = 0; i < nbEpisode; i++)
        {
            List<State> listS = new List<State>();
            List<Action> listA = new List<Action>();
            List<int> listR = new List<int>();
            State start = ES ? stateWithES[Random.Range(0, stateWithES.Count)] : startState;
            int T = GenerateEpisode(i, start, listS, listA, listR);

            float G = 0;
            for (int t = T - 1; t >= 0; t--)
            {
                if (MCTS_FV)
                {
                    if (listS.Take(t - 1).Contains(listS[t]))
                    {
                        continue;
                    }
                }
                G += listR[t];
                listS[t].Return += G;
                listS[t].N += 1;
            }

            if (Policy)
            {
                UpdatePolicy();
            }

        }

        if (!Policy)
        {
            UpdatePolicy();
        }
        

    }

    /*void MCTS_EV_OnPolicy(int nbEpisode)
    {
        foreach (var s in state)
        {
            s.N = 0;
            s.Return = 0;
            if (s.Actions.Length > 0)
            {
                s.policy = s.Actions[Random.Range(0, s.Actions.Length)];
            }
        }

        for (int i = 0; i < nbEpisode; i++)
        {
            List<State> listS = new List<State>();
            List<Action> listA = new List<Action>();
            List<int> listR = new List<int>();
            int T = 0;
            State current = startState;
            while (T < 10000)
            {
                if (current == finalState)
                {
                    break;
                }
                
                if (deadlyState.Contains(current))
                {
                    //print(listR.Last());
                    break;
                }
                
                listS.Add(current);
                Action nextAction;
                if (EpsilonGreedy())
                {
                    var otherAction = current.actions.Where(x => x != current.policy).ToList();
                    if (otherAction.Count == 0)
                    {
                        otherAction = current.actions;
                    }

                    nextAction = otherAction[Random.Range(0, otherAction.Count - 1)];
                    current = nextAction.nextState;
                }else
                {
                    nextAction = current.policy;
                    current = nextAction.nextState;
                }
                
                listA.Add(nextAction);
                listR.Add(nextAction.reward);

                T++;
            }

            float G = 0;
            for (int t = T - 1; t >= 0; t--)
            {
                G += listR[t];
                listS[t].Return += G;
                listS[t].N += 1;
            }
            
            UpdatePolicy();

        }
    }*/
}
