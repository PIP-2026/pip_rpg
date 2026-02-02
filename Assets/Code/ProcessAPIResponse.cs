using System.Net;
using UnityEngine.Networking;
using UnityEngine;

/// <remarks>
///  <para>
///   Author: name / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///  </para>
///   <para>
///   Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/23">link to issue</a>
///  </para>
///  <para>
///   Usage/meta information for teammates.
///   Method uses HTTP Status Code for feedback of requests.
///   Class is solely established to avoid errors.
///   Incorporate method into API Class.
///  </para>
/// </remarks>
/// <summary>
///   Enum according to standard HTTP Status Codes. Use namespace System.Net for incorporation
///   Unity has inbuilt Handler Objects, but not a code for it.
/// </summary>

public class APIResponseHandler
{
  public void ProcessAPIResponse(UnityWebRequest request)
  {
    HttpStatusCode code= (HttpStatusCode)request.responseCode;

    switch (code)
    {
      case HttpStatusCode.OK: // 200
        Debug.Log("Request successful");
        break;
      case HttpStatusCode.Created:  // 201
        Debug.Log("Resource successfully created");
        break;
      case HttpStatusCode.BadRequest: // 400
        Debug.Log("Request incorrectly formed");
        break;
      case HttpStatusCode.Unauthorized: // 401
        Debug.Log("Request is unauthorized");
        break;
      case HttpStatusCode.NotFound: // 404
        Debug.Log("Path to item not found");
        break;
      case HttpStatusCode.Conflict: // 409
        Debug.Log("Conflict detected"); 
        break;
      case HttpStatusCode.InternalServerError:  // 500
        Debug.Log("Internal Server Error detected");
        break;
    }
  }  
}
