
using System;
using UnityEngine;

[Serializable]
public class DebugMessage
{
  [SerializeField] public bool isActive ;
  [SerializeField] public string message ;

  public bool TryInvoke(bool force = false)
  {
#if UNITY_EDITOR
    if( isActive )
      Debug.Log( message ) ;
#endif
    if( !isActive && force )
      Debug.Log( message ) ;
    return isActive ;
  }

  public bool TryInvoke(string value, bool force = false)
  {
#if UNITY_EDITOR
    if( isActive )
      Debug.Log( $"{message}::{value}" ) ;
#endif
    if( !isActive && force )
      Debug.Log( $"{message}::{value}" ) ;
    return isActive ;
  }
}