using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameStatisticsApi;
using GameStatisticsApi.ResponseData;
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
  private int _currentSessionId ;
  private const string FileName = "profile.dat";
  private string  _saveFilePath;
  private int _activeSlotIndex = -1 ; // assume that now slot is currently loaded
#region UnityEditor
  [SerializeField] private int _maxSaveSlots = 3 ; // assuming we run with up to three save slots

  [SerializeField] private int _minsToAutoSave = 5 ;    // mins until Auto Save is invoked
#endregion


#region MonoBehaviour
  private void Awake()
  {
    if( _instance != null )
    {
      throw new Exception( "Program tried to implement a nw instance of SaveManager, but one already existed" ) ;
    }
    _instance = this;
    _saveFilePath = Path.Combine(Application.persistentDataPath, FileName);
    LoadUserProfile() ;
  }
  private void LoadUserProfile()
  {
    // Check if the user i already initialized in the system
    if ( _activeUserProfile == null ) InitializeUser() ;
    // If it is just load all the save data of the profile
    // if it isn't, create a new user
    // 
  }
  public void InitializeUser()
  {
    _activeUserProfile = new UserProfile() ;
    _activeUserProfile.UserName = "UserName" ;
    for ( int i = 0 ; i < _maxSaveSlots ; i++)
    {
      _activeUserProfile.UserSaveDatas.Add( new UserProfile.UserSaveData 
      {
        SessionId = -1,  // UninitializedSession
        statistics = new UserProfile.UserSaveStatistics() 
      }) ;
    }
  }
  public void InitializeActiveSave(int slotIndex)
  {
    if (slotIndex < 0 || slotIndex >= _activeUserProfile.UserSaveDatas.Count ) return ;
    _activeSlotIndex = slotIndex ;
    var activeData = _activeUserProfile.UserSaveDatas[_activeSlotIndex] ;
    if (activeData.SessionId <= 0)
    {
      StartCoroutine( RestApi.AddSession( DateTime.Now , DateTime.Now , (newId) => {
        var data = _activeUserProfile.UserSaveDatas[slotIndex] ;
        data.SessionId = newId ;
        _activeUserProfile.UserSaveDatas[slotIndex] = data ;

        SaveProfileLocal() ;
        
      }));
    }
  }
#endregion

/// <summary>
/// Called by the Save Button, Serializes the Data of the active User profile and passes it on to the API to request a response for Data storage, either updating or creating a new save file
/// </summary>
#region Save/Load
  public void SaveProfileLocal()
  {
    _saveFilePath = Path.Combine(Application.persistentDataPath, _activeUserProfile.UserName + "_profile.dat");

    string json = SerializeData() ;
    byte[] encryptedData = SaveSystem.Encrypt(json) ;
    File.WriteAllBytes(_saveFilePath, encryptedData);

    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }
  
  public void SyncStatistics()
  {
    if ( _activeSlotIndex >= 0 ) return ;
    var activeData = _activeUserProfile.UserSaveDatas[_activeSlotIndex] ;
    StartCoroutine( RestApi.UpdateSession(
      activeData.SessionId,
      activeData.statistics.TimeStartedAt,
      DateTime.Now,
      ()
    )) ;
  }
  public void LoadProfile( int sessionId )
  {
    StartCoroutine( RestApi.GetSession( sessionId, (onResult) =>
    {
      if( !string.IsNullOrEmpty( onResult ) )
      {
        byte[] cipherData = Convert.FromBase64String( onResult ) ;
        string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
        _currentSessionId = sessionId ;
        DeserializeData( decryptedJson ) ;
        Debug.Log( $"Profile {sessionId} loaded and deserialized" ) ;
      }
    })) ;
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }

  public void DeleteProfile( int sessionId )
  {
    StartCoroutine( RestApi.Endpoints.Session.Get( sessionId, (onResult) =>
    {
      if( !string.IsNullOrEmpty( onResult ) )
      {
        RestApi.DeleteSession( sessionId ) ;
        Debug.Log( $"Profile {sessionId} has been deleted" ) ;
      }
    })) ;
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }
/// <summary>
/// Automatically save the Profile, not implemented yet
/// </summary>
  private IEnumerator AutoSave()
  {
    while (true)
    {
      yield return new WaitForSecondsRealtime( _minsToAutoSave * 60 ) ;
      SaveProfileLocal() ;
      Debug.Log( $"AutoSave initiated." ) ;
    }
  }
#endregion

/// <summary>
/// Copied from older project, probably needs some adjustments
/// </summary>
#region Encryption/Decryption
  public static class SaveSystem
  {
    private static readonly byte[] Key = Encoding.UTF8.GetBytes( "0123456789abcdef" ) ;
    private static readonly byte[] Iv = Encoding.UTF8.GetBytes( "abcdef0123456789" ) ;
    public static byte[] Encrypt( string plainText )
    {
      if ( string.IsNullOrEmpty( plainText ))  throw new ArgumentNullException( nameof( plainText ) );

      using Aes aes = Aes.Create();
      aes.Key = Key;
      aes.IV = Iv;
      using MemoryStream memoryStream = new() ;
      ICryptoTransform encryptor = aes.CreateEncryptor( aes.Key , aes.IV ) ;
      using (CryptoStream cryptoStream = new( memoryStream , encryptor , CryptoStreamMode.Write ) )
      using (StreamWriter writer = new( cryptoStream , Encoding.UTF8 ) )
      {
        writer.Write( plainText ) ;
      }
      return memoryStream.ToArray();
    }

    public static string Decrypt(byte[] cipherData)
    {
      if (cipherData == null || cipherData.Length == 0)
          throw new ArgumentNullException(nameof(cipherData));

      using Aes aes = Aes.Create();
      aes.Key = Key;
      aes.IV = Iv;
      using MemoryStream memoryStream = new(cipherData);
      using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
      using StreamReader reader = new(cryptoStream, Encoding.UTF8);
      return reader.ReadToEnd();
    }
  }

#endregion
#region Synchronization

#endregion

#region De-/Serialization
private string SerializeData() { return JsonUtility.ToJson( _activeUserProfile ) ; }
private void DeserializeData( string jsonData ) { _activeUserProfile = JsonUtility.FromJson<UserProfile>( jsonData ) ; }
#endregion
}