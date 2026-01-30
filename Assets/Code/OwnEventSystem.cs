using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEditor.Compilation;
/// <remarks>
///   <para>
///     Author: Maria Lindling / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
///     CoAuthor: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///       Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/25">link to issue</a>
///   </para>
///   <para>
///     This class should maintain a strictly unique instance.
///   </para>
/// </remarks>
/// <summary>
///   Host class for our custom game events, which may be invoked from this class' static properties of the same names.
///   Can be expanded upon further development. Check if an event is solely for tracking purpose only or if it affects Game Logic as well
/// </summary>
public class OurEventSystem : MonoBehaviour
{
  #region Singleton
    private static OurEventSystem _instance ;
    public static GameState CurrentState {get; private set;}
  #endregion
  
  
  #region Unity Editor
    [SerializeField] EventSystemConfig config ;
    [SerializeField] StatisticsTrackingEvents statisticsTrackingEvents ;
    [SerializeField] GameEvents gameEvents ;
  #endregion
  
  
  #region Invokables: StatisticsTracking
    /// <remarks>
    ///   <para>
    ///     Author: Maria Lindling / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Listeners are assigned in the Unity Editor. This should only ever be invoked.
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Invokable UnityEvent to broadcast changes the player's navigation through the game state.
    /// </summary>
    public static UnityEvent<GameState, GameState> MetaNavigation => _instance.statisticsTrackingEvents.metaNavigation ;
    /// NOTE: instead of MetaNavigationAction it might actually be more elegant to instead make the parameters the (prev,next) game state, such as "Exploring", "InMenu" and "InDialogue", etc.
    public static UnityEvent DialogueSkipped => _instance.statisticsTrackingEvents.dialogueLineSkip ;
    public static UnityEvent InteractionInitiated => _instance.statisticsTrackingEvents.anyInteract ;
    public static UnityEvent AnyButtonPressed => _instance.statisticsTrackingEvents.anyButtonPress ;
    public static UnityEvent<int> PlayerMoved => _instance.statisticsTrackingEvents.tilesMoved ;
  #endregion
  

  #region Invokable GameEvents
  /// <remarks>
  ///   <para>
  ///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
  ///   </para>
  ///   <para>
  ///     Listeners are assigned in the Unity Editor. This should only ever be invoked. They affect the logic layer of the game.
  ///     Transitions, critical points, or quest tracking could be controlled with these, without having to flag too many events but connect Quests or Area restrictions with items or other stats
  ///   </para>
  /// </remarks>
  /// <summary>
  ///   Currently 
  /// </summary>
  public static UnityEvent<GameState, GameState> GameStateChanged => _instance.gameEvents.gameStateChange ;
  #endregion
  

  #region MonoBehavior
  private void Awake()
  {
    // generally validate that there is only ever one OurEventSystem instance
    // stop everything and throw an exception if not
    if( _instance != null )
      throw new Exception("Program attempted to create an instance of EventSystem, but one already existed.") ;
      // Destroy the new instance of the object in case one of OurEventSystem is already set
      if(_instance != this)
        {
          Destroy(gameObject) ;
          return ;
        }
    
    _instance = this ;
  }
  /// <summary>
  /// Naming is ambiguous as we have yet to decide if we want to validate the Transition on button press alone or protect this with an API request and extend on the API class
  /// </summary>
  public static void ChangeGameState(GameState next)
  {
    if (next == CurrentState) return ;
    GameState prev = CurrentState ;
    CurrentState = next ;
    _instance.gameEvents.gameStateChange.Invoke(prev, next) ;
    _instance.statisticsTrackingEvents.metaNavigation.Invoke(prev, next) ;

    Debug.Log($"[EventSystem] State changed from {prev} to {next}.") ;
  }
  #endregion
  
  
  #region Serializables
  [Serializable]
  private class StatisticsTrackingEvents 
  {
    [SerializeField] public readonly UnityEvent<GameState, GameState> metaNavigation ; // prev, next
    [SerializeField] public readonly UnityEvent/*<DialogueLineObject>*/ dialogueLineSkip ;
    [SerializeField] public readonly UnityEvent anyButtonPress ;
    [SerializeField] public readonly UnityEvent/*<Actor,IInteractable>*/ anyInteract ;  // to track all player initiated Interactions, can pass InteractionTypes if we want to differentiate between them for more stats
    [SerializeField] public readonly UnityEvent <int> tilesMoved;  // to track player movement
  }
  /// <summary>
  /// I added some suggestions.
  /// This allows for the modular approach to control game progression by these and seperating inherent game only logic from the Statistics tracking
  /// </summary>
  [Serializable]
  private class GameEvents
  {
    [SerializeField] public readonly UnityEvent<GameState, GameState> gameStateChange ;
    // [SerializeField] public readonly UnityEvent gameInitiated;
    // [SerializeField] public readonly UnityEvent questInitiated;
    // [SerializeField] public readonly UnityEvent questFinished;
    // [SerializeField] public readonly UnityEvent questItemCollected;
    // [SerializeField] public readonly UnityEvent transitionPlayArea;
    // [SerializeField] public readonly UnityEvent gameFinished;

  }

  [Serializable]
  private class EventSystemConfig 
  {
    [SerializeField] private string helloWorld = "hello world" ;
    // Extend here, whenever you feel like it and validate the reason for the new field to exist and how to implement it
  }
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
  #endregion
}