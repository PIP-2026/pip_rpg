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
  ///  <para>
  ///    Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
  ///  </para>
  ///  <para>
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
    [SerializeField] private string apiRoot = "/statistics" ;

    [Header("Endpoints")]
    [SerializeField] private ApiEndpoint session ;
    [SerializeField] private ApiEndpoint input ;
    [SerializeField] private ApiEndpoint time ;
#endregion


#region Static Properties
    internal static string Host => _instance.apiHost ;
    internal static string Port => _instance.apiPort ;
    internal static string Root => _instance.apiRoot ;
#endregion


#region Actions: Session
    public IEnumerator GetSessions(Action<(int,DateTime,DateTime,DateTime)[]> action, DateTime cache = default )
    {
      List<(int,DateTime,DateTime,DateTime)> values = new () ;

      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.session.GetAll.Call(
        data,
        (text) => {
          GetSessionResponse res = JsonUtility.FromJson<GetSessionResponse>(text) ;
          foreach( SessionRowData data in res.data )
          {
            values.Add( (data.id, DateTime.Parse(data.started_at), DateTime.Parse(data.ended_at), DateTime.Parse(data.recorded_at)) ) ;
          }
        }
      ) ) ;

      action?.Invoke( values.ToArray() ) ;
    }

    public IEnumerator GetSession(int sessionId,Action<int,DateTime,DateTime,DateTime> action, DateTime cache = default )
    {
      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.session.GetOne.Call(sessionId,
        (text) =>
        {
          SessionRowData data = JsonUtility.FromJson<GetSessionResponse>(text).data[0] ;
          action?.Invoke( data.id, DateTime.Parse(data.started_at), DateTime.Parse(data.ended_at), DateTime.Parse(data.recorded_at) ) ;
        }
      ) ) ;
    }

    public IEnumerator AddSession(DateTime started_at, DateTime ended_at, Action<int> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"started_at\": \"{started_at}\", \"ended_at\": \"{ended_at}\" }}" ) ;

      yield return StartCoroutine( _instance.session.Add.Call( data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutSessionResponse>(text).insert_id ) ; }
      ) ) ;
    }

    public IEnumerator UpdateSession(int sessionId, DateTime started_at, DateTime ended_at, Action<bool> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"started_at\": \"{started_at}\", \"ended_at\": \"{ended_at}\" }}" ) ;

      yield return StartCoroutine( _instance.session.Update.Call( sessionId, data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutSessionResponse>(text).ok ) ; }
      ) ) ;
    }

    public IEnumerator DeleteSession(int sessionId, Action<bool> action)
    {
      yield return StartCoroutine( _instance.session.Delete.Call( sessionId,
        (text) => { action?.Invoke( JsonUtility.FromJson<DeletionResponse>(text).ok ) ; }
      ) ) ;
    }
#endregion 


#region Actions: Input
    public IEnumerator GetInputs(Action<(int,int,int,int,DateTime)[]> action, DateTime cache = default )
    {
      List<(int,int,int,int,DateTime)> values = new () ;

      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.input.GetAll.Call(
        data,
        (text) => {
          GetInputResponse res = JsonUtility.FromJson<GetInputResponse>(text) ;
          foreach( InputRowData data in res.data )
          {
            values.Add( (data.session_id, data.times_buttons_clicked, data.distance_moved, data.etc, DateTime.Parse(data.recorded_at)) ) ;
          }
        }
      ) ) ;

      action?.Invoke( values.ToArray() ) ;
    }

    public IEnumerator GetInput(int sessionId, Action<int,int,int,int,DateTime> action, DateTime cache = default )
    {
      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.input.GetOne.Call(
        sessionId,
        data,
        (text) => {
          InputRowData data = JsonUtility.FromJson<GetInputResponse>(text).data[0] ;
          action?.Invoke( data.session_id, data.times_buttons_clicked, data.distance_moved, data.etc, DateTime.Parse(data.recorded_at) ) ;
        }
      ) ) ;
    }

    public IEnumerator AddInput(int sessionId, int times_buttons_clicked, int distance_moved, int etc, Action<int> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"times_buttons_clicked\": \"{times_buttons_clicked}\", \"distance_moved\": \"{distance_moved}\", \"etc\": \"{etc}\" }}" ) ;

      yield return StartCoroutine( _instance.input.Add.Call(sessionId, data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutInputResponse>(text).insert_id ) ; }
      ) ) ;
    }

    public IEnumerator UpdateInput(int sessionId, int times_buttons_clicked, int distance_moved, int etc, Action<bool> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"times_buttons_clicked\": \"{times_buttons_clicked}\", \"distance_moved\": \"{distance_moved}\", \"etc\": \"{etc}\" }}" ) ;

      yield return StartCoroutine( _instance.input.Update.Call(sessionId, data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutInputResponse>(text).ok ) ; }
      ) ) ;
    }

    public IEnumerator DeleteInput(int sessionId, Action<bool> action)
    {
      yield return StartCoroutine( _instance.input.Delete.Call( sessionId,
        (text) => { action?.Invoke( JsonUtility.FromJson<DeletionResponse>(text).ok ) ; }
      ) ) ;
    }
#endregion 


#region Actions: Input
    public IEnumerator GetTimes(int sessionId, Action<(int,TimeSpan,TimeSpan,TimeSpan,DateTime)[]> action, DateTime cache = default )
    {
      List<(int,TimeSpan,TimeSpan,TimeSpan,DateTime)> values = new () ;

      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.time.GetAll.Call(
        sessionId,
        data,
        (text) => {
          GetTimeResponse res = JsonUtility.FromJson<GetTimeResponse>(text) ;
          foreach( TimeRowData data in res.data )
          {
            values.Add( (
              data.session_id,
              TimeSpan.FromSeconds(data.in_menus),
              TimeSpan.FromSeconds(data.in_exploration),
              TimeSpan.FromSeconds(data.in_dialogue),
              DateTime.Parse(data.recorded_at))
            ) ;
          }
        }
      ) ) ;
      action?.Invoke( values.ToArray() ) ;
    }
   
    public IEnumerator GetTime(int sessionId, Action<int,TimeSpan,TimeSpan,TimeSpan,DateTime> action, DateTime cache = default )
    {
      byte[] data = default ;
      if( cache != default )
        data = Encoding.UTF8.GetBytes( $"{{ \"cache\": \"{cache}\" }}" ) ;

      yield return StartCoroutine( _instance.input.GetOne.Call(
        sessionId,
        data,
        (text) => {
          TimeRowData data = JsonUtility.FromJson<GetTimeResponse>(text).data[0] ;
          action?.Invoke(
            data.session_id,
            TimeSpan.FromSeconds(data.in_menus),
            TimeSpan.FromSeconds(data.in_exploration),
            TimeSpan.FromSeconds(data.in_dialogue),
            DateTime.Parse(data.recorded_at)
          ) ;
        }
      ) ) ;
    }

    public IEnumerator AddTime(int sessionId, TimeSpan in_menus, TimeSpan in_exploration, TimeSpan in_dialogue, Action<int> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"in_menus\": \"{in_menus.TotalMilliseconds/1000}\", \"in_exploration\": \"{in_exploration.TotalMilliseconds/1000}\", \"in_dialogue\": \"{in_dialogue.TotalMilliseconds/1000}\" }}" ) ;

      yield return StartCoroutine( _instance.time.Update.Call(sessionId, data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutTimeResponse>(text).insert_id ) ; }
      ) ) ;
    }

    public IEnumerator UpdateTime(int sessionId, TimeSpan in_menus, TimeSpan in_exploration, TimeSpan in_dialogue, Action<bool> action)
    {
      byte[] data = Encoding.UTF8.GetBytes( $"{{ \"in_menus\": \"{in_menus.TotalMilliseconds/1000}\", \"in_exploration\": \"{in_exploration.TotalMilliseconds/1000}\", \"in_dialogue\": \"{in_dialogue.TotalMilliseconds/1000}\" }}" ) ;

      yield return StartCoroutine( _instance.time.Update.Call(sessionId, data,
        (text) => { action?.Invoke( JsonUtility.FromJson<PostOrPutTimeResponse>(text).ok ) ; }
      ) ) ;
    }

    public IEnumerator DeleteTime(int sessionId, Action<bool> action)
    {
      yield return StartCoroutine( _instance.time.Delete.Call( sessionId,
        (text) => { action?.Invoke( JsonUtility.FromJson<DeletionResponse>(text).ok ) ; }
      ) ) ;
    }
#endregion 


#region Test
    public void OnClickSave()
    {
      StartCoroutine( AddSession( DateTime.Now - TimeSpan.FromSeconds(Time.timeSinceLevelLoadAsDouble), DateTime.Now, (i) => {} ) ) ;
    }
    public void OnClickLoad()
    {
      StartCoroutine( AddInput( -1, 0, 0, 0, (i) => {} ) ) ;
    }
    public void OnClickDelete()
    {
      StartCoroutine( AddTime( 5, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, (i) => {} ) ) ;
    }
#endregion 


#region MonoBehavior
    private void Awake()
    {
      if( _instance != null )
        throw new Exception("Program attempted to create an instance of RestApi, but one already existed.") ;
      
      _instance = this ;
    }
#endregion
  }
}

