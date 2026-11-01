using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
   [Header("UI References")]
   [SerializeField] private GameObject pauseMenuUI;

   [Header("Pause Settings")]
   private bool isPaused = false;

   private void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Ensure the game starts unpaused
    }

    public void OnOpenMenu(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
    }
}
