using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameUI : MonoBehaviour
{
    public void DestroyWaitingObject()
    {
        GameObject.Destroy(gameObject);
    }
}
