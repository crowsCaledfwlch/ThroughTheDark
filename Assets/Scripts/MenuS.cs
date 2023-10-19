using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuS : MonoBehaviour
{
    // Called when we click the "Play" button.
    public void OnSceneButton(int sceneNum)
    {
        SceneManager.LoadScene(sceneNum);
    }
    // Called when we click the "Quit" button.
    public void OnQuitButton()
    {
        Application.Quit();
    }
}