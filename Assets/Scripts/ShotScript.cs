using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    public JachManager manager;
    public float shotSpeed;
    private int id;
    private int player;

    // Start is called before the first frame update
    
    public void setId(int i)
    {
        id = i;
    }

    public int getId()
    {
        return id;
    }

    public void setPlayer(int p)
    {
        player = p;
    }

    public int setPlayer()
    {
        return player;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position +=  transform.right * Time.deltaTime * shotSpeed;
        if (Mathf.Abs(transform.position.x) > 17 || Mathf.Abs(transform.position.y) > 11)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<BodyScript>().playerNum == player) return;
        }
        else if (collision.gameObject.tag == "Shot") return;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        {
            GameObject a = collision.gameObject;
            if (collision.gameObject.tag == "Player")
            {
                if (collision.gameObject.GetComponent<BodyScript>().playerNum == player) return;
                collision.gameObject.GetComponent<BodyScript>().takeHit();
            }
            else if (collision.gameObject.tag == "Shot") return;
            gameObject.SetActive(false);
        }
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Player")
    //    {
    //        if (collision.gameObject.GetComponent<BodyScript>().playerNum == player) return;
    //    }
    //    else if (collision.gameObject.tag == "Shot") return;
    //    gameObject.SetActive(false);
    //}
}
