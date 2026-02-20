using System;
using System.Collections.Generic;
using UnityEngine;

/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///     Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/33">link to issue</a>
///   </para>
///   <para>
///    This class is used for the initialization and communication between API and SaveManager for the bookkeeping of the User.
///   </para>
/// </remarks>
/// <summary>
///   It contains the structs we deemed useful to be able to pass on
///   A profile store the user Id it gets after initialization by requesting an id from the api
///   The profile is stored locally and the statistics can be up and downloaded to and from the Database
/// </summary>

[Serializable]
public class UserProfile
{
  public int UserId ;
  public string UserName ;
  public UserProfileConfiguration Config ;
  public List<UserSaveData> UserSaveDatas = new() ;
}
[Serializable]
public class UserProfileData
{
  public int UserId ;   // Either clientId or SessionId to identify corresponding Database entry
  public string UserName ; 
  public UserProfileConfiguration Config ;
  public List<UserSaveData> UserSaveDatas ;
}

/// <summary>
/// The Settings hold Interface information, e.g. Volume Settings
/// </summary>
[Serializable]
public class UserProfileConfiguration
{
  public float MasterVolume;
  public float MusicVolume;
  public float SfxVolume;
  public bool Muted;
}

/// <summary>
///   DataPackage to transfer statistics between Application and Database
/// </summary>
[Serializable]
public class UserSaveStatistics
{
  // Time
  public DateTime TimeStartedAt ;
  public DateTime TimeEndedAt ;
  public DateTime TimeLastCache ;
  public float TimeInMenu ;
  public float TimeInExploration ;
  public float TimeTotal ;

  // Actions
  public int TilesMoved ;
  public int ButtonsPressed ;
  public int InteractionsInitiated ;
  public int DialogueLinesSkipped ;
}

  /// <summary>
  ///   Is storing Application relevant information, to safely store/load current status in application, not statistics
  /// </summary>
  
[Serializable]
public class UserSaveData
{
  public int SessionId ;
  public int PlayerPosition_x ;
  public int PlayerPosition_y ;
  public int ActiveSceneIndex ;
  public UserSaveStatistics statistics ;
}