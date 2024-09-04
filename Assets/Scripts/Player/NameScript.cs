using UnityEngine;

public class NameScript : MonoBehaviour
{
    PlayerMovement activePlayer;

    void Start()
    {
        activePlayer = GetActivePlayer();
    }

    void Update()
    {
        Vector3 lookAwayPosition = transform.position * 2 - activePlayer.transform.position;
        transform.LookAt(lookAwayPosition);
    }

    PlayerMovement GetActivePlayer()
    {
        foreach (PlayerMovement player in GameObject.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (player.enabled)
            {
                return player;
            }
        }
        return null;
    }

    public void SetName(string name)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = name;
    }
}
