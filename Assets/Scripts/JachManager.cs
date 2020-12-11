using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JachManager : MonoBehaviour
{
    public List<GameObject> links;
    public List<GameObject> foods;

    // Start is called before the first frame update

    private void Awake()
    {
        foods = new List<GameObject>();
        links = new List<GameObject>();
        foods.Add(GameObject.Find("Egg"));
        links.Add(GameObject.Find("mid_link"));
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
