
using System;
using UnityEngine;

[Serializable]
public class DebugMessage
{
  [SerializeField] public bool isActive ;
  [SerializeField] public string message ;

  public bool TryInvoke()
  {
#if UNITY_EDITOR
  if( isActive )
    Debug.Log( message ) ;
#endif
    return isActive ;
  }
}