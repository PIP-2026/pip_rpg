using System;
using UnityEngine;


/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///     Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/33">link to issue</a>
///     Architecture Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/39">link to architecture</a>
///   </para>
///   <para>
///     This class should maintain a strictly unique instance.
///   </para>
/// </remarks>
/// <summary>
///   The SaveManager is controlling the active clients profile and initializes or keeps track of editions to it.
///   Communicates between Api and application.
/// </summary>


public class SaveManager : MonoBehaviour
{
  private static SaveManager _instance ;
  private UserProfile _activeUserProfile ;

#region UnityEditor
  [SerializeField] private int _minsToAutoSave = 5 ;    // mins until 
#endregion


#region MonoBehaviour
  private void Awake()
  {
    if( _instance != null )
    {
      throw new Exception( "Program tried to implement a nw instance of SaveManager, but one already existed" ) ;
    }
    _instance = this;
  }
#endregion


#region Save/Load
public void SaveProfile() { throw new NotImplementedException() ; }
public void LoadProfile() { throw new NotImplementedException() ; }
private void SetActiveProfile() { throw new NotImplementedException() ; }
private void AutoSave() { throw new NotImplementedException() ; }
#endregion


#region Encryption/Decryption
private void EncryptData() { throw new NotImplementedException() ; }
private void DecryptData() { throw new NotImplementedException() ; }
#endregion


#region De-/Serialization
private void SerializeData() { throw new NotImplementedException() ; }
private void DeserializeData() { throw new NotImplementedException() ; }
#endregion
}