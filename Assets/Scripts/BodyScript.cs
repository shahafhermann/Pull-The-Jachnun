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
    public float hitPenalty;
    private List<GameObject> links;
    public int playerNum;
    public static int initialLinksPerPlayer = 5;
    private GameObject playerParent;

    [Range(0.1f, 10f)] 
    public float moveSpeed = 5f;

    [Range(2f, 30f)]
    public float rotationSpeed = 5f;

    public KeyCode up;
    public KeyCode right;
    public KeyCode left;
    public KeyCode fire;

    void Start()
    {
        links = manager.links;
        hitPenalty = 0f;
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
        if (hitPenalty == 0 && Input.GetKeyDown(fire))
        {
            shoot();
        }
    }

    private void FixedUpdate() {
        if (hitPenalty > 0)
        {
            hitPenalty -= Time.deltaTime;
            return;
        }
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
        
        if (Poly.ContainsPoint(verArr, manager.getCurrentEgg().transform.position))
        {
            // TODO: notify about the food that got eaten (Gameobject food)
            // food.SetActive(false);
            manager.spawnEgg(true);
            manager.addPoint(playerNum);
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

    void shoot()
    {
        GameObject shot = manager.getShot();
        shot.SetActive(true);
        shot.transform.position = transform.position;
        if (playerNum == 1) shot.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 180));
        else shot.transform.rotation = transform.rotation;
        shot.GetComponent<ShotScript>().setPlayer(playerNum);  
    }

    public void takeHit()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        IEnumerator flashCR()
        {
            for (int i = 0; i < 10; i++)
            {
                renderer.enabled = false;
                yield return new WaitForSeconds(0.25f);
                renderer.enabled = true;
                yield return new WaitForSeconds(0.25f);
            }
        }
        StartCoroutine(flashCR());
        if (hitPenalty == 0) hitPenalty = 5;
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
