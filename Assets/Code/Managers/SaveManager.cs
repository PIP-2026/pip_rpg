using System;
using System.Collections;
using GameStatisticsApi;
using GameStatisticsApi.ResponseData;
using UnityEngine;
using UnityEngine.Networking;

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
  private int _currentSessionId ;

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
public void SaveProfile()
  {
    string json = SerializeData() ;
    WWWForm form = new() ;
    form.AddField( "profileData" , json ) ;
    int[] ids = new int[] { _activeUserProfile.UserId, _currentSessionId } ;
    if( _currentSessionId > 0 )
    {
      StartCoroutine( RestApi.Session.Put( ids, form, (onResult) =>
      {
        Debug.Log( $"{ids} :Existing file updated." ) ;
      })) ;
    }
    else
    {
      StartCoroutine( RestApi.Session.Post( ids, form , (onResult) =>
      {
        Debug.Log( $"New save file under {ids} created" ) ;
      })) ;
    }
  }
public void LoadProfile( int sessionId )
  {
    StartCoroutine( RestApi.Session.Get( sessionId, (onResult) =>
    {
      if( !string.IsNullOrEmpty( onResult ) )
      {
        _currentSessionId = sessionId ;
        DeserializeData( onResult ) ;
        Debug.Log( $"Profile {sessionId} loaded and deserialized" ) ;
      }
    })) ;
  }
private IEnumerator AutoSave()
  {
    while (true)
    {
      yield return new WaitForSecondsRealtime( _minsToAutoSave * 60 ) ;
      SaveProfile() ;
      Debug.Log( $"AutoSave initiated." ) ;
    }
  }
#endregion


#region Encryption/Decryption
private void EncryptData() { throw new NotImplementedException() ; }
private void DecryptData() { throw new NotImplementedException() ; }
#endregion


#region De-/Serialization
private string SerializeData() { return JsonUtility.ToJson( _activeUserProfile ) ; }
private void DeserializeData( string jsonData ) { _activeUserProfile = JsonUtility.FromJson<UserProfile>( jsonData ) ; }
#endregion
}