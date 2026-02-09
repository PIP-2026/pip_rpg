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
using System.Text;

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
    [SerializeField] private ApiPath input ;
    [SerializeField] private ApiPath interaction ;
    [SerializeField] private ApiPath time ;
#endregion


#region Static Properties
    public static Endpoints Endpoints = new() ;
    public static string Host => _instance.apiHost ;
    public static string Port => _instance.apiPort ;
#endregion


#region TEST
    public void OnClickSave()
    {
      byte[] rawData = Encoding.UTF8.GetBytes(
        JsonUtility.ToJson(new PostSessionData() {
          started_at = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble)).ToString(),
          ended_at = DateTime.Now.ToString() } )
      ) ;
      StartCoroutine( (Endpoints.Session as SessionPath).Post( rawData ) );
    }
    public void OnClickLoad()
    {
      StartCoroutine( (Endpoints.Session as SessionPath).Get() ) ;
    }
    public void OnClickUpdate()
    {
      byte[] rawData = Encoding.UTF8.GetBytes(
        JsonUtility.ToJson(new PostSessionData() {
          started_at = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble)).ToString(),
          ended_at = DateTime.Now.ToString() } )
      ) ;
      StartCoroutine( (Endpoints.Session as SessionPath).Put(3,rawData) ) ;
    }
    public void OnClickDelete()
    {
      StartCoroutine( (Endpoints.Session as SessionPath).Delete(3) ) ;
    }
#endregion 


#region MonoBehavior
    private void Awake()
    {
      if( _instance != null )
        throw new Exception("Program attempted to create an instance of RestApi, but one already existed.") ;
      
      _instance = this ;

      Endpoints.Session     = _instance.session ;
      Endpoints.Input       = _instance.input ;
      Endpoints.Interaction = _instance.interaction ;
      Endpoints.Time        = _instance.time ;
    }
#endregion
  }

#region Endpoints
  public class Endpoints
  {
    public ApiPath Session { get ; internal set ; }
    public ApiPath Input { get ; internal set ; }
    public ApiPath Interaction { get ; internal set ; }
    public ApiPath Time { get ; internal set ; }
  }
#endregion 
}

