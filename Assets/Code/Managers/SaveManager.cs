using System.Net.Security;
using System.Runtime.Serialization;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameStatisticsApi;
using GameStatisticsApi.ResponseData;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

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
  private int _activeSlotIndex = -1 ; // assume that now slot is currently loaded

  // Directories
  private const string RootFolderName = "UserProfiles" ;
  private const string SavesFolderName = "SaveDatas" ;
  private const string FilesFolderName = "SaveFiles" ;
  private string UserProfilesPath => Path.Combine( Application.persistentDataPath , RootFolderName ) ;
#region UnityEditor
  [SerializeField] private int _maxSaveSlots = 3 ; // assuming we run with up to three save slots
  [SerializeField] private int _maxSaveFiles = 8; // Assuming up to 8 different SaveFiles for a given Slot
  [SerializeField] private int _minsToAutoSave = 5 ;    // mins until Auto Save is invoked
#endregion


#region MonoBehaviour
  private void Awake()
  {
    if( _instance != null )
    {
      throw new Exception( "Program tried to implement a new instance of SaveManager, but one already existed" ) ;
    }
    _instance = this;
    if ( !Directory.Exists( UserProfilesPath ) ) Directory.CreateDirectory( UserProfilesPath ) ;
    LoadUserProfile() ;
  }
  private string GetProfilePath( string userName ) => Path.Combine( UserProfilesPath, userName ) ;
  private string GetSlotPath( int slotIndex ) => Path.Combine( UserProfilesPath, _activeUserProfile.UserName, "SaveDatas" , $"Slot_{slotIndex}" ) ;
  private string GetUserName()
  {
    if( _activeUserProfile == null || string.IsNullOrEmpty( _activeUserProfile.UserName)) return "DefaultUser" ;
    return _activeUserProfile.UserName ;
  }
  public void InitializeUser( string inputName )
  {
    string userName = string.IsNullOrEmpty( inputName ) ? "User_" + UnityEngine.Random.Range(100, 999) : inputName ;
    string profilePath = Path.Combine( UserProfilesPath, userName ) ;
    if( !Directory.Exists( profilePath ) ) Directory.CreateDirectory( profilePath ) ;
    StartCoroutine( RestApi.AddSession( DateTime.Now, DateTime.Now, (newClientId) =>
    {
      _activeUserProfile = new UserProfile() ;
      _activeUserProfile.UserName = userName ;
      _activeUserProfile.UserId = newClientId ;
      for ( int i = 0 ; i < _maxSaveSlots ; i++ )
      { InitializeSaveSlot(profilePath, i ) ; }
     
      SaveProfileData() ;
      SaveProfileConfig() ;
    })) ;      
  }
  private void InitializeSaveSlot(string profilePath, int slotIndex)
  {
    _activeUserProfile.UserSaveDatas.Add( new UserProfile.UserSaveData 
    {
      SessionId = -1,  // UninitializedSession
      statistics = new UserProfile.UserSaveStatistics() 
    }) ;
    Directory.CreateDirectory( Path.Combine( profilePath, "SaveDatas" , $"Slot_{slotIndex}" ) ) ;
    Directory.CreateDirectory( Path.Combine( profilePath, "SaveDatas" , $"Slot_{slotIndex}" , "SaveFiles" ) ) ;
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
      }));
    }
  }
#endregion

/// <summary>
/// Called by the Save Button, Serializes the Data of the active User profile and passes it on to the API to request a response for Data storage, either updating or creating a new save file
/// </summary>
#region Save
  public void SaveProfileData()
  {
    string json = SerializeData( _activeUserProfile ) ;
    byte[] encryptedData = SaveSystem.Encrypt(json) ;
    string path = Path.Combine( GetProfilePath( _activeUserProfile.UserName ), "ProfileData.dat" ) ;
    File.WriteAllBytes(path, encryptedData) ;
    
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }
  private void SaveProfileConfig()
  {
    string json = SerializeData( _activeUserProfile.Config ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetProfilePath( _activeUserProfile.UserName ), "ProfileConfig.dat" ) ;
    File.WriteAllBytes( path, encryptedData ) ;

    OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;
  }
  private void SaveData()
  {
    string json = SerializeData( _activeUserProfile.UserSaveDatas[_activeSlotIndex] ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetSlotPath( _activeSlotIndex ), $"Save_{_activeSlotIndex}" ) ;
    File.WriteAllBytes( path , encryptedData ) ;
    OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;
  }
  private void SaveDataStatistics( int sessionId )
  {
    if( _activeSlotIndex < 0 ) return ;
    string json = SerializeData( _activeUserProfile.UserSaveDatas[_activeSlotIndex].statistics ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetSlotPath( _activeSlotIndex ) , "Statistics.dat" ) ;
    File.WriteAllBytes( path , encryptedData ) ;
  }
  private void SaveUserDataFile()
  {
    throw new NotImplementedException();
  }
  
  private IEnumerator AutoSave()
  {
    while (true)
    {
      yield return new WaitForSecondsRealtime( _minsToAutoSave * 60 ) ;
      Debug.Log( $"AutoSave initiated." ) ;
    }
  }

#endregion


#region Load
  private void LoadUserProfile()
  {
    // Check if the user is already initialized in the system
    if ( _activeUserProfile == null ) InitializeUser(null) ; // null for now until setup is implemented
    // If it is just load all the save data of the profile
    // if it isn't, create a new user
    // 
  }
  public void LoadUserProfile( string profilePath )
  {
    GetProfilePath(profilePath) ;
    if ( string.IsNullOrEmpty( profilePath ) ) GetProfilePath( _activeUserProfile.UserName ) ;
    byte[] cipherData =  ;
    string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
    DeserializeData( decryptedJson ) ;
   
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }

//TODO Rewrite this Method
/// <summary>
/// Automatically save the Profile, not implemented yet
/// </summary>
#endregion


#region Delete
  public void DeleteProfile( int sessionId )
  {

    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
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
  public void SyncStatistics()
  {
    if ( _activeSlotIndex >= 0 ) return ;
    var activeData = _activeUserProfile.UserSaveDatas[_activeSlotIndex] ;
    StartCoroutine( RestApi.UpdateSession(
      activeData.SessionId,
      activeData.statistics.TimeStartedAt,
      DateTime.Now,
      ( success ) =>
      {
        if ( success ) Debug.Log( "$Session {activeData.SessionId} stats successfully synced" ) ;
        else Debug.Log( "Sync failed" ) ;
      }
    )) ;
  }

#endregion


#region De-/Serialization
private string SerializeData( object obj ) { return JsonUtility.ToJson( obj ) ; }
private void DeserializeData( string jsonData ) { _activeUserProfile = JsonUtility.FromJson<UserProfile>( jsonData ) ; }
#endregion
}