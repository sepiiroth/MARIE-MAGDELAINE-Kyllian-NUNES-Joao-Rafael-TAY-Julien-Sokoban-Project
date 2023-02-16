using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameStateTDL : MonoBehaviour
{
    public State[] state;
    public State finalState;
    public State startState;
    public Transform agent;
    
    
    public GameObject debugFloor;
    public GameObject debugText;

    private List<GameObject> floors;
    private List<GameObject> texts;

    public int nbEp;
    public float alpha;
    public float gamma;
    // Start is called before the first frame update
    void Start()
    {
        floors = new List<GameObject>();
        texts = new List<GameObject>();
        
        SARSA(nbEp, alpha,gamma);
        //QLearning(nbEp, alpha,gamma);
        
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

    void SARSA(int nbEpisodes, float alpha, float gamma)
    {
        //Init
        foreach (var s in state)
        {
            /*foreach (var action in s._a)
            {
                action.Qs = 0;
            }*/
            if (s.Actions.Length > 0)
            {
                s.policy = s.Actions[Random.Range(0, s.Actions.Length)];
            }
        }
        
        //
        for (int i = 0; i < nbEpisodes; i++)
        {
            State current;
            current = startState;
            Action a;
            if (EpsilonGreedy(0.5f, 0f, nbEpisodes, i))
            {
                a = current._a[Random.Range(0, current._a.Count - 1)];
            }
            else
            {
                print(current);
                a = current.policy;
            }

            var T = 0;
            while (T < 20)
            {
                if (current == finalState)
                {
                    break;
                }
                float r = a.reward;
                State sPrime = a.nextState;
                Action aPrime;
                if (EpsilonGreedy(0.7f, 0f, nbEpisodes, i))
                {
                    aPrime = sPrime._a.Where(x=> x != sPrime.policy).ToList()[Random.Range(0,  sPrime._a.Where(x=> x != sPrime.policy).ToList().Count - 1)];
                }
                else
                {
                    aPrime = sPrime.policy;
                }
                a.Qs += alpha * (r + gamma * (aPrime.Qs) - a.Qs);
                current = sPrime;
                a = aPrime;
                T += 1;
            }
        }
    }
    
    void QLearning(int nbEpisodes, float alpha, float gamma)
    {
        //Init
        foreach (var s in state)
        {
            foreach (var action in s._a)
            {
                action.Qs = 0;
            }
            if (s.Actions.Length > 0)
            {
                s.policy = s.Actions[Random.Range(0, s.Actions.Length)];
            }
        }
        
        //
        for (int i = 0; i < nbEpisodes; i++)
        {
            State current;
            current = startState;
            Action a;
            if (EpsilonGreedy(0.5f, 0f, nbEpisodes, i))
            {
                a = current._a[Random.Range(0, current._a.Count - 1)];
            }
            else
            {
                print(current);
                a = current.policy;
            }

            do
            {
                float r = a.reward;
                State sPrime = a.nextState;
                Action aPrime;
                if (EpsilonGreedy(0.5f, 0f, nbEpisodes, i))
                {
                    aPrime = sPrime._a[Random.Range(0, sPrime._a.Count - 1)];
                }
                else
                {
                    aPrime = sPrime.policy;
                }

                float target = sPrime._a.OrderByDescending(x => x.Qs).First().Qs;
                a.Qs += alpha * (r + gamma * target - a.Qs);
                current = sPrime;
                a = aPrime;
            } while (current != finalState);
        }
    }
}
