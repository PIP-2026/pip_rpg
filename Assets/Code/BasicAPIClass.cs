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
public partial class BasicAPIClass : MonoBehaviour
{
#region Unity Editor
  [SerializeField] private string apiHost = "127.0.0.1" ;
  [SerializeField] private string apiPort = "4141" ;
#endregion


#region Api Endpoints
 private const string API_PATH_SESSION = "/statistics/session" ;
#endregion


#region Cache
  private Dictionary<int,SessionRowData> CachedSessionData { get ; } = new () ;
#endregion


  IEnumerator GetRequest( string uri, Action<string> onResult )
  {
    UnityWebRequest webRequest = UnityWebRequest.Get( uri ) ;
#if UNITY_EDITOR
    Debug.Log( $"Dispatching a GET request to \"{uri}\"." ) ;
#endif
    yield return webRequest.SendWebRequest() ;

    if( webRequest.error != null )
    {
#if UNITY_EDITOR
      Debug.Log( $"Web request encountered an error: {webRequest.error}" ) ;
#endif
      yield break ;
    } else {
#if UNITY_EDITOR
      Debug.Log( $"Received: {webRequest.downloadHandler.text}" ) ;
#endif
    }

    onResult?.Invoke( webRequest.downloadHandler.text ) ;
  }


#region Response Parent Class
  class RequestResponse
  {
    public bool ok ;
    public string error ;
  }
#endregion
}