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
    public State finalState;
    public State startState;
    public Transform agent;
    
    
    public GameObject debugFloor;
    public GameObject debugText;

    private List<GameObject> floors;
    private List<GameObject> texts;
    // Start is called before the first frame update
    void Start()
    {
        floors = new List<GameObject>();
        texts = new List<GameObject>();
        MCTS_EV_OffPolicy(20);
        //MCTS_EV_OnPolicy(50);
        Debug();
        StartCoroutine(Move());
    }


    bool EpsilonGreedy(float pInit, float pFinal, int nbEpisodes, int currentEpisode)
    {
        float r = Mathf.Max((nbEpisodes - currentEpisode) / nbEpisodes, 0);
        float epsilon = (pInit - pFinal) * r + pFinal;
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
            agent.Translate(current.policy.nextState.transform.position - current.transform.position);
            current = current.policy.nextState;
        }
    }
    
    void Debug()
    {
        floors.ForEach(x => Destroy(x));
        floors.Clear();
        texts.ForEach(x => Destroy(x));
        texts.Clear();
            
        foreach (var s in state)
        {
            var go = Instantiate(debugFloor, s.transform.position, Quaternion.Euler(0, 0, 0));
            go.GetComponent<MeshRenderer>().material.color = new Color(s.Vs, 0, 0);
            floors.Add(go);
            go = Instantiate(debugText, s.transform.position, Quaternion.Euler(90, 0, 90));
            go.GetComponent<TextMeshPro>().text = String.Format("{0:0.##}", s.Vs);
            texts.Add(go);
        }
    }

    void MCTS_EV_OffPolicy(int nbEpisode)
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

        foreach (var s in state)
        {
            if (s.policy == null)
            {
                continue;
            }
            for (int i = 0; i < nbEpisode; i++)
            {
                List<State> listS = new List<State>();
                List<Action> listA = new List<Action>();
                List<int> listR = new List<int>();
                int T = 0;
                State current = s;
                while (T < 20)
                {
                    if (current == finalState)
                    {
                        break;
                    }
                    
                    listS.Add(current);
                    listA.Add(current.policy);
                    listR.Add(current.policy.reward);
                    
                    if (EpsilonGreedy(0.5f, 0f, nbEpisode, i))
                    {
                        current = current._a[Random.Range(0, current._a.Count - 1)].nextState;
                    }else
                    {
                        current = current.policy.nextState;
                    }
                    T++;
                }
            
                float G = 0;
                for (int t = T - 1; t >= 0; t--)
                {
                    G += listR[t];
                    listS[t].Return += G;
                    listS[t].N += 1;
                }

            }
            
            foreach (var x in state)
            {
                if (x.policy == null)
                {
                    continue;
                }
                if (x.N == 0 | x.Return == 0)
                {
                    x.Vs = 0;
                    continue;
                }
                x.Vs = x.Return / x.N;
            }
            
            foreach (var x in state)
            {
                if (x._a.Count == 0)
                {
                    continue;
                }
                x.policy = x._a.OrderByDescending(y => y.nextState.Vs).First();
            }

        }
        
        
    }

    void MCTS_EV_OnPolicy(int nbEpisode)
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

        foreach (var s in state)
        {
            if (s.policy == null)
            {
                continue;
            }
            for (int i = 0; i < nbEpisode; i++)
            {
                List<State> listS = new List<State>();
                List<Action> listA = new List<Action>();
                List<int> listR = new List<int>();
                int T = 0;
                State current = s;
                while (T < 20)
                {
                    if (current == finalState)
                    {
                        break;
                    }
                    listS.Add(current);
                    listA.Add(current.policy);
                    listR.Add(current.policy.reward);
                    current = current.policy.nextState;
                    T++;
                }
            
                float G = 0;
                for (int t = T - 1; t >= 0; t--)
                {
                    G += listR[t];
                    listS[t].Return += G;
                    listS[t].N += 1;
                }
                
                foreach (var x in state)
                {
                    if (x.policy == null)
                    {
                        continue;
                    }
                    if (x.N == 0 | x.Return == 0)
                    {
                        x.Vs = 0;
                        continue;
                    }
                    x.Vs = x.Return / x.N;
                }
            
                foreach (var x in state)
                {
                    if (x._a.Count == 0)
                    {
                        continue;
                    }
                    x.policy = x._a.OrderByDescending(y => y.nextState.Vs).First();
                }

            }

        }
    }
}
