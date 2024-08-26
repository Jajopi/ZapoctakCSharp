using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameUI : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void DestroyWaitingObject()
    {
        GameObject.Destroy(gameObject);
    }
}
