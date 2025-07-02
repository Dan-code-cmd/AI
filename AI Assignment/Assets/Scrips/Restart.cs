using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Restart : MonoBehaviour
{
    [SerializeField]
    KeyCode KeyRestart;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyRestart)) 
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
