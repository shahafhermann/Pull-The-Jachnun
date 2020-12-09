using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JachManager : MonoBehaviour
{
    public List<GameObject> foods;

    // Start is called before the first frame update
    void Start()
    {
        foods = new List<GameObject>();
        foods.Add(GameObject.Find("Egg"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
