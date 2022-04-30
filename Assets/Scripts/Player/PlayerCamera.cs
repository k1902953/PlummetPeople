using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviourPun
{
    public Transform player;
    float heading = 0;
    float tilt = 15;
    public float camDist = 3;
    public float camDistDefault = 3;
    public float playerHeight = 1.25f;
    float defaultPlayerHeight;
    float scroll;
    float camMin = 1.5f;
    float camMax = 5f;
    public float camRight;
    public CursorLockMode cursor;
    public float mouseSensitivity = 350f; // make this change based on user settings

    private void Awake()
    {
        if (photonView.IsMine)
        {
            PlayerManager.localCameraInstance = gameObject;
            transform.parent.GetComponent<PlayerMovement>().cam = transform;
            player = transform.parent;
            DontDestroyOnLoad(gameObject);
            transform.parent = null;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        defaultPlayerHeight = playerHeight;
        SetMouseLock(cursor);
    }

    public void SetMouseLock(CursorLockMode thisCursor)
    {
        Cursor.lockState = thisCursor;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (!player || PauseMenu.instance.isPaused) return;

        heading += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        tilt += Input.GetAxis("Mouse Y") * Time.deltaTime * -mouseSensitivity;

        tilt = Mathf.Clamp(tilt, -14f, 85);

        transform.rotation = Quaternion.Euler(tilt, heading, 0);

        transform.position = player.position - (transform.forward * camDist) + (Vector3.up * playerHeight) + (Vector3.right * camRight);
    }

    private void Update()
    {
        if (!player) return;
        DoDrawRay();
        CamZoom();
    }

    void CamZoom()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            camAdjustPlus();
        }
        if (scroll < 0)
        {
            camAdjustMinus();
        }
    }

    public LayerMask blockCamLayers;
    float hitDistance;

    private void DoDrawRay()
    {
        RaycastHit hit;
        Vector3 dir = transform.position - (player.transform.position + transform.up * 1);
        if (Physics.Raycast(player.transform.position + transform.up * playerHeight, dir, out hit, camDistDefault, blockCamLayers))
        {
            hitDistance = hit.distance;
            Debug.DrawRay(player.transform.position + transform.up * playerHeight, dir, Color.green);
            //camDist = Mathf.Lerp(camDist, camDistDefault - (camDistDefault - hitDistance), Time.deltaTime * 10);
            camDist = camDistDefault - (camDistDefault - hitDistance);
        }
        else
        {
            hitDistance = 0;
            //camDist = Mathf.Lerp(camDist, camDistDefault, Time.deltaTime * 5);
            camDist = camDistDefault;
            //Debug.DrawRay(player.transform.position + transform.up * playerHeight, dir, Color.white);
        }
    }

    void camAdjustMinus()
    {
        camDistDefault += 0.2f;
        camDistDefault = Mathf.Clamp(camDistDefault, camMin, camMax);
    }
    void camAdjustPlus()
    {
        camDistDefault -= 0.2f;
        camDistDefault = Mathf.Clamp(camDistDefault, camMin, camMax);
    }
}
