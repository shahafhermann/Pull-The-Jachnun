using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public Rigidbody2D headBody;
    public float spawnDistance;
    public GameObject linkPrefab;
    public JachManager manager;
    public float hitPenalty;
    public float fullStamina;
    public float stamina;
    public float moveCost;
    public int shootCost;
    public float rechargeRate;
    private List<GameObject> links;
    public int playerNum;
    public static int initialLinksPerPlayer = 8;
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
        fullStamina = 100;
        stamina = fullStamina;
        playerParent = transform.parent.gameObject;
        for (int i = 0; i < initialLinksPerPlayer; i++) 
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
        if (Time.timeScale == 0) return;
        if (hitPenalty == 0 && Input.GetKeyDown(fire))
        {
            if (shootCost <= stamina)
            {
                shoot();
                staminaHandler(-shootCost);
            }
        }        
        staminaHandler(rechargeRate * Time.deltaTime);
        
    }

    void FixedUpdate() {
        if (hitPenalty > 0)
        {
            hitPenalty = (hitPenalty - Time.deltaTime >= 0) ? hitPenalty - Time.deltaTime : 0;
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
            if (moveCost <= stamina)
            {
                staminaHandler(-moveCost);
                int dir = (playerNum == 1) ? -1 : 1;
                Vector2 direction = dir * transform.right;
                headBody.MovePosition(headBody.position + moveSpeed * Time.deltaTime * direction);
            }
           
        }
    }

    void staminaHandler(float change)
    {
        if ((stamina == 100 && change > 0) || (stamina == 0 && change < 0)) return;
        float percent = (stamina + change <= fullStamina) ? change : fullStamina - stamina;
        manager.changeStaminaPercent(playerNum, percent);
        stamina += percent;
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
        
        if (Poly.ContainsPoint(verArr, manager.getCurrentEgg().transform.position)) {
            // manager.getCurrentEgg().GetComponent<Animator>().SetBool("PickedUp", true);
            GameObject curEgg = manager.spawnFood("egg");
            manager.getCurrentEgg().GetComponent<Animator>().SetBool("PickedUp", false);
            StartCoroutine(showAnimation(curEgg));
            manager.addPoint(playerNum, "egg");
            addLink();
        } else if (Poly.ContainsPoint(verArr, manager.getCurrentTomato().transform.position)) {
            // manager.getCurrentEgg().GetComponent<Animator>().SetBool("PickedUp", true);
            GameObject curTomato = manager.spawnFood("tomato");
            manager.getCurrentTomato().GetComponent<Animator>().SetBool("PickedUp", false);
            StartCoroutine(showAnimation(curTomato));
            manager.addPoint(playerNum, "tomato");
            addLink();
        }
    }

    IEnumerator showAnimation(GameObject curFood) {
        curFood.GetComponent<Animator>().SetBool("PickedUp", true);
        yield return new WaitForSecondsRealtime(1);
        Destroy(curFood);
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
        manager.soundSource.PlayOneShot(manager.sounds[playerNum]);
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
