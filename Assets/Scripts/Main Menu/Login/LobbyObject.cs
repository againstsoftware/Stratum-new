using UnityEngine;
using UnityEngine.SceneManagement;
public class LobbyObject: MonoBehaviour
{
    public void OnPointerPress()
    {
        SceneManager.LoadScene("Lobby Test");
    }
    
}