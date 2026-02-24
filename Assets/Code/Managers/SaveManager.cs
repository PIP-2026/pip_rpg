using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameStatisticsApi;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
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
  private string UserProfilesPath => Path.Combine( Application.persistentDataPath , RootFolderName ) ;


#region UnityEditor
// These fields can be adjusted in the Editor to customize how many Files a user can store and 
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
    LoadUserProfile( GetUserName() ) ;
  }
  /// <summary>
  /// With no input name given, the system will initialize a default user with a random number attached to it.
  /// </summary>
  /// <param name="inputName"></param>
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
  /// <summary>
  /// The profile path is given by the User Name, each User has their own Profile Data, Config and a list of SaveFiles. the Save files are stored within a slot and each slot
  /// holds up to x automatic/manual save files with their own statistics for tracking
  /// </summary>
  /// <param name="profilePath"></param>
  /// <param name="slotIndex"></param>
  private void InitializeSaveSlot(string profilePath, int slotIndex)
  {
    if( string.IsNullOrEmpty( profilePath ) ) profilePath = GetProfilePath( _activeUserProfile.UserName ) ; 
    _activeUserProfile.UserSaveDatas.Add( new UserSaveData 
    {
      SessionId = -1,  // UninitializedSession
      statistics = new UserSaveStatistics() 
    }) ;
    Directory.CreateDirectory( Path.Combine( profilePath, "SaveDatas" , $"Slot_{slotIndex}" ) ) ;
    Directory.CreateDirectory( Path.Combine( profilePath, "SaveDatas" , $"Slot_{slotIndex}" , "SaveFiles" ) ) ;
  }
/// <summary>
/// Here is when a session is actually connected to the Api for the purpose of tracking it.
/// </summary>
/// <param name="slotIndex"></param>
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


  #region HelperMethods
  private string GetProfilePath( string userName ) => Path.Combine( UserProfilesPath, userName ) ;
  private string GetSlotPath( int slotIndex ) => Path.Combine( UserProfilesPath, _activeUserProfile.UserName, "SaveDatas" , $"Slot_{slotIndex}" ) ;
  private string GetUserName()
  {
    if( _activeUserProfile == null || string.IsNullOrEmpty( _activeUserProfile.UserName)) return "DefaultUser" ;
    return _activeUserProfile.UserName ;
  }
  public List<string> GetAvailableSaveFiles( int slotIndex )
  {
    string filesDir = Path.Combine( GetSlotPath( slotIndex ) , "SaveFiles" ) ;
    if (!Directory.Exists( filesDir) ) return new List<string>() ;
    return Directory.GetFiles( filesDir, "*.dat" )
                    .OrderByDescending( f => File.GetLastWriteTime( f ) )
                    .ToList() ;
  }
  public void SelectActiveProfile( string userName )
  {
    throw new NotImplementedException() ;
  }
  public void SelectActiveSaveSlot( int slotIndex )
  {
    _activeSlotIndex = slotIndex ;
    Debug.Log($"Active Slot{_activeSlotIndex} selected") ;
  }
  public void SelectActiveSaveData()
  {
    
  }
  #endregion

/// <summary>
/// Called by the Save Button, Serializes the Data of the active User profile and passes it on to the API to request a response for Data storage, either updating or creating a new save file
/// </summary>
#region Save
//TODO Check the data validation to ensure the correspondence with Api is correctly established. Currently empty data is passed on.
  public void SaveProfileData()
  {
    string json = SerializeData( _activeUserProfile ) ;
    byte[] encryptedData = SaveSystem.Encrypt(json) ;
    string path = Path.Combine( GetProfilePath( _activeUserProfile.UserName ), "ProfileData.dat" ) ;
    File.WriteAllBytes(path, encryptedData) ;
          Debug.Log($"Profile data saved") ;

    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }
  public void SaveProfileConfig()
  {
    string json = SerializeData( _activeUserProfile.Config ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetProfilePath( _activeUserProfile.UserName ), "ProfileConfig.dat" ) ;
    File.WriteAllBytes( path, encryptedData ) ;
      Debug.Log($"Config saved") ;
    OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;
  }
  public void SaveData()
  {
    string json = SerializeData( _activeUserProfile.UserSaveDatas[_activeSlotIndex] ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetSlotPath( _activeSlotIndex ), $"Save_{_activeSlotIndex}" ) ;
    File.WriteAllBytes( path , encryptedData ) ;
    Debug.Log($"Data saved to {_activeSlotIndex}");
    OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;
  }
  public void SaveDataStatistics()
  {
    if( _activeSlotIndex < 0 ) return ;
    string json = SerializeData( _activeUserProfile.UserSaveDatas[_activeSlotIndex].statistics ) ;
    SyncStatistics_ToServer() ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetSlotPath( _activeSlotIndex ) , "Statistics.dat" ) ;
    File.WriteAllBytes( path , encryptedData ) ;
    Debug.Log($"Statistics saved to Slot: {_activeSlotIndex}");
  }
  public void SaveUserDataFile( bool isAutoSave )
  {
    if( _activeSlotIndex < 0 ) return ;
    string filesDir = Path.Combine( GetSlotPath(_activeSlotIndex) , "SaveFiles" ) ;
    if( !Directory.Exists(filesDir) ) Directory.CreateDirectory( filesDir ) ;
    string prefix = isAutoSave ? "AutoSave_" : "ManualSave_" ;
    var existingFiles = Directory.GetFiles( filesDir, prefix + "*.dat" )
                                .OrderBy(f => File.GetCreationTime(f) )
                                .ToList() ;
    if( existingFiles.Count >= _maxSaveFiles )
    {
      File.Delete( existingFiles[0] ) ;
    }
    string fileName = $"{prefix}{DateTime.Now:yyyyMMdd_HHmmss}.dat" ;
    string fullPath = Path.Combine( filesDir , fileName ) ;

    string json = SerializeData( _activeUserProfile.UserSaveDatas[_activeSlotIndex] ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    File.WriteAllBytes( fullPath , encryptedData ) ;
    Debug.Log($"File: {fileName} saved to {fullPath}");
  }
  //TODO AutoSave function
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
  public void LoadUserProfile( string userName )
  {
    string targetUser = string.IsNullOrEmpty(userName) ? GetUserName() : userName ;
    string path = Path.Combine( GetProfilePath( targetUser ) , "ProfileData.dat" ) ;
    if ( !File.Exists(path ))
    {
      InitializeUser( userName ) ;
      return ;
    } 
    byte[] cipherData =  File.ReadAllBytes( path ) ;
    string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
    _activeUserProfile = DeserializeData<UserProfile>( decryptedJson ) ;
    LoadProfileConfiguration( targetUser ) ;
    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }

  private void LoadProfileConfiguration( string userName )
  {
    string path = Path.Combine( GetProfilePath( userName ) , "ProfileConfig.dat" ) ;
    if ( !File.Exists( path ) ) return ;
    byte[] cipherData =  File.ReadAllBytes( path ) ;
    string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
    _activeUserProfile.Config = DeserializeData<UserProfileConfiguration>( decryptedJson ) ;

  }
  public void LoadSlotData( int slotIndex )
  {
    _activeSlotIndex = slotIndex ;
    string filesDir = Path.Combine( GetSlotPath( slotIndex ) , "SaveFiles" ) ;
    if ( !Directory.Exists( filesDir ) ) return ;
    // I consolidated this logic somewhere... i think
    var latestFile = Directory.GetFiles( filesDir, "*dat" )
                              .OrderByDescending( f => File.GetLastWriteTime(f) )
                              .FirstOrDefault() ;
    if ( latestFile != null )
    {
      byte[] cipherData = File.ReadAllBytes( latestFile ) ;
      string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
      _activeUserProfile.UserSaveDatas[slotIndex] = DeserializeData<UserSaveData>( decryptedJson ) ;
      Debug.Log( $"Successfully loaded: { Path.GetFileName( latestFile ) } into Slot { slotIndex }" ) ;

      OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;
    }
  }
  private void LoadSaveData( int slotIndex , string fullPath )
  {
    if ( !File.Exists(fullPath) ) return ;
    _activeSlotIndex = slotIndex ;
    byte[] cipherData = File.ReadAllBytes( fullPath ) ;
    string decryptedJson = SaveSystem.Decrypt ( cipherData ) ;
    _activeUserProfile.UserSaveDatas[slotIndex] = DeserializeData<UserSaveData>( decryptedJson ) ;
    Debug.Log( $"Loaded save data: { Path.GetFileName( fullPath ) } into slot: { slotIndex } " ) ;
    OurEventSystem.profileEdited.Invoke( _activeUserProfile ) ;

  }
#endregion


#region Delete
  public void DeleteProfile( string inputName )
  {

    OurEventSystem.profileEdited.Invoke(_activeUserProfile) ;
  }
  public void DeleteSaveData()
  {
    
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
  public void SyncStatistics_ToServer()
  {
    if ( _activeSlotIndex < 0 ) return ;
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

  public void SyncStatistics_FromServer()
  {
    
  }

#endregion


#region De-/Serialization
  private string SerializeData( object obj ) { return JsonUtility.ToJson( obj ) ; }
  private T DeserializeData<T>(string jsonData) { return JsonUtility.FromJson<T>(jsonData) ; }
#endregion
}