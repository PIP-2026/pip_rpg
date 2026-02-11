using System;
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
/// </summary>


public class UserProfile
{
  public int UserId ;
  public int CurrentSessionId ;
  public string UserName ;

  
#region Serializable
  public struct UserProfileData
  {
    public int userId ;   // Either clientId or SessionId to identify corresponding Database entry
    public string userName ;    // Neat to know
    public UserSaveStatistics statistics ;
    public UserProfileConfiguration config ;
    public UserSaveData SaveData ;
  }

  /// <summary>
  /// The Settings hold Interface information, e.g. Volume Settings
  /// </summary>
  public struct UserProfileConfiguration
  {
    public float MasterVolume;
    public float MusicVolume;
    public float SfxVolume;
    public bool Muted;
  }

  /// <summary>
  ///   DataPackage to transfer statistics between Application and Database
  /// </summary>
  public struct UserSaveStatistics
  {
    // Time
    public float TimeStartedAt ;
    public float TimeEndedAt ;
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
  public struct UserSaveData
  {
    public int PlayerPosition_x ;
    public int PlayerPosition_y ;
    public int ActiveSceneIndex ;

  }
#endregion
}