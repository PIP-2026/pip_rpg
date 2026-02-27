using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///     Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/21">link to issue</a>
///   </para>
///   <para>
///     This class should maintain a strictly unique instance.
///   </para>
/// </remarks>
/// <summary>
///   The UIManager doesn't hold functionality of HUD or anything of that on its own. It controls nothing else but but the display of UI Elements.
///   The UI Panels are usually in the x = -2000 position and only need to be transformed to x = 0 if you want them displayed
/// </summary>

public class UIManager : MonoBehaviour
{
#region Singleton
  private static UIManager _instance ;
#endregion


#region MonoBehaviour
  private void Awake()
  {
    if( _instance != null )
    {
      throw new Exception( "Program tried to implement a nw instance of UIManager, but one already existed" ) ;
    }
    _instance = this;
  }
#endregion


#region Unity Editor
  // The list holds the information of available panels to toggle, depending on Events and informing the Statistics tracker of it
  [SerializeField] private List<GameObject> _panels ;
  [SerializeField] private GameObject _main_PausePanel ;
  [SerializeField] private GameObject _loadingPanel ;
  [SerializeField] private float _loadingScreenTime = 5f ;
  [SerializeField] private DebugMessages debugMessages ;
#endregion


#region Events
  // To track the statistics we need, track the navigation
  public void HandleGameStateChange( GameState prev , GameState next )
  {
    if ( prev == next ) return ;
    foreach( var p in _panels )
    {
      PanelType panelType = p.GetComponent<PanelObjectBase>().Type ;

      switch ( next )
      {
        case GameState.Exploration:
          // Toggle Panels accordingly
          if( panelType == PanelType.HUD ) p.gameObject.SetActive( true ) ;
          else p.gameObject.SetActive( false ) ;
          break ;
        case GameState.Dialogue:
          if( panelType == PanelType.Dialogue ) p.gameObject.SetActive( true ) ;   // Why you not working?
          else p.gameObject.SetActive( false ) ;
          break ;
        case GameState.Menu:
          if( _main_PausePanel.activeSelf == false ) _main_PausePanel.SetActive( true ) ;
          if( panelType != PanelType.Menu ) p.gameObject.SetActive( false ) ;
          break ;
        case GameState.Loading:
          StartCoroutine( ShowLoadingScreen() ) ;
          break ;
      }
    }
  }
#endregion


#region Coroutines
  private IEnumerator ShowLoadingScreen()
  {
    _loadingPanel.SetActive( true ) ;
    yield return new WaitForSecondsRealtime( _loadingScreenTime ) ;
    _loadingPanel.SetActive( false ) ;
  }
#endregion


#region Serializable

#endregion


#region Tests
  public void TestPause() {
    debugMessages?.testPause.TryInvoke() ;
    OurEventSystem.GameStateChanged.Invoke( GameState.Exploration , GameState.Menu );
  }  // WORKS
     // TODO Put this to the GameManager that will handle it all

  public void TestDialogue(){
    debugMessages?.testDialogue.TryInvoke() ;
    OurEventSystem.GameStateChanged.Invoke(GameState.None, GameState.Dialogue);
  } // Does NOT work yet
  public void TestLoading(){
    debugMessages?.testLoading.TryInvoke() ;
    OurEventSystem.GameStateChanged.Invoke(GameState.None, GameState.Loading);
  } // WORKS
  // TODO Write that down

  public void TestHUD() {
    debugMessages?.testHUD.TryInvoke() ;
    OurEventSystem.GameStateChanged.Invoke(GameState.None, GameState.Exploration ) ;
  }
#endregion


#region DebugMessages
  [Serializable]
  class DebugMessages
  {
    [SerializeField] public DebugMessage testPause ;
    [SerializeField] public DebugMessage testDialogue ;
    [SerializeField] public DebugMessage testLoading ;
    [SerializeField] public DebugMessage testHUD ;
  }
#endregion
}
