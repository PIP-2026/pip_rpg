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
using Unity.VisualScripting;

namespace GameStatisticsApi 
{
  /// <remarks>
  ///  <para>
  ///   Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
  ///  </para>
  ///   <para>
  ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
  ///   </para>
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
  ///   The class should be capable of making API calls to the the Basic Response Server and deserialize the dummy data produced.
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
    [SerializeField] private ApiPath time ;
#endregion


#region Static Properties
    internal static Endpoints Endpoints = new() ;
    internal static string Host => _instance.apiHost ;
    internal static string Port => _instance.apiPort ;
#endregion


#region TEST
    public void OnClickSave()
    {
      byte[] rawData = Encoding.UTF8.GetBytes(
        JsonUtility.ToJson(new PostOrPutSessionData() {
          started_at = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble)).ToString(),
          ended_at = DateTime.Now.ToString() } )
      ) ;
      StartCoroutine( (Endpoints.Session as SessionPath).Post( rawData ) ) ;
    }
#endregion 


#region Actions: Session
    public IEnumerator GetSessions(Action<(int,DateTime,DateTime,DateTime)[]> action)
    {
      yield return StartCoroutine( (Endpoints.Session as SessionPath).Get() ) ;
      action?.Invoke( default ) ;
    }
    public IEnumerator GetSession(int sessionId,Action<(int,DateTime,DateTime,DateTime)> action)
    {
      yield return StartCoroutine( (Endpoints.Session as SessionPath).Get(sessionId) ) ;
      action?.Invoke( default ) ;
    }
    public IEnumerator AddSession(DateTime started_at, DateTime ended_at, Action<int> action)
    {
      yield return StartCoroutine( (Endpoints.Session as SessionPath).Post(null /* datetime data */ ) ) ;
      action?.Invoke( default ) ;
    }
    public IEnumerator UpdateSession(int sessionId, DateTime started_at, DateTime ended_at, Action<bool> action)
    {
      yield return StartCoroutine( (Endpoints.Session as SessionPath).Put(sessionId, null /* datetime data */ ) ) ;
      action?.Invoke( default ) ;
    }
    public IEnumerator DeleteSession(int sessionId, Action<bool> action)
    {
      yield return StartCoroutine( (Endpoints.Session as SessionPath).Delete(sessionId) ) ;
      action?.Invoke( default ) ;
    }
#endregion 


#region Actions: Input
    public void GetInput(int sessionId)
    {
      StartCoroutine( (Endpoints.Input as InputPath).Get(sessionId) ) ;
    }
    public void AddInput(DateTime started_at, DateTime ended_at)
    {
      StartCoroutine( (Endpoints.Input as InputPath).Post(null /* datetime data */ ) ) ;
    }
    public void UpdateInput(int sessionId, DateTime started_at, DateTime ended_at)
    {
      StartCoroutine( (Endpoints.Input as InputPath).Put(sessionId, null /* datetime data */ ) ) ;
    }
    public void DeleteInput(int sessionId)
    {
      StartCoroutine( (Endpoints.Input as InputPath).Delete(sessionId) ) ;
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
      Endpoints.Time        = _instance.time ;
    }
#endregion
  }

#region Endpoints
  internal class Endpoints
  {
    public ApiPath Session { get ; internal set ; }
    public ApiPath Input { get ; internal set ; }
    public ApiPath Time { get ; internal set ; }
  }
#endregion 
}

