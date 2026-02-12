using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
  }
#endregion

/// <summary>
/// Called by the Save Button, Serializes the Data of the active User profile and passes it on to the API to request a response for Data storage, either updating or creating a new save file
/// </summary>
#region Save/Load
public void SaveProfile()
  {
    string json = SerializeData() ;
    byte[] encryptedData = SaveSystem.Encrypt(json) ;
    if( _currentSessionId > 0 )
    {
      StartCoroutine( RestApi.UpdateSession( _activeUserProfile.UserId, encryptedData, (onResult) =>
      {
        Debug.Log( $"{_activeUserProfile.UserId} :Existing file updated." ) ;
      })) ;
    }
    else
    {
      StartCoroutine( RestApi.AddSession( encryptedData , (onResult) =>
      {
        Debug.Log( $"New save file under {_activeUserProfile.UserId} created" ) ;
      })) ;
    }
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
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
      SaveProfile() ;
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


#region De-/Serialization
private string SerializeData() { return JsonUtility.ToJson( _activeUserProfile ) ; }
private void DeserializeData( string jsonData ) { _activeUserProfile = JsonUtility.FromJson<UserProfile>( jsonData ) ; }
#endregion
}