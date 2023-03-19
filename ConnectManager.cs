using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Actor Connection Manager
// Attach to GameManager
namespace Unity.FPS.AI
{
    public class ConnectManager : MonoBehaviour
    {
        // Start is called before the first frame update
        Dictionary<int, List<GameObject>> Agents;
        void Start()
        {
            Agents = new Dictionary<int, List<GameObject>>();
            Debug.Log("Init");
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var key in Agents.Keys){
                int list_total = Agents[key].Count;
                List<int> selected_items = new List<int>();
                for (int i = 0; i < list_total; i++){
                    if (((list_total - i) <= (2 - selected_items.Count)) ||
                        (Random.Range(0f,1f) < (list_total-1)/CombinationProbability(list_total, 2))){
                        selected_items.Add(i);
                    }
                    if (selected_items.Count >= 2){
                        break;
                    }
                }
                var dist = (Agents[key][selected_items[0]].transform.position - 
                            Agents[key][selected_items[1]].transform.position).magnitude;
                Debug.Log(string.Format("Distance : {0} ({1} - {2})", 
                                        dist, 
                                        Agents[key][selected_items[0]].name, 
                                        Agents[key][selected_items[1]].name));
            }
        }

        int Factorial(int val){
            int res = val;
            if (val <= 0){
                return 1;
            }
            for (int i = 1; i < val; i++){
                res *= (val-i);
            }
            return res;
        }

        float CombinationProbability(int numerator, int denominator){
            return Factorial(numerator)/Factorial(denominator);
        }

        public void AddAgent(GameObject obj, int affiliation)
        {
            if (Agents.ContainsKey(affiliation)){
                // Add a agent in existing team
                Agents[affiliation].Add(obj);
            }else{
                // Add a agent in unknown team
                Agents.Add(affiliation, new List<GameObject>(){obj});
            }
        }

        public void RemoveAgent(GameObject obj, int affiliation)
        {
            if (Agents[affiliation].Contains(obj)){
                Agents[affiliation].Remove(obj);
                Debug.Log(string.Format("Remove GameObject from {0}", affiliation));
            }
        }
    }
}
