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

        public float gamma = 0.5f;

        private bool isStable = false;
        public bool isPolicy = false;
        public bool isClickable = true;

        public Transform agent;

        public State start;

        public GameObject debugFloor;
        public GameObject debugText;

        private List<GameObject> floors;
        private List<GameObject> texts;

        // Start is called before the first frame update
        void Start()
        {
            floors = new List<GameObject>();
            texts = new List<GameObject>();

            if (isPolicy)
            {
                PolicyInitialization();
            }
            
            foreach (var s in state)
            {
                var go = Instantiate(debugFloor, s.transform.position, Quaternion.Euler(0, 0, 0));
                go.GetComponent<MeshRenderer>().material.color = new Color(s.Vs, 0, 0);
                floors.Add(go);
            }
        }

        IEnumerator Move()
        {
            var current = start;
            while (current.policy != null)
            {
                yield return new WaitForSeconds(1);
                //Debug.Log(current);
                agent.Translate(current.policy.nextState.transform.position - current.transform.position);
                current = current.policy.nextState;
            }
        }

        public void ButtonClick() {
            if (!isClickable)
            {
                return;
            }
            if (isPolicy)
            {
                if(!isStable) {
                    PolicyEvaluation(gamma);
                    isStable = PolicyImprovement(gamma);
                    Debug();
                }
            }
            else
            {
                ValueIteration(gamma);
                Debug();
                isStable = true;
            }
            
            
            if(isStable) {
                StartCoroutine(Move());
                isClickable = false;
            }
        }


        void PolicyInitialization()
        {
            for (int i = 0; i < state.Length; i++)
            {
                //state[i].Vs = 0;
                if (state[i].Actions.Length > 0)
                {
                    state[i].policy = state[i].Actions[Random.Range(0, state[i].Actions.Length)];
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
                if (s.policy == null)
                {
                    continue;
                }
                Action temp = s.policy;

                Action action = s._a[0];
                float res = action.reward + gamma * action.nextState.Vs;
                for (int i = 1; i < s._a.Count; i++)
                {
                    if (s._a[i].reward + gamma * s._a[i].nextState.Vs > res)
                    {
                        action = s._a[i];
                        res = s._a[i].reward + gamma * s._a[i].nextState.Vs;
                    }
                }
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
                    if (s._a.Count == 0)
                    {
                        continue;
                    }
                    float temp = s.Vs;
                    s.Vs = s._a.Max(x => x.reward + gamma * x.nextState.Vs);
                    delta = Mathf.Max(delta, Mathf.Abs(temp - s.Vs));

                }
            } while (delta > 0.01);
            
            foreach (var s in state)
            {
                if (s._a.Count == 0)
                {
                    continue;
                }
                s.policy = s._a.OrderByDescending(x => x.nextState.Vs).First();
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
    }
}


