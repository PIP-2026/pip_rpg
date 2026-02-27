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
#region Internal
  private static SaveManager _instance ;
  private UserProfile _activeUserProfile ;
  public UserProfile ActiveUserProfile => _activeUserProfile ;
  private UserSaveData _activeSessionData;
  public static UserSaveData SelectedSaveData ;
  private int _activeSlotIndex = -1 ; // assume that now slot is currently loaded
  private bool _isInitializingSession = false ;
  // Directories
  private const string RootFolderName = "UserProfiles" ;
  private string UserProfilesPath => Path.Combine( Application.persistentDataPath , RootFolderName ) ;
#endregion

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
  public IEnumerator InitializeUser( string inputName )
  {
    string userName = string.IsNullOrEmpty( inputName ) ? "User_" + UnityEngine.Random.Range(100, 999) : inputName ;
    string profilePath = Path.Combine( UserProfilesPath, userName ) ;
    if( !Directory.Exists( profilePath ) ) Directory.CreateDirectory( profilePath ) ;
    yield return StartCoroutine( RestApi.AddSession( DateTime.Now, DateTime.Now, (newClientId) =>
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
  private IEnumerator InitializeActiveSave(int slotIndex)
  {
    if (_isInitializingSession) yield break ;
    _isInitializingSession = true ;
    if (_activeSessionData == null)
    {
      _activeSessionData = new UserSaveData 
      { 
        SlotIndex = slotIndex, 
        SessionId = -1, 
        statistics = new UserSaveStatistics {
          TimeStartedAt = DateTime.Now,
        } 
      };
      _activeUserProfile.UserSaveDatas.Add(_activeSessionData);
    }
    yield return StartCoroutine(RestApi.AddSession(DateTime.Now, DateTime.Now, (newId) => 
    {
      _activeSessionData.SessionId = newId;
      _isInitializingSession = false;
      Debug.Log($"Session {newId} linked to activeSessionData.");
    }));
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

    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile) ;
  }
  public void SaveProfileConfig()
  {
    string json = SerializeData( _activeUserProfile.Config ) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetProfilePath( _activeUserProfile.UserName ), "ProfileConfig.dat" ) ;
    File.WriteAllBytes( path, encryptedData ) ;
      Debug.Log($"Config saved") ;
    OurEventSystem.ProfileEdited.Invoke( _activeUserProfile ) ;
  }
  public void SaveData() => StartCoroutine(SaveDataCoroutine());
  public IEnumerator SaveDataCoroutine()
  {
    if (_activeSlotIndex < 0) yield break ;
    if (_activeSessionData == null || _activeSessionData.SessionId <= 0)
    {
      yield return StartCoroutine(InitializeActiveSave(_activeSlotIndex));
      while (_isInitializingSession) yield return null; 
    }
    yield return StartCoroutine(SaveUserDataFile(false)) ;
    yield return StartCoroutine(SaveDataStatistics() );
    SaveProfileData() ;
  }
  private IEnumerator SaveDataStatistics()
  {
    if( _activeSlotIndex < 0 ) yield break ;
    string json = SerializeData(_activeSessionData.statistics ) ;
    yield return StartCoroutine(SyncStatistics_ToServer()) ;
    byte[] encryptedData = SaveSystem.Encrypt( json ) ;
    string path = Path.Combine( GetSlotPath( _activeSlotIndex ) , "Statistics.dat" ) ;
    File.WriteAllBytes( path , encryptedData ) ;
    Debug.Log($"Statistics saved to Slot: {_activeSlotIndex}");
  }
  private IEnumerator SaveUserDataFile( bool isAutoSave )
  {
    if( _activeSessionData == null ) yield break ;
    _activeSessionData.statistics.TimeEndedAt = DateTime.Now;
    _activeSessionData.statistics.TimeLastCache = DateTime.Now;

    string json = SerializeData(_activeSessionData);
    UserSaveData newData = DeserializeData<UserSaveData>(json);
    
    newData.SessionId = _activeSessionData.SessionId;
    string prefix = isAutoSave ? "AutoSave_" : "ManualSave_" ;
    string fileName = $"{prefix}{DateTime.Now:yyyyMMdd_HHmmss}.dat" ;
    newData.FileName = fileName;
    newData.SlotIndex = _activeSlotIndex;

    _activeUserProfile.UserSaveDatas.Add(newData) ;

    string filesDir = Path.Combine( GetSlotPath(_activeSlotIndex) , "SaveFiles" ) ;
    var garbageFiles = _activeUserProfile.UserSaveDatas
      .Where(d=> d.SlotIndex == _activeSlotIndex && d.SessionId == -1)
      .ToList();
    foreach (var garbage in garbageFiles)
    {
      string gPath = Path.Combine(filesDir, garbage.FileName);
      if (File.Exists(gPath)) File.Delete(gPath);
      _activeUserProfile.UserSaveDatas.Remove(garbage);
    }

    if( !Directory.Exists(filesDir) ) Directory.CreateDirectory( filesDir ) ;
    byte[] encryptedData = SaveSystem.Encrypt( SerializeData( newData ) ) ;
    File.WriteAllBytes( Path.Combine( filesDir , fileName ) , encryptedData ) ;

    var existingFiles = _activeUserProfile.UserSaveDatas
      .Where(d => d.SlotIndex == _activeSlotIndex && !string.IsNullOrEmpty(d.FileName))
      .OrderBy(d => d.statistics.TimeStartedAt)
      .ToList();

    if( existingFiles.Count > _maxSaveFiles )
    {
      var fileToDelete = existingFiles[0];
      DeleteSaveDataFile(fileToDelete);
      Debug.Log($"Overwriting expensive Folder");
    }
    Debug.Log($"File {fileName} saved. Slot {_activeSlotIndex} now has " + 
              _activeUserProfile.UserSaveDatas.Count(d => d.SlotIndex == _activeSlotIndex) + " files.");

    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile) ;
    yield return null ;
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
    if (File.Exists(path))
    {
      byte[] cipherData =  File.ReadAllBytes( path ) ;
      string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
      _activeUserProfile = DeserializeData<UserProfile>( decryptedJson ) ;
      LoadProfileConfiguration( targetUser ) ;
      for (int i = 0; i < _maxSaveSlots; i++)
      {
        LoadSlotData(i);
      }
    } else StartCoroutine(InitializeUser( userName ) );

    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile) ;
  }

  private void LoadProfileConfiguration( string userName )
  {
    string path = Path.Combine( GetProfilePath( userName ) , "ProfileConfig.dat" ) ;
    if ( !File.Exists( path ) ) return ;
    byte[] cipherData =  File.ReadAllBytes( path ) ;
    string decryptedJson = SaveSystem.Decrypt( cipherData ) ;
    _activeUserProfile.Config = DeserializeData<UserProfileConfiguration>( decryptedJson ) ;

  }
  public void LoadSlotData(int slotIndex)
  {
    if (slotIndex < 0) slotIndex = _activeSlotIndex ;
    string filesDir = Path.Combine( GetSlotPath( slotIndex ) , "SaveFiles" ) ;
    if ( !Directory.Exists( filesDir ) ) return ;
    var allFiles = Directory.GetFiles( filesDir, "*dat" )
      .OrderByDescending( f => File.GetLastWriteTime(f) )
      .ToList() ;
    foreach (string fullPath in allFiles) LoadSaveData(slotIndex, Path.GetFileName(fullPath), false);
  }
  public void LoadSelectedData() => LoadSaveData(SelectedSaveData.SlotIndex, SelectedSaveData.FileName, true);
  public void LoadSaveData(int slotIndex, string fileName, bool setAsActive)
  {
    string filesDir = Path.Combine(GetSlotPath(slotIndex), "SaveFiles");
    string fullPath = Path.Combine(filesDir, fileName);
    if (!File.Exists(fullPath)) return;
    _activeSlotIndex = slotIndex;

    byte[] cipherData = File.ReadAllBytes(fullPath);
    string decryptedJson = SaveSystem.Decrypt(cipherData);
    UserSaveData loadedData = DeserializeData<UserSaveData>(decryptedJson);

    if (setAsActive)
    {
      loadedData.FileName = string.Empty;
      loadedData.SlotIndex = slotIndex;
      _activeSessionData = loadedData;

      var placeholder = _activeUserProfile.UserSaveDatas
        .FirstOrDefault(d => d.SlotIndex == slotIndex && string.IsNullOrEmpty(d.FileName));

      if (placeholder != null)
      {
        int index = _activeUserProfile.UserSaveDatas.IndexOf(placeholder);
        _activeUserProfile.UserSaveDatas[index] = _activeSessionData;
      }
      else _activeUserProfile.UserSaveDatas.Add(_activeSessionData);

      Debug.Log($"Active Session restored for slot {slotIndex}. SessionID: {loadedData.SessionId}");
    }
    else
    {
      loadedData.FileName = fileName;
      loadedData.SlotIndex = slotIndex;
      _activeUserProfile.UserSaveDatas.Add(loadedData);
    }

    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile);
  }
#endregion


#region Delete
  public void DeleteProfile( string inputName )
  {

    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile) ;
  }
  public void DeleteSlot() => StartCoroutine(DeleteSlotCoroutine()) ;
  public IEnumerator DeleteSlotCoroutine()
  {
    yield return StartCoroutine( RestApi.DeleteSession(
      _activeUserProfile.UserSaveDatas[_activeSlotIndex].SessionId, 
      (success) =>
      {
        if(success) Debug.Log($"Session deleted") ;
        else Debug.LogError("Lets leave this alone for now") ; 
      })) ; 
    if (Directory.Exists(GetSlotPath(_activeSlotIndex))) Directory.Delete(GetSlotPath(_activeSlotIndex), true);
    _activeUserProfile.UserSaveDatas.RemoveAll(d => d.SlotIndex == _activeSlotIndex);
    InitializeSaveSlot(GetProfilePath(_activeUserProfile.UserName) , _activeSlotIndex) ;
    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile);
  }
  public void DeleteSelectedFile() => DeleteSaveDataFile(SelectedSaveData);
  public void DeleteSaveDataFile(UserSaveData dataToDelete)
  {
    if (dataToDelete == null || string.IsNullOrEmpty(dataToDelete.FileName)) return;

    File.Delete(Path.Combine(GetSlotPath(dataToDelete.SlotIndex), "SaveFiles", dataToDelete.FileName));
    _activeUserProfile.UserSaveDatas.Remove(dataToDelete);
    SelectedSaveData = null ;
    OurEventSystem.ProfileEdited.Invoke(_activeUserProfile);
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
  public IEnumerator SyncStatistics_ToServer()
  {
    if (_activeSessionData == null || _activeSessionData.SessionId <= 0) yield break; 

    yield return StartCoroutine( RestApi.UpdateSession(
      _activeSessionData.SessionId,
      _activeSessionData.statistics.TimeStartedAt,
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