using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameStateController : MonoBehaviour
{
    [Header("TitleBar References")]
    public Image player1Icon;                                        // Reference to the playerX icon
    public Image player2Icon;                                        // Reference to the playerO icon
    public InputField player1InputField;                             // Reference to P1 input field
    public InputField player2InputField;                             // Reference to P2 input field
    public Text winnerText;                                          // Displays the winners name

    [Header("Misc References")]
    public GameObject endGameState;                                  // Game footer container + winner text

    [Header("Asset References")]
    public Sprite tilePlayerO;                                       // Sprite reference to O tile
    public Sprite tilePlayerX;                                       // Sprite reference to X tile
    public Sprite tileEmpty;                                         // Sprite reference to empty tile
    public Text[] tileList;                                          // Gets a list of all the tiles in the scene

    [Header("GameState Settings")]
    public Color inactivePlayerColor;                                // Color to display for the inactive player icon
    public Color activePlayerColor;                                  // Color to display for the active player icon
    public string whoPlaysFirst;                                     // Who plays first (X : O)

    [Header("AI Settings")]
    public float aiMoveDelay = 1.0f;                                // Delay before AI makes move (in seconds)
    private bool isAIEnabled = false;                               // AI mode (controlled by StartMenu)
    
    [Header("Private Variables")]
    private string playerTurn;                                       // Internal tracking whos turn is it
    private string player1Name;                                      // Player1 display name
    private string player2Name;                                      // Player2 display name
    private int moveCount;                                           // Internal move counter
    private string aiPlayerSign = "O";                               // Which sign AI is playing (starts with O)
    private string humanPlayerSign = "X";                            // Which sign human is playing (starts with X)
    private bool isAIThinking = false;                               // Prevents multiple AI moves

    private void Start()
    {
        // Set the internal tracker of whos turn is first and setup UI icon feedback for whos turn it is
        playerTurn = whoPlaysFirst;
        UpdatePlayerIconColors();

        //Adds a listener to the name input fields and invokes a method when the value changes. This is a callback.
        player1InputField.onValueChanged.AddListener((_) => OnPlayer1NameChanged());
        player2InputField.onValueChanged.AddListener((_) => OnPlayer2NameChanged());

        // Set the default values to what the inputField text is
        player1Name = player1InputField.text;
        player2Name = player2InputField.text;
        
        // Update input field states
        UpdateInputFieldStates();
        
        // Check if AI should make first move
        if (isAIEnabled && playerTurn == aiPlayerSign)
        {
            StartCoroutine(MakeAIMove());
        }
    }

    /// <summary>
    /// Updates player icon colors based on current turn
    /// </summary>
    private void UpdatePlayerIconColors()
    {
        if (playerTurn == "X")
        {
            player1Icon.color = activePlayerColor;
            player2Icon.color = inactivePlayerColor;
        }
        else
        {
            player1Icon.color = inactivePlayerColor;
            player2Icon.color = activePlayerColor;
        }
    }

    /// <summary>
    /// Called at the end of every turn to check for win conditions
    /// Hardcoded all possible win conditions (8)
    /// We just take position of tiles and check the neighbours (within a row)
    /// 
    /// Tiles are numbered 0..8 from left to right, row by row, example:
    /// [0][1][2]
    /// [3][4][5]
    /// [6][7][8]
    /// </summary>
    public void EndTurn()
    {
        moveCount++;
        if (tileList[0].text == playerTurn && tileList[1].text == playerTurn && tileList[2].text == playerTurn) GameOver(playerTurn);
        else if (tileList[3].text == playerTurn && tileList[4].text == playerTurn && tileList[5].text == playerTurn) GameOver(playerTurn);
        else if (tileList[6].text == playerTurn && tileList[7].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[0].text == playerTurn && tileList[3].text == playerTurn && tileList[6].text == playerTurn) GameOver(playerTurn);
        else if (tileList[1].text == playerTurn && tileList[4].text == playerTurn && tileList[7].text == playerTurn) GameOver(playerTurn);
        else if (tileList[2].text == playerTurn && tileList[5].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[0].text == playerTurn && tileList[4].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[2].text == playerTurn && tileList[4].text == playerTurn && tileList[6].text == playerTurn) GameOver(playerTurn);
        else if (moveCount >= 9) GameOver("D");
        else
            ChangeTurn();
    }

    /// <summary>
    /// Changes the internal tracker for whos turn it is
    /// </summary>
    public void ChangeTurn()
    {
        // Change turn
        playerTurn = (playerTurn == "X") ? "O" : "X";
        UpdatePlayerIconColors();
        
        // If AI is enabled and it's AI's turn, make AI move
        if (isAIEnabled && playerTurn == aiPlayerSign && !isAIThinking)
        {
            StartCoroutine(MakeAIMove());
        }
    }

    /// <summary>
    /// AI Move Coroutine with delay for realistic gameplay
    /// </summary>
    private IEnumerator MakeAIMove()
    {
        isAIThinking = true;
        yield return new WaitForSeconds(aiMoveDelay);
        
        int bestMove = GetBestAIMove();
        if (bestMove != -1)
        {
            // Make the AI move
            TileController tileController = tileList[bestMove].GetComponentInParent<TileController>();
            tileController.UpdateTile();
        }
        
        isAIThinking = false;
    }

    /// <summary>
    /// Medium Level AI Strategy
    /// 1. Check if AI can win in this move
    /// 2. Check if AI needs to block player from winning
    /// 3. Take center if available
    /// 4. Take corners if available
    /// 5. Take any available spot
    /// </summary>
    private int GetBestAIMove()
    {
        // 1. Check if AI can win
        int winMove = FindWinningMove(aiPlayerSign);
        if (winMove != -1) return winMove;
        
        // 2. Check if AI needs to block player
        int blockMove = FindWinningMove(humanPlayerSign);
        if (blockMove != -1) return blockMove;
        
        // 3. Take center if available
        if (IsEmptyTile(4)) return 4;
        
        // 4. Take corners in order of preference
        int[] corners = {0, 2, 6, 8};
        foreach (int corner in corners)
        {
            if (IsEmptyTile(corner)) return corner;
        }
        
        // 5. Take any available edge
        int[] edges = {1, 3, 5, 7};
        foreach (int edge in edges)
        {
            if (IsEmptyTile(edge)) return edge;
        }
        
        return -1; // No move available
    }

    /// <summary>
    /// Finds if a player can win in the next move
    /// </summary>
    private int FindWinningMove(string player)
    {
        // Check all winning combinations
        int[][] winPatterns = {
            new int[] {0,1,2}, new int[] {3,4,5}, new int[] {6,7,8}, // Rows
            new int[] {0,3,6}, new int[] {1,4,7}, new int[] {2,5,8}, // Columns
            new int[] {0,4,8}, new int[] {2,4,6}                     // Diagonals
        };
        
        foreach (int[] pattern in winPatterns)
        {
            int playerCount = 0;
            int emptyIndex = -1;
            
            for (int i = 0; i < pattern.Length; i++)
            {
                if (tileList[pattern[i]].text == player)
                {
                    playerCount++;
                }
                else if (tileList[pattern[i]].text == "")
                {
                    emptyIndex = pattern[i];
                }
            }
            
            // If player has 2 in a row and there's an empty spot, return that spot
            if (playerCount == 2 && emptyIndex != -1)
            {
                return emptyIndex;
            }
        }
        
        return -1;
    }

    /// <summary>
    /// Checks if a tile is empty
    /// </summary>
    private bool IsEmptyTile(int index)
    {
        return tileList[index].text == "";
    }

    /// <summary>
    /// Called when the game has found a win condition or draw
    /// </summary>
    /// <param name="winningPlayer">X O D</param>
    private void GameOver(string winningPlayer)
    {
        string displayName = "";
        switch (winningPlayer)
        {
            case "D":
                displayName = "DRAW";
                break;
            case "X":
                displayName = (isAIEnabled && aiPlayerSign == "X") ? "AI WINS!" : 
                             (aiPlayerSign == "O" ? player1Name : player2Name);
                break;
            case "O":
                displayName = (isAIEnabled && aiPlayerSign == "O") ? "AI WINS!" : 
                             (aiPlayerSign == "X" ? player1Name : player2Name);
                break;
        }
        
        winnerText.text = displayName;
        endGameState.SetActive(true);
        ToggleButtonState(false);
    }

    /// <summary>
    /// Restarts the game state and switches signs
    /// </summary>
    public void RestartGame()
    {
        // Switch signs for next game
        SwitchPlayerSigns();
        
        // Reset some gamestate properties
        moveCount = 0;
        playerTurn = whoPlaysFirst;
        ToggleButtonState(true);
        endGameState.SetActive(false);
        isAIThinking = false;

        // Loop through all tiles and reset them
        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<TileController>().ResetTile();
        }
        
        UpdatePlayerIconColors();
        //UpdateInputFieldStates();
        
        // Check if AI should make first move
        if (isAIEnabled && playerTurn == aiPlayerSign)
        {
            StartCoroutine(MakeAIMove());
        }
    }

    /// <summary>
    /// Switches player signs after each game
    /// </summary>
    private void SwitchPlayerSigns()
    {
        if (isAIEnabled)
        {
            // Switch AI and Human signs
            string temp = aiPlayerSign;
            aiPlayerSign = humanPlayerSign;
            humanPlayerSign = temp;
            
            //Update icon
            player1Icon.sprite = humanPlayerSign == "X" ? tilePlayerX : tilePlayerO;
            player2Icon.sprite = aiPlayerSign == "X" ? tilePlayerX : tilePlayerO;
        }
        else
        {
            // In player vs player, just alternate who starts
            // This creates the same switching effect
        }
    }

    /// <summary>
    /// Sets game mode (called by StartMenuController)
    /// </summary>
    public void SetGameMode(bool enableAI)
    {
        isAIEnabled = enableAI;
        
        // Reset signs when changing mode
        aiPlayerSign = "O";
        humanPlayerSign = "X";
        
        UpdateInputFieldStates();
    }

    /// <summary>
    /// Resets game for new mode
    /// </summary>
    public void ResetGameForNewMode()
    {
        // Reset all game state
        moveCount = 0;
        playerTurn = whoPlaysFirst;
        ToggleButtonState(true);
        endGameState.SetActive(false);
        isAIThinking = false;

        // Loop through all tiles and reset them
        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<TileController>().ResetTile();
        }
        
        UpdatePlayerIconColors();
        UpdateInputFieldStates();
        
        // Check if AI should make first move
        if (isAIEnabled && playerTurn == aiPlayerSign)
        {
            StartCoroutine(MakeAIMove());
        }
    }

    /// <summary>
    /// Resets to initial state (for returning to menu)
    /// </summary>
    public void ResetToInitialState()
    {
        // Stop any ongoing AI thinking
        StopAllCoroutines();
        isAIThinking = false;
        
        // Reset all game variables
        moveCount = 0;
        playerTurn = whoPlaysFirst;
        isAIEnabled = false;
        aiPlayerSign = "O";
        humanPlayerSign = "X";
        
        // Reset UI
        ToggleButtonState(true);
        endGameState.SetActive(false);
        
        // Reset tiles
        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<TileController>().ResetTile();
        }
        
        UpdatePlayerIconColors();
        UpdateInputFieldStates();
    }

    /// <summary>
    /// Toggles AI mode on/off (legacy method, now controlled by StartMenu)
    /// </summary>
    public void ToggleAI()
    {
        isAIEnabled = !isAIEnabled;
        UpdateInputFieldStates();
        
        // Reset signs when toggling AI
        aiPlayerSign = "O";
        humanPlayerSign = "X";
        
        // If enabling AI and it's AI's turn, make move
        if (isAIEnabled && playerTurn == aiPlayerSign && moveCount < 9)
        {
            StartCoroutine(MakeAIMove());
        }
    }

    /// <summary>
    /// Updates AI toggle button text (legacy method)
    /// </summary>
    private void UpdateAIToggleButton()
    {
        // This method is kept for backward compatibility
        // but functionality moved to StartMenuController
    }

    /// <summary>
    /// Updates input field states based on AI mode
    /// </summary>
    private void UpdateInputFieldStates()
    {
        if (isAIEnabled)
        {
            // Disable AI player's input field
            if (aiPlayerSign == "X")
            {
                player1InputField.interactable = false;
                player1InputField.text = "AI Player";
                player2InputField.interactable = true;
            }
            else
            {
                player2InputField.interactable = false;
                player2InputField.text = "AI Player";
                player1InputField.interactable = true;
            }
        }
        else
        {
            // Enable both input fields
            player1InputField.interactable = true;
            player2InputField.interactable = true;
            player1InputField.text = string.IsNullOrEmpty(player1Name) ? "Player 1" : player1Name;
            player2InputField.text = string.IsNullOrEmpty(player2Name) ? "Player 2" : player2Name;
        }
    }

    /// <summary>
    /// Enables or disables all the buttons
    /// </summary>
    private void ToggleButtonState(bool state)
    {
        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<Button>().interactable = state;
        }
    }

    /// <summary>
    /// Returns the current players turn (X / O)
    /// </summary>
    public string GetPlayersTurn()
    {
        return playerTurn;
    }

    /// <summary>
    /// Returns the display sprite (X / O)
    /// </summary>
    public Sprite GetPlayerSprite()
    {
        if (playerTurn == "X") return tilePlayerX;
        else return tilePlayerO;
    }

    /// <summary>
    /// Returns if current player is AI
    /// </summary>
    public bool IsCurrentPlayerAI()
    {
        return isAIEnabled && playerTurn == aiPlayerSign;
    }

    /// <summary>
    /// Callback for when the P1_textfield is updated. We just update the string for Player1
    /// </summary>
    public void OnPlayer1NameChanged()
    {
        player1Name = player1InputField.text;
    }

    /// <summary>
    /// Callback for when the P2_textfield is updated. We just update the string for Player2
    /// </summary>
    public void OnPlayer2NameChanged()
    {
        player2Name = player2InputField.text;
    }
}