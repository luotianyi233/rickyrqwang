using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoController : MonoBehaviour
{
    public GameObject playerInfoPrefab;
    public Transform tarPoint;

    public Camera Camera1;
    public Camera Camera2;

    private PlayerController playerController;

    Transform playerInfoBar;
    Text info;


    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode == RenderMode.WorldSpace&&playerController!=null)
            {
                switch (playerController.playerNO)
                {
                    case 1:
                        if (canvas.gameObject.layer == LayerMask.NameToLayer("P2UI"))
                        {
                            playerInfoBar = Instantiate(playerInfoPrefab, canvas.transform).transform;
                            info = playerInfoBar.GetChild(0).GetComponent<Text>();
                            info.text = "Joystick";
                        }
                        break;
                    case 2:
                        if (canvas.gameObject.layer == LayerMask.NameToLayer("P1UI"))
                        {
                            playerInfoBar = Instantiate(playerInfoPrefab, canvas.transform).transform;
                            info = playerInfoBar.GetChild(0).GetComponent<Text>();
                            info.text = "Keyboard";
                        }
                        break;
                }

            }
        }
    }

    private void LateUpdate()
    {
        if (playerInfoBar != null && playerController.playerNO == 1)
        {
            playerInfoBar.position = tarPoint.position;
            playerInfoBar.forward = Camera1.transform.forward;
        }
        else if (playerInfoBar != null && playerController.playerNO == 2)
        {
            playerInfoBar.position = tarPoint.position;
            playerInfoBar.forward = Camera2.transform.forward;
        }
    }
}
