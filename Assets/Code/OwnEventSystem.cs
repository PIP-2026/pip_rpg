using UnityEngine;
using System;
using UnityEngine.Events;
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
    public static UnityEvent DialogueSkipped => _instance.statisticsTrackingEvents.dialogueLineSkip ; // Look into configs and wire this via inspector
    public static UnityEvent InteractionInitiated => _instance.statisticsTrackingEvents.anyInteract ; // Look into configs and wire this via inspector
    public static UnityEvent AnyButtonPressed => _instance.statisticsTrackingEvents.anyButtonPress ;  // Listeners wired via code
    public static UnityEvent<int> PlayerMoved => _instance.statisticsTrackingEvents.tilesMoved ;      // Listeners wired via code
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
  public static UnityEvent<GameState, GameState> GameStateChanged => _instance.gameEvents.gameStateChange ;   // Wire this via code
  #endregion
  

  #region MonoBehavior
  private void Awake()
  {
    // generally validate that there is only ever one OurEventSystem instance
    // stop everything and throw an exception if not
    if( _instance != null )
      throw new Exception("Program attempted to create an instance of EventSystem, but one already existed.") ;
    _instance = this ;
  }
  #endregion
  
  
  #region Serializables
  // TODO: Probably have to expand all Events with the identifier of the client
  [Serializable]
  private class StatisticsTrackingEvents 
  {
    [SerializeField] public UnityEvent<GameState, GameState> metaNavigation ; // prev, next
    [SerializeField] public UnityEvent/*<DialogueLineObject>*/ dialogueLineSkip ;
    [SerializeField] public UnityEvent anyButtonPress ;
    [SerializeField] public UnityEvent/*<Actor,IInteractable>*/ anyInteract ;  // to track all player initiated Interactions, can pass InteractionTypes if we want to differentiate between them for more stats
    [SerializeField] public UnityEvent <int> tilesMoved;  // to track player movement
  }
  /// <summary>
  /// I added some suggestions.
  /// This allows for the modular approach to control game progression by these and separating inherent game only logic from the Statistics tracking
  /// </summary>
  // TODO: Probably have to expand all Events with the identifier of the client
  [Serializable]
  private class GameEvents
  {
    [SerializeField] public UnityEvent<GameState, GameState> gameStateChange ;
    // [SerializeField] public readonly UnityEvent gameInitiated;
    // [SerializeField] public readonly UnityEvent questInitiated;
    // [SerializeField] public readonly UnityEvent questFinished;
    // [SerializeField] public readonly UnityEvent questItemCollected;
    // [SerializeField] public readonly UnityEvent transitionPlayArea;
    // [SerializeField] public readonly UnityEvent gameFinished;

  }
/// <summary>
/// Publicly exposed fields for inspector assignments to the Events. People who like assigning their Listeners in the Inspector can do that.
/// </summary>
  [Serializable]
  private class EventSystemConfig 
  {
    [SerializeField] private string helloWorld = "hello world" ;
    // Extend here, whenever you feel like it and validate the reason for the new field to exist and how to implement it
  }

  #endregion
}