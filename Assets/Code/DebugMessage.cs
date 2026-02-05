
using System;
using UnityEngine;

/// <remarks>
///  <para>
///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
///  </para>
///  <para>
///   Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/37">link to issue</a>
///  </para>
///  <para>
///   Messages will only be logged within the Unity Editor runtime. If necessary
///   for some reason, exceptions to this rule may be made at a later date.
///  </para>
/// </remarks>
/// <summary>
///   Objects of this class are configurable in the Unity Editor and Invokable
///   in their local environment for the purposes of more orderly log messages
///   for any occasions.
/// </summary>
[Serializable]
public class DebugMessage
{
  [Tooltip("Whether or not the message should be logged to the console in Unity Editor runtime.")]
  [SerializeField] private bool isActive = true ;

  [Tooltip("A descriptive message about the observable event where it is invoked.")]
  [SerializeField] private string message ;

  /// <summary>
  ///   Try to invoke the message as-is.
  /// </summary>
  /// <returns>true if the message was actually invoked</returns>
  public bool TryInvoke(bool force = false)
  {
#if UNITY_EDITOR
    if( isActive || force )
    {
      Debug.Log( message ) ;
      return true ;
    }
#endif
    return false ;
  }

  /// <summary>
  ///   Try to invoke the message with an added comment.
  /// </summary>
  /// <returns>true if the message was actually invoked</returns>
  public bool TryInvoke(string value, bool force = false)
  {
#if UNITY_EDITOR
    if( isActive || force )
    {
      Debug.Log( $"{message}::{value}" ) ;
      return true ;
    }
#endif
    return false ;
  }
}