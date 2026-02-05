using UnityEngine;
using UnityEngine.UI;
/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///       Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/21">link to issue</a>
///   </para>
///   <para>
///   Attach this script to the UIPanels you would like to track.
///   Find them in the Folder ../Code/UI/
///   </para>
/// </remarks>
/// <summary>
///   This class is made to hold the base functionality of any Panel in the system. It holds which type of Panel it is and how the UIManager treats it
///   and to communicate between it and the API to track statistics. 
/// </summary>

public class PanelObjectBase : MonoBehaviour
{
  #region UnityEditor
  [SerializeField] private PanelType panelType;   // Assign this in the Inspector
  public PanelType Type => panelType ;
  #endregion
  #region Setup
  #endregion
  #region Behaviour
  // Hook up all the buttons in the Panel to the Event System and make them count for the statistics tracker
  private void Start()
  {
    Button[] buttons = GetComponentsInChildren<Button>( true ) ;
    foreach( Button btn in buttons)
    {
      Button cbtn = btn;
      cbtn.onClick.AddListener( () => OnButtonPress() ) ;
    }
  }
  public void OnButtonPress()
  {
    OurEventSystem.AnyButtonPressed.Invoke();
  }
  #endregion
}
