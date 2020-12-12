using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public Rigidbody2D headBody;
    public float spawnDistance;
    public GameObject linkPrefab;
    public JachManager manager;
    private List<GameObject> links;
    // public static GameObject midLink;
    // public static GameObject lastLink1;
    // public static GameObject lastLink2;
    public int playerNum;
    public static int initialLinksPerPlayer = 4;
    private GameObject playerParent;

    [Range(0.1f, 7f)] 
    public float moveSpeed = 5f;

    [Range(2f, 30f)]
    public float rotationSpeed = 5f;

    public KeyCode up;
    public KeyCode right;
    public KeyCode left;
    
    void Start()
    {
        links = manager.links;
        playerParent = transform.parent.gameObject;
        for (int i = 0; i < initialLinksPerPlayer; i++) // Needed! 
        {
            addLink();
        }
    }

    void addLink()
    {
        GameObject lastLink = (playerNum == 1) ? lastLink = links[0] : links[links.Count - 1];
        Vector2 push = (transform.position - lastLink.transform.position).normalized * spawnDistance;
        Vector2 spawnPos = new Vector2(lastLink.transform.position.x, lastLink.transform.position.y) + push;
        headBody.transform.position = (spawnPos + 1.8f * push);
        GameObject newLink = Instantiate(linkPrefab, spawnPos, Quaternion.Slerp(transform.rotation, lastLink.transform.rotation, 0.5f));
        newLink.transform.parent = playerParent.transform;
        GetComponent<HingeJoint2D>().connectedBody = newLink.GetComponent<Rigidbody2D>();
        newLink.GetComponent<HingeJoint2D>().connectedBody = lastLink.GetComponent<Rigidbody2D>();
        newLink.transform.parent = playerParent.transform;
        if (playerNum == 1) links.Insert(0, newLink);
        else links.Add(newLink);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNum == 1 && Input.GetKeyDown(KeyCode.Space))  // TODO: detect circle closure instead
        {
            addLink();
        }
        if (playerNum == 2 && Input.GetKeyDown(KeyCode.LeftAlt))  // TODO: detect circle closure instead
        {
            addLink();
        }
    }

    private void FixedUpdate() {
        if (Input.GetKey(left))
        {
            headBody.MoveRotation(transform.eulerAngles.z + rotationSpeed);
        }

        if (Input.GetKey(right))
        {
            headBody.MoveRotation(transform.eulerAngles.z - rotationSpeed);
        }

        if (Input.GetKey(up)) {
            int dir = (playerNum == 1) ? -1 : 1;
            Vector2 direction = dir * transform.right;
            headBody.MovePosition(headBody.position + moveSpeed * Time.deltaTime * direction);
        }
    }

    void checkFoodPickup(GameObject link)
    {
        List<Vector2> vertices = new List<Vector2>();
        if (playerNum == 1)
        {
            int i = 1;
            vertices.Add(transform.position);
            while (links[i - 1] != link)
            {
                vertices.Add(links[i - 1].transform.position);
                i++;
            }
        }
        else
        {
            for (int i = links.IndexOf(link); i < links.Count; i++)
            {
                vertices.Add(links[i].transform.position);
            }
            vertices.Add(transform.position);
        }
        Vector2[] verArr = vertices.ToArray();
        // foreach (GameObject food in manager.foods)
        // {
        //     if (Poly.ContainsPoint(verArr, food.transform.position))
        //     {
        //         // TODO: notify about the food that got eaten (Gameobject food)
        //         // food.SetActive(false);
        //         manager.spawnEgg(true);
        //         addLink();
        //     }
        // }
        // manager.foods.Add(manager.getCurrentEgg());
        
        if (Poly.ContainsPoint(verArr, manager.getCurrentEgg().transform.position))
        {
            // TODO: notify about the food that got eaten (Gameobject food)
            // food.SetActive(false);
            manager.spawnEgg(true);
            addLink();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "link")
        {
            checkFoodPickup(collision.gameObject);
        }
    }

    public static class Poly
    {
        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }
    }
}
