using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class JachManager : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;
    public new GameObject camera;
    public AudioSource soundSource;
    public List<AudioClip> sounds;
    public GameObject bar1;
    public GameObject bar2;
    public GameObject mask1;
    public GameObject mask2;
    public Canvas uiCanvas;
    public GameObject endScreenPrefab;
    public GameObject shotPrefab;
    public Sprite winner2;
    public Sprite tie;
    public float maxCameraDistance;
    public float viewMargin;
    public float zoomTime;
    private float zoomCurr;
    public List<GameObject> links;
    public List<GameObject> foods;
    
    private GameObject curEgg;
    public GameObject eggPrefab;
    public Image eggWayPoint;
    private GameObject curTomato;
    public GameObject tomatoPrefab;
    public Image tomatoWayPoint;
    public Vector3 eggWayPointOffset;
    public Vector3 tomatoWayPointOffset;
    
    Vector3 startPos;
    private float staminaDrainFactor;

    public bool timeUp = false;
    public float roundTime;
    public Text timerText;
    
    private int p1Score = 0;
    private int p2Score = 0;
    private int p1Eggs = 0;
    private int p2Eggs = 0;
    private int p1Tomatoes = 0;
    private int p2Tomatoes = 0;
    public Sprite[] numbers;
    public Image p1TomatoImg;
    public Image p1EggImg;
    public Image p2TomatoImg;
    public Image p2EggImg;

    private GameObject[] shotPool;
    int currShotIdx;

    private void Awake()
    {
        foods = new List<GameObject>();
        links = new List<GameObject>();
        links.Add(GameObject.Find("mid_link"));
    }

    void Start()
    {
        Time.timeScale = 1;
        startPos = camera.transform.position;
        soundSource = camera.GetComponent<AudioSource>();
        zoomCurr = zoomTime;
        
        spawnFood("egg");
        spawnFood("tomato");
        
        shotPool = new GameObject[30];
        staminaDrainFactor = 0;
        for (int i = 0; i < 20; i++)
        {
            shotPool[i] = Instantiate(shotPrefab);
            shotPool[i].GetComponent<ShotScript>().setId(i);
            shotPool[i].SetActive(false);
        }
        currShotIdx = 0;
    }

    public GameObject getShot()
    {
        GameObject shot = shotPool[currShotIdx];
        currShotIdx = (currShotIdx == 29) ? 0 : currShotIdx + 1;
        return shot;
    }

    public void addPoint(int playerNum, String food) {
        switch (playerNum) {
            case 1:
                p1Score++;
                if (food.Equals("egg")) {
                    p1Eggs++;
                    p1EggImg.sprite = numbers[p1Eggs];
                }
                else {
                    p1Tomatoes++;
                    p1Score++;
                    p1TomatoImg.sprite = numbers[p1Tomatoes];
                }
                break;
            case 2:
                p2Score++;
                if (food.Equals("egg")) {
                    p2Eggs++;
                    p2EggImg.sprite = numbers[p2Eggs];
                }
                else {
                    p2Tomatoes++;
                    p2Score++;
                    p2TomatoImg.sprite = numbers[p2Tomatoes];
                }
                break;
        }
    }

    public GameObject getCurrentEgg() {
        return curEgg;
    }
    
    public GameObject getCurrentTomato() {
        return curTomato;
    }
    
    /**
     * Instantiate the first egg at a position different than the player's
     */
    public GameObject spawnFood(String food) {
        GameObject prevFood = food.Equals("egg") ? curEgg : curTomato;
        
        int xPos, yPos;
        bool notValid = true;
        while (notValid) {
            xPos = Random.Range(-14, 15);
            yPos = Random.Range(-8, 6);
            // Check proximity to heads
            if (math.abs(xPos - p1.transform.position.x) < 2 &&
                math.abs(yPos - p1.transform.position.y) < 2 ||
                math.abs(xPos - p2.transform.position.x) < 2 &&
                math.abs(yPos - p2.transform.position.y) < 2) {
                continue;
            }
            
            // Check proximity to body
            bool shouldCont = false;
            foreach (GameObject link in links) {
                if (math.abs(xPos - link.transform.position.x) < 2 &&
                    math.abs(yPos - link.transform.position.y) < 2) {
                    shouldCont = true;
                }
            }
            if (shouldCont) {
                continue; }
            
            // Passed the tests, instantiate
            if (food.Equals("egg")) {
                curEgg = Instantiate(eggPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
            }
            else {
                curTomato = Instantiate(tomatoPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
            }
            // foods.Add(curEgg);
            notValid = false;
        }

        return prevFood;
    }

    Vector3 moveCamera()
    {
        float camX = (p1.transform.position.x + p2.transform.position.x) / 2;
        float camY = (p1.transform.position.y + p2.transform.position.y) / 2;
        float f = camera.GetComponent<Camera>().fieldOfView;
        float distance = (p1.transform.position - p2.transform.position).magnitude;
        float camZ = -Mathf.Sqrt(3) * ((distance / 2) + viewMargin);
        if (zoomCurr <= 0) return new Vector3(camX, camY, camZ);
        zoomCurr -= Time.deltaTime;
        return Vector3.Lerp(new Vector3(camX, camY, camZ), startPos, zoomCurr / zoomTime);
    }

    public void changeStaminaPercent(int player, float percent)
    {
        GameObject stamina = (player == 1) ? bar1 : bar2;
        GameObject mask = (player == 1) ? mask1 : mask2;
        if (staminaDrainFactor == 0) staminaDrainFactor =
                stamina.transform.GetComponent<RectTransform>().rect.height * uiCanvas.scaleFactor;
        Vector3 move = staminaDrainFactor * (percent / 100) *  Vector3.up;
        stamina.transform.position -= move;
        mask.transform.position += move;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        
        camera.transform.position = moveCamera(); 
        
        showWayPoints();
        
        if (roundTime > 0) {
            roundTime -= Time.deltaTime;
            roundTime = (roundTime < 0) ? 0 : roundTime;
            timerText.text = ((int) roundTime).ToString();
        }
        else {
            endGame();
        }
    }

    /**
     * Food Waypoints
     */
    private void showWayPoints() {
        // Egg
        // if (!curEgg.GetComponent<Renderer>().isVisible) {
        //     if (!eggWayPoint.enabled) {
        //         eggWayPoint.enabled = true;
        //     }
            
        float minX = eggWayPoint.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;
        float minY = eggWayPoint.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector2 pos = Camera.main.WorldToScreenPoint(curEgg.transform.position + eggWayPointOffset);
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        eggWayPoint.transform.position = pos;
        // }
        // else if (eggWayPoint.enabled) {
        //     eggWayPoint.enabled = false;
        // }
        
        // Tomato
        // if (!curTomato.GetComponent<Renderer>().isVisible) {
        //     if (!tomatoWayPoint.enabled) {
        //         tomatoWayPoint.enabled = true;
        //     }
            
        minX = tomatoWayPoint.GetPixelAdjustedRect().width / 2;
        maxX = Screen.width - minX;
        minY = tomatoWayPoint.GetPixelAdjustedRect().height / 2;
        maxY = Screen.height - minY;

        pos = Camera.main.WorldToScreenPoint(curTomato.transform.position + tomatoWayPointOffset);
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        tomatoWayPoint.transform.position = pos;
        // }
        // else if (tomatoWayPoint.enabled) {
        //     tomatoWayPoint.enabled = false;
        // }
        
    }

    void endGame()
    {
        Time.timeScale = 0;
        GameObject endScreen = Instantiate(endScreenPrefab);
        Image winner = endScreen.transform.Find("playerWinner").GetComponent<Image>();
        if (p1Score < p2Score)
        {
            winner.sprite = winner2;
        }
        // TODO: else if (p1Score == p2Score)  winner.sprite = tie;
        Button playAgain = endScreen.transform.Find("PlayAgainButton").GetComponent<Button>();
        playAgain.onClick.AddListener(newGame);
        soundSource.clip = sounds[0];
        soundSource.Play();
    }
    void newGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
