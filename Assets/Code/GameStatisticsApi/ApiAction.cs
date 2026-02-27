using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
  public class ApiAction : MonoBehaviour
  {
    private static Regex _paramPattern = new (@"{[a-zA-Z]+}") ;
    protected enum RequestType
    {
      Invalid,
      None,
      GET,
      POST,
      PUT,
      DELETE,
    }

    /// <remarks>
    ///   <para>
    ///     Author: Maria Wickes / <a href="mailto:maria.lindling@protonmail.com">maria.lindling@protonmail.com</a>
    ///   </para>
    /// </remarks>
    /// <summary>
    ///   The base path of this API endpoint, not including parameters.
    /// </summary>
    /// <value>URL as <c>/</c> segmented string</value>
    [SerializeField] protected RequestType requestType = RequestType.None ;
    [SerializeField] protected string endpointPath = "" ;
    [SerializeField] public DebugMessage debugMessage ;

    public virtual string BaseURI => $"http://{RestApi.Host}:{RestApi.Port}{RestApi.Root}{endpointPath}" ;


#region Make Web Request
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
    ///   Makes a request to the RestApi.
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
    internal IEnumerator Call( int[] ids, byte[] data, Action<string> onResult )
    {
      debugMessage?.TryInvoke() ;

      /** Construct URI */
      string uri = BaseURI ;
      
      if( _paramPattern.Matches(uri).Count != ids.Length )
        throw new ArgumentException( $"A {requestType}-request to the API path \"{endpointPath}\" takes {_paramPattern.Matches(uri).Count} parameters, but {ids.Length} were given.") ;

      for( int i = 0; i < ids.Length; i++)
      {
        uri = _paramPattern.Replace(uri,ids[i].ToString()) ;
      }
      /** End */


      /** Make Web Request */
      using UnityWebRequest webRequest = new ( uri, requestType.ToString() );

      if (data != null)
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
      webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
      webRequest.SetRequestHeader("Content-Type", "application/json");

#if UNITY_EDITOR
      Debug.Log($"Dispatching a {requestType} request to \"{uri}\".");
#endif
      yield return webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
#if UNITY_EDITOR
        Debug.Log($"Web request encountered an error: {webRequest.error}");
#endif
        yield break;
      }
      else
      {
#if UNITY_EDITOR
        Debug.Log($"Received: {webRequest.downloadHandler.text}");
#endif
      }
      /** End */


      /** Handle Response Body */
      onResult?.Invoke(webRequest.downloadHandler.text);
      /** End */
    }


    internal IEnumerator Call( int id, byte[] data, Action<string> onResult ) => Call( new int[]{id}, data, onResult ) ;
    internal IEnumerator Call( byte[] data, Action<string> onResult ) => Call( new int[]{}, data, onResult ) ;

    internal IEnumerator Call( int id, Action<string> onResult ) => Call( new int[]{id}, null, onResult ) ;
    internal IEnumerator Call( Action<string> onResult ) => Call( new int[]{}, null, onResult ) ;

    internal IEnumerator Call( int id ) => Call( new int[]{id}, null, null ) ;
    internal IEnumerator Call() => Call( new int[]{}, null, null ) ;
#endregion
  }

}