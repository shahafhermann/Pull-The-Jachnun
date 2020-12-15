using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsMenu : MonoBehaviour {
    
    public GameObject mainMenu;

    public void Back() {
        gameObject.SetActive(false);
    }
}
