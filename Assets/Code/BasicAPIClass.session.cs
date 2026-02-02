using UnityEngine;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Serialization.Json; // Assuming we will need it. i put this here
using Unity.Properties;
using Unity.Serialization;
using System.Collections;
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
//SessionRowData
  [Serializable]
  class SessionRowData
  {
    public int id ;
    public string started_at ;  // JSON can not read DateTime
    public string ended_at ;
    public string recorded_at ;
  }
// ContextKey
  [Serializable]
  class ContextKey
  {
    public string of ;
    public string[] forT ;
  }
// ResponseContext
  [Serializable]
  class ResponseContext <TKey>
  {
    public TKey keys ;
  }
// SessionContextKeys
  [Serializable]
  class SessionContextKeys
  {
    public ContextKey id ;
  }
// GetSessionResponse
  [Serializable]
  class GetSessionResponse : RequestResponse
  {
    public bool cacheable ;
    public string[] custody_chain ;
    public SessionRowData[] data;
    public ResponseContext<SessionContextKeys> context;
    public string _meta ;
  }


  public void TestGetSession()
  {
    StartCoroutine( GetSession( -1, (res) => {
      int entriesAdded = 0 ;
      int entriesUpdated = 0 ;
      foreach( SessionRowData data in res.data )
      {
        if( CachedSessionData.TryGetValue( data.id, out SessionRowData cachedData ) )
        {
          if( true /* && data.recorded_at > cachedData.recorded_at */ )
          {
            // replace entry with updated data from server
            CachedSessionData[data.id] = data ;
            entriesUpdated++ ;
          }
        }
        else
        {
          // create a new entry using server data
          CachedSessionData.Add( data.id, data ) ;
          entriesAdded++;
        }
      }
      Debug.Log( $"Request successful! Entries added: {entriesAdded} ; Entries updated: {entriesUpdated} ; Total entries: {CachedSessionData.Count}" ) ;
    } ) ) ;
  }

  IEnumerator GetSession( int id, Action<GetSessionResponse> onResult )
  {
    string uri = $"http://{apiHost}:{apiPort}{API_PATH_SESSION}{(id < 0 ? string.Empty : $"/{id}")}";

    GetSessionResponse parsedResponse = null ;

    yield return StartCoroutine( GetRequest( uri, (text) => { parsedResponse = JsonUtility.FromJson<GetSessionResponse>(text) ; } ) ) ;

    if (parsedResponse == null || !parsedResponse.ok)
    {
#if UNITY_EDITOR
      Debug.LogError($"GetSession error: {(parsedResponse != null ? parsedResponse.error : "Invalid JSON")}");
#endif
      yield break ;
    }

    onResult?.Invoke( parsedResponse ) ;
  }
}