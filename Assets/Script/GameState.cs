using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ESGI
{
    public class GameState : MonoBehaviour
    {
        public State[] state;

        [Range(0.0f, 1.0f)]
        public float gamma = 0.5f;

        private bool isStable = false;
        private bool isClickable = true;

        public Transform agent;
        public Transform box;

        public State startState;
        public State[] finalState;

        public GameObject debugFloor;
        public GameObject debugText;

        private List<GameObject> floors;
        private List<GameObject> texts;
        
        private Vector2 size;
        
        public enum ITERATION_TYPE // your custom enumeration
        {
            PolicyIteration,
            ValueIteration,
        };
    
        public ITERATION_TYPE algo = ITERATION_TYPE.PolicyIteration; 

        // Start is called before the first frame update
        void Start()
        {
            state = GameManager.Instance().GetStates();
            floors = new List<GameObject>();
            texts = new List<GameObject>();

            size = GameManager.Instance().size;

            int offset = (GameManager.Instance().startPlayer * (int)Math.Pow((int)(size.x * size.y), GameManager.Instance().nbBox));
            for (int i = 0; i < GameManager.Instance().nbBox; i++)
            {
                offset += (GameManager.Instance().startBox[i] * (int)Math.Pow((int)(size.x * size.y),GameManager.Instance().nbBox - (i + 1)));
            }

            startState = state[offset];
            finalState = GameManager.Instance().GetFinalStates();

            if (algo == ITERATION_TYPE.PolicyIteration)
            {
                PolicyInitialization();
            }
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

            if (GameManager.Instance().nbBox == 0)
            {
                if (state[i].policy == null)
                {
                    go.GetComponent<MeshRenderer>().material.color = new Color(state[i].Vs, 0, 0);
                    floors.Add(go);
                    continue;
                }
                go.GetComponent<MeshRenderer>().material.color = new Color(0,state[i].Vs, 0);
                floors.Add(go);
                go = Instantiate(debugText, grid[i].transform.position, Quaternion.Euler(90, 0, 90));
                go.GetComponent<TextMeshPro>().text = String.Format("{0:0.###}", state[i].Vs);
                texts.Add(go);
            }
            
        }

        foreach (var s in state)
        {
            if (s == null)
            {
                continue;
            }
            
            //print($"{s.name} : {s.Vs}");
        }
    }

        public void ButtonClick() {
            if (!isClickable)
            {
                return;
            }
            if (algo == ITERATION_TYPE.PolicyIteration)
            {
                float startTime = System.DateTime.Now.Millisecond;
                print(startTime);
                while(!isStable) {
                    PolicyEvaluation(gamma);
                    isStable = PolicyImprovement(gamma);
                    Debug();
                }
                print(System.DateTime.Now.Millisecond);
                float timeEnd = System.DateTime.Now.Millisecond - startTime;
                print($"Temps d'execution : {timeEnd}");
            }
            else
            {
                float startTime = System.DateTime.Now.Millisecond;
                ValueIteration(gamma);
                Debug();
                isStable = true;
                float timeEnd = System.DateTime.Now.Millisecond - startTime;
                print($"Temps d'execution : {timeEnd}");
            }
            
            
            if(isStable) {
                print("Go");
                StartCoroutine(Move());
                isClickable = false;
            }
        }


        void PolicyInitialization()
        {
            for (int i = 0; i < state.Length; i++)
            {
                if (state[i] == null) continue;
                if (state[i].actions.Count > 0)
                {
                    state[i].policy = state[i].actions[Random.Range(0, state[i].actions.Count)];
                }
                
            }
        }
        
        void PolicyEvaluation(float gamma)
        {
            float delta = 0;

            do
            {
                delta = 0;
                foreach (var s in state)
                {
                    if (s == null) continue;
                    if (s.policy == null)
                    {
                        continue;
                    }
                    float temp = s.Vs;
                    s.Vs = s.policy.reward + gamma * s.policy.nextState.Vs;
                    delta = Mathf.Max(delta, Mathf.Abs(temp - s.Vs));

                }
            } while (delta > 0.01);
        }

        bool PolicyImprovement(float gamma)
        {
            
            bool policyStable = true;
            foreach (var s in state)
            {
                if (s == null) continue;
                if (s.policy == null)
                {
                    continue;
                }
                Action temp = s.policy;

                Action action = s.actions[0];
                float res = action.reward + gamma * action.nextState.Vs;
                for (int i = 1; i < s.actions.Count; i++)
                {
                    if (s.actions[i].reward + gamma * s.actions[i].nextState.Vs > res)
                    {
                        action = s.actions[i];
                        res = s.actions[i].reward + gamma * s.actions[i].nextState.Vs;
                    }
                }
                //print($"{s.name} : {action.nextState.name} - {action.reward}");
                s.policy = action;
                if (temp != s.policy)
                {
                    policyStable = false;
                }
            }

            return policyStable;

        }
        
        void ValueIteration(float gamma)
        {
            float delta = 0;

            do
            {
                delta = 0;
                foreach (var s in state)
                {
                    if (s == null) continue;
                    if (s.actions.Count == 0)
                    {
                        continue;
                    }
                    float temp = s.Vs;
                    s.Vs = s.actions.Max(x => x.reward + gamma * x.nextState.Vs);
                    delta = Mathf.Max(delta, Mathf.Abs(temp - s.Vs));

                }
            } while (delta > 0.01);
            
            foreach (var s in state)
            {
                if (s == null) continue;
                if (s.actions.Count == 0)
                {
                    continue;
                }
                s.policy = s.actions.OrderByDescending(x => x.nextState.Vs).First();
            }
            
        }
    }
}


