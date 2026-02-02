using UnityEngine;
using System;
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
  private void Awake()
  {
    if( _instance != null )
    {
      throw new Exception("Program tried to implement a nw instance of UIManager, but one already existed") ;
    }
    _instance = this;
  }
  #endregion
  #region Unity Editor
  // The list holds the information of available panels to toggle, depending on Events and informing the Statistics tracker of it
//  [SerializeField] private List<GameObject> _panels ;

  #endregion
  #region MonoBehaviour
// Update is called once per frame
  void Update()
  {

  }
  #endregion
}
#region Serializable
#endregion