using System;


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
  ///   
  /// </summary>


public class UserProfile
{
#region Serializable
  public struct UserProfileData
  {
    public int userId ;   // Either clientId or SessionId to identify corresponding Database entry
    public string userName ;    // Neat to know
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
  /// 
  /// </summary>
  public struct UserSaveData
  {
    //TODO: Consider which things we need in here
  }
#endregion
}