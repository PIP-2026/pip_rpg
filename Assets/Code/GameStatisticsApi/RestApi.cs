using UnityEngine;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Serialization.Json; // Assuming we will need it. i put this here
using Unity.Properties;
using Unity.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameStatisticsApi.ResponseData;
using UnityEditorInternal;

namespace GameStatisticsApi 
{
  /// <remarks>
  ///  <para>
  ///   Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
  ///  </para>
  ///   <para>
  ///   Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/22">link to issue</a>
  ///  </para>
  ///  <para>
  ///   Usage/meta information for teammates.
  ///   Without <a href="https://github.com/PIP-2026/pip_rpg/issues/18"> Issue #18 finished yet, i can not feasibly finalize this feature.
  ///   Namespaces will need cleanup upon completion.
  ///  </para>
  /// </remarks>
  /// <summary>
  ///   Basic API Class that can perform calls to the server.
  ///   The class should be capable of deserializing making API calls to the the Basic Response Server and deserialize the dummy data produced.
  ///   The class should recognize HTTP error codes and handle them appropriately. In this case this means logging them.
  /// </summary>
  public partial class RestApi : MonoBehaviour
  {
#region 
    private static RestApi _instance ;
#endregion

#region Unity Editor
    [SerializeField] private string apiHost = "127.0.0.1" ;
    [SerializeField] private string apiPort = "4141" ;

    [Header("Endpoints")]
    [SerializeField] private ApiPath session ;
#endregion


#region Properties
    public string Host => apiHost ;
    public string Port => apiPort ;
#endregion


#region Cache
    internal Dictionary<int,SessionRowData> CachedSessionData { get ; } = new () ;
#endregion


#region Static Properties
    public static ApiPath Session => _instance.session ;
    public static ApiPath Input => throw new NotImplementedException() ;
    public static ApiPath Interaction => throw new NotImplementedException() ;
    public static ApiPath Time => throw new NotImplementedException() ;
#endregion 


#region MonoBehavior
    private void Awake()
    {
      if( _instance != null )
        throw new Exception("Program attempted to create an instance of EventSystem, but one already existed.") ;
      _instance = this ;
    }
#endregion
  }
}