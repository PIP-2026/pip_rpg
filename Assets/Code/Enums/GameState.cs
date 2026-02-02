/// <summary>
/// The GameState is an overarching tool to track statistics and drive a few events in game. other than that we can use the UIManager to control these.
/// If we just don't trust the user we can write it as a request to the API and check if the state change is secure enough.
/// </summary>
public enum GameState
{
  None = 0,  // Default
  Exploration = 1,  // True if Menu Panels are closed and the player just explores the world
  Dialogue = 2,  // True only if the Dialogue Panel is open
  Menu = 3,  // True if any other Menu Panel besides the Dialogue is open
  Loading = 4, // just in case
  GameOver = 5  // If player dies or talks too much? *joke*
}
