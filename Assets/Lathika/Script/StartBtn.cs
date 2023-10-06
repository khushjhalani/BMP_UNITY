using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class StartBtn : MonoBehaviour
{
   
   public void PlayButton()
    {
       SceneManager.LoadScene("Triple Frontier Test");
    }
   
}