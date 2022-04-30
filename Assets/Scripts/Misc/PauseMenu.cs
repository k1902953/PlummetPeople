using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    public GameObject pauseUi;
    public bool isPaused = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isPaused = false;
        UpdatePauseMenu();
    }

    private void OnEnable()
    {
        isPaused = false;
        UpdatePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            UpdatePauseMenu();
        }
    }

    private void UpdatePauseMenu()
    {
        pauseUi.SetActive(isPaused);
        Cursor.visible = isPaused;

        if (isPaused)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
}
