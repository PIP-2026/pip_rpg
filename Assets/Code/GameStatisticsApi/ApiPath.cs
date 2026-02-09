using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GameStatisticsApi 
{
  /// <remarks>
  ///   <para>
  ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
  ///   </para>
  ///   <para>
  ///       Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/22">link to issue</a>
  ///   </para>
  ///   <para>
  ///     The default (virtual) methods of this class should be overridden by methods that perform the default action
  ///     for the specific type of endpoint the child class represents. <c>SessionPath</c> for example, has been given
  ///     a default action that caches the received data.
  ///   </para>
  /// </remarks>
  /// <summary>
  ///   The default implementation of an ApiPath/Endpoint that can be slotted into and thereafter called from the RestApi object.
  /// </summary>
  public class ApiPath : MonoBehaviour
  {
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   The base path of this API endpoint, not including parameters.
    /// </summary>
    /// <value>URL as <c>/</c> segmented string</value>
    [SerializeField] protected string endpointPath = "" ;
    [SerializeField] private DebugMessages debugMessages ;

    public virtual string BaseURI => $"http://{RestApi.Host}:{RestApi.Port}{endpointPath}" ;

#region GET
    public virtual IEnumerator Get( int[] ids, Action<string> onResult )
    {
      StringBuilder uri = new (BaseURI) ;

      foreach( int id in ids ) { if(id == -1) break ; uri.Append('/') ; uri.Append(id) ; }

      yield return StartCoroutine( GetRequest( uri.ToString(), onResult ) );
    }
    public virtual IEnumerator Get( int id, Action<string> onResult ) => Get( new[] {id}, onResult ) ;
    public virtual IEnumerator Get( Action<string> onResult ) => Get( new[] {-1}, onResult ) ;
    
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a GET request to the RestApi.
    /// </summary>
    /// <param name="ids">An array of <c>id</c>s by which to access a specific entry of this endpoint</param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    public virtual IEnumerator Get( int[] ids )
    {
      StringBuilder uri = new (BaseURI) ;

      foreach( int id in ids ) { if(id == -1) break ; uri.Append('/') ; uri.Append(id) ; }
      
      yield return StartCoroutine( GetRequest( uri.ToString(), (s) => {} ) );
    }
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a GET request to the RestApi.
    /// </summary>
    /// <param name="id"><c>id</c> by which to access a specific entry of this endpoint</param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    public virtual IEnumerator Get( int id )  => Get( new[] {id} ) ;
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a GET request to the RestApi.
    /// </summary>
    /// <param name="ids">An array of <c>id</c>s by which to access a specific entry of this endpoint</param>
    /// <param name="id"><c>id</c> by which to access a specific entry of this endpoint</param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    public virtual IEnumerator Get() => Get( new[] {-1} ) ;
#endregion


#region POST
    public virtual IEnumerator Post( int[] ids, byte[] data, Action<string> onResult )
    {
      StringBuilder uri = new (BaseURI) ;

      foreach( int id in ids ) { if(id == -1) break ; uri.Append('/') ; uri.Append(id) ; }

      yield return StartCoroutine( PostRequest( uri.ToString(), data, onResult ) );
    }
    public virtual IEnumerator Post( int[] ids, byte[] data ) => Post( ids, data, (s) => {} ) ;
    public virtual IEnumerator Post( int id, byte[] data, Action<string> onResult ) => Post( new[] {id}, data, onResult ) ;
    public virtual IEnumerator Post( int id, byte[] data ) => Post( new[] {id}, data, (s) => {} ) ;
    public virtual IEnumerator Post( byte[] data, Action<string> onResult ) => Post( new[] {-1}, data, onResult ) ;
    public virtual IEnumerator Post( byte[] data ) => Post( new[] {-1}, data, (s) => {} ) ;
#endregion


#region PUT
    public virtual IEnumerator Put( int[] ids, byte[] data, Action<string> onResult )
    {
      StringBuilder uri = new (BaseURI) ;

      foreach( int id in ids ) { if(id == -1) break ; uri.Append('/') ; uri.Append(id) ; }

      yield return StartCoroutine( PutRequest( uri.ToString(), data, onResult ) );
    }
    public virtual IEnumerator Put( int[] ids, byte[] data) => Put( ids, data, (s) => {} ) ;
    public virtual IEnumerator Put( int id, byte[] data, Action<string> onResult ) => Put( new[] {id}, data, onResult) ;
    public virtual IEnumerator Put( int id, byte[] data) => Put( new[] {id}, data, (s) => {} ) ;
#endregion


#region DELETE
    public virtual IEnumerator Delete( int[] ids, Action<string> onResult )
    {
      StringBuilder uri = new (BaseURI) ;

      foreach( int id in ids ) { if(id == -1) break ; uri.Append('/') ; uri.Append(id) ; }

      yield return StartCoroutine( DeleteRequest( uri.ToString(), onResult ) );
    }
    public virtual IEnumerator Delete( int[] ids) => Delete( ids, (s) => {} ) ;
    public virtual IEnumerator Delete( int id, Action<string> onResult ) => Delete( new[] {id}, onResult ) ;
    public virtual IEnumerator Delete( int id ) => Delete( new[] {id}, (s) => {} ) ;
#endregion


#region GET Request
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.<br/>
    ///     All handled exceptions should be caught here and not further upstream. 
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a GET request to the RestApi.
    /// </summary>
    /// <param name="uri">The identifier to which the request is be sent</param>
    /// <param name="onResult">
    ///   The action which should be performed on the text body received by
    ///   the UnityWebRequest's DownloadHandler.
    /// </param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    protected IEnumerator GetRequest( string uri, Action<string> onResult )
    {
      debugMessages?.onGet.TryInvoke() ;
      using ( UnityWebRequest webRequest = UnityWebRequest.Get( uri ) )
      {
#if UNITY_EDITOR
        Debug.Log( $"Dispatching a GET request to \"{uri}\"." ) ;
#endif
        yield return webRequest.SendWebRequest() ;

        if( webRequest.result != UnityWebRequest.Result.Success )
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
    }
#endregion


#region POST Request
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.<br/>
    ///     All handled exceptions should be caught here and not further upstream. 
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a POST request to the RestApi.
    /// </summary>
    /// <param name="uri">The identifier to which the request is be sent</param>
    /// <param name="form">A WWWForm that contains the full post data</param>
    /// <param name="onResult">
    ///   The action which should be performed on the text body received by
    ///   the UnityWebRequest's DownloadHandler.
    /// </param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    protected IEnumerator PostRequest( string uri, byte[] data, Action<string> onResult )
    {
      debugMessages?.onPost.TryInvoke() ;
      using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST") )
      {
        webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(data) ;
        webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer() ;
        webRequest.SetRequestHeader("Content-Type", "application/json") ;

#if UNITY_EDITOR
       Debug.Log( $"Dispatching a POST request to \"{uri}\"." ) ;
#endif
        yield return webRequest.SendWebRequest() ;

        if( webRequest.result != UnityWebRequest.Result.Success )
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
    }
#endregion


#region PUT Request
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.<br/>
    ///     All handled exceptions should be caught here and not further upstream. 
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a PUT request to the RestApi.
    /// </summary>
    /// <param name="uri">The identifier to which the request is be sent</param>
    /// <param name="jsonData">A string containing json formatted data. Use JsonUtility.ToJson(data) on a Serializable object matching your request.</param>
    /// <param name="onResult">
    ///   The action which should be performed on the text body received by
    ///   the UnityWebRequest's DownloadHandler.
    /// </param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    protected IEnumerator PutRequest( string uri, byte[] data, Action<string> onResult )
    {
      debugMessages?.onPut.TryInvoke() ;
      using (UnityWebRequest webRequest = new UnityWebRequest(uri, "PUT") )
      {
        webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(data) ;
        webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer() ;
        webRequest.SetRequestHeader("Content-Type", "application/json") ;

#if UNITY_EDITOR
        Debug.Log( $"Dispatching a PUT request to \"{uri}\"." ) ;
#endif
        yield return webRequest.SendWebRequest() ;

        if( webRequest.result != UnityWebRequest.Result.Success )
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
    }
#endregion


#region DELETE Request
    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    ///   <para>
    ///     Only call this as a Unity Coroutine.<br/>
    ///     All handled exceptions should be caught here and not further upstream. 
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   Makes a DELETE request to the RestApi.
    /// </summary>
    /// <param name="uri">The identifier to which the request is be sent</param>
    /// <param name="onResult">
    ///   The action which should be performed on the text body received by
    ///   the UnityWebRequest's DownloadHandler.
    /// </param>
    /// <returns>Coroutine yield.</returns>
    /// <exception cref="Exception">Network related exceptions //
    /// TODO: find and handle exactly which network related exceptions
    /// </exception>
    protected IEnumerator DeleteRequest( string uri, Action<string> onResult )
    {
      debugMessages?.onDelete.TryInvoke() ;
      using (UnityWebRequest webRequest = UnityWebRequest.Delete( uri ) )
      {
#if UNITY_EDITOR
        Debug.Log( $"Dispatching a DELETE request to \"{uri}\"." ) ;
#endif
        yield return webRequest.SendWebRequest() ;

        if( webRequest.result != UnityWebRequest.Result.Success )
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
    }
#endregion


#region DebugMessages
  [Serializable]
  class DebugMessages
  {
    [SerializeField] public DebugMessage onGet ;
    [SerializeField] public DebugMessage onPost ;
    [SerializeField] public DebugMessage onPut ;
    [SerializeField] public DebugMessage onDelete ;
  }
#endregion
  }

}