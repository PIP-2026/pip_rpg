using UnityEngine;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Serialization.Json; // Assuming we will need it. i put this here
using Unity.Properties;
using Unity.Serialization;
using System.Collections;
using System.Diagnostics;
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

public class BasicAPIClass : MonoBehaviour
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
  class GetSessionResponse
  {
    public bool ok ;
    public bool cacheable ;
    public string[] custody_chain ;
    public SessionRowData[] rows;
    public ResponseContext<SessionContextKeys> context;
    public string _meta ;
    public string error ;
  }

// Dormant for the reason of waiting
// public static class SessionReader 
// {
//   public static DateTime GetDate(string rawDate) => DateTime.Parse(rawDate) ;
//   public static void LogSummary(GetSessionResponse response)
//   {
//     if (response.rows == null) return ;
//     foreach(var row in response.rows)
//     {
//       Debug.Log($"[Session {row.id}] Started: {GetDate(row.started_at):g}") ;
//     }
//   }
// }

// Dormant for the reason of waiting
//  public async Task GetSessions(Action<GetSessionResponse> onResult)
//  {
//    // Fictive endpoint, change this to actual endpoint after Issue #18
//    string endpoint = "replace me with the actual endpoint";
//    using (UnityWebRequest request = UnityWebRequest.Get(EndPoint))
//    {
//      request.SetRequestHeader("X-Unity-Client", Application.version);
//      var op = request.SendWebRequest();
//      while(!op.isDone) await Task.Yield();
//      // Call Response Server method to process API requests.
//    }
//
//  }
  
}