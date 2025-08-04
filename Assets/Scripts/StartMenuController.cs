using UnityEngine;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    [Header("Menu UI References")]
    public GameObject startMenuPanel;                                // Main start menu panel
    public GameObject gamePanel;                                     // Game panel (your existing game UI)
    public Button playerVsPlayerButton;                              // 1v1 button
    public Button playerVsAIButton;                                  // Player vs AI button
    public Button backToMenuButton;                                  // Back to menu button (optional)
    
    [Header("Game Mode Display")]
    //public Text gameModeText;                                        // Shows current game mode
    
    [Header("Component References")]
    public GameStateController gameController;                       // Reference to game controller
    
    [Header("Menu Settings")]
    public string playerVsPlayerText = "Player vs Player";
    public string playerVsAIText = "Player vs AI";
    
    private GameMode currentGameMode = GameMode.PlayerVsPlayer;
    
    // Enum for game modes
    public enum GameMode
    {
        PlayerVsPlayer,
        PlayerVsAI
    }

    private void Start()
    {
        // Setup button listeners
        if (playerVsPlayerButton != null)
            playerVsPlayerButton.onClick.AddListener(() => StartGame(GameMode.PlayerVsPlayer));
            
        if (playerVsAIButton != null)
            playerVsAIButton.onClick.AddListener(() => StartGame(GameMode.PlayerVsAI));
            
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);
        
        // Show start menu initially
        ShowStartMenu();
    }

    /// <summary>
    /// Shows the start menu and hides game panel
    /// </summary>
    public void ShowStartMenu()
    {
        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);
            
        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    /// <summary>
    /// Starts the game with selected mode
    /// </summary>
    /// <param name="gameMode">Selected game mode</param>
    public void StartGame(GameMode gameMode)
    {
        currentGameMode = gameMode;
        
        // Hide start menu, show game panel
        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);
            
        if (gamePanel != null)
            gamePanel.SetActive(true);
        
        // Configure game controller based on mode
        if (gameController != null)
        {
            gameController.SetGameMode(gameMode == GameMode.PlayerVsAI);
            gameController.ResetGameForNewMode();
        }
        
        // Update game mode display
       //UpdateGameModeDisplay();
    }

    /// <summary>
    /// Returns to start menu from game
    /// </summary>
    public void BackToMenu()
    {
        // Reset game state
        if (gameController != null)
        {
            gameController.ResetToInitialState();
        }
        
        ShowStartMenu();
    }

    /// <summary>
    /// Updates the game mode display text
    /// </summary>
    /*private void UpdateGameModeDisplay()
    {
        if (gameModeText != null)
        {
            gameModeText.text = currentGameMode == GameMode.PlayerVsAI ? playerVsAIText : playerVsPlayerText;
        }
    }*/

    /// <summary>
    /// Returns current game mode
    /// </summary>
    public GameMode GetCurrentGameMode()
    {
        return currentGameMode;
    }

    public void Quit()
    {
        Application.Quit();
    }
}