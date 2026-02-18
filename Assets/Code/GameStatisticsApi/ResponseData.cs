
using System;

namespace GameStatisticsApi.ResponseData
{
#region RequestResponse
  internal class RequestResponse
  {
    public bool ok ;
    public bool cacheable ;
    public string[] custody_chain ;
    public string _meta ;
    public string error ;
  }

// ResponseContext
  [Serializable]
  internal class ResponseContext <TKey>
  {
    public TKey keys ;
  }

// ContextKey
  [Serializable]
  internal class ContextKey
  {
    public string of ;
    public string[] forT ;
  }

// DeletionInfo
  [Serializable]
  internal class DeletionInfo
  {
    public string location ;
    public int count ;
  }
#endregion


#region Session
//SessionRowData
  [Serializable]
  internal class SessionRowData
  {
    public int id ;
    public string started_at ;  // JSON can not read DateTime
    public string ended_at ;
    public string recorded_at ;
  }
// SessionContextKeys
  [Serializable]
  internal class SessionContextKeys
  {
    public ContextKey id ;
  }
// GetSessionResponse
  [Serializable]
  internal class GetSessionResponse : RequestResponse
  {
    public SessionRowData[] data;
    public ResponseContext<SessionContextKeys> context;
  }
// PostSessionData
  [Serializable]
  internal class PostOrPutSessionData
  {
    public string started_at ;
    public string ended_at ;
  }
// PostSessionResponse
  [Serializable]
  internal class PostOrPutSessionResponse : RequestResponse
  {
    public int insert_id ;
    public ResponseContext<SessionContextKeys> context;
  }
#endregion


#region Input
//InputRowData
  [Serializable]
  internal class InputRowData
  {
    public int session_id ;
    public int times_buttons_clicked ;
    public int distance_moved ;
    public int etc ;
    public string recorded_at ;
  }
// PostOrPutInputResponse
  [Serializable]
  internal class PostOrPutInputResponse : RequestResponse
  {
    public int insert_id ;
    public ResponseContext<SessionContextKeys> context;
  }
  [Serializable]
  internal class GetInputResponse : RequestResponse
  {
    public InputRowData[] data;
    public ResponseContext<SessionContextKeys> context;
  }
#endregion


#region Interaction
//InteractionRowData
  [Serializable]
  internal class InteractionRowData
  {
    public int session_id ;

    // TODO: add appropriate fields
  }
  [Serializable]
  internal class GetInteractionResponse : RequestResponse
  {
    public InteractionRowData[] data;
    public ResponseContext<SessionContextKeys> context;
  }
#endregion


#region Time
//TimeRowData
  [Serializable]
  internal class TimeRowData
  {
    public int session_id ;
    public float in_menus ;
    public float in_exploration ;
    public float in_dialogue ;
    public string recorded_at ;
  }
// PostOrPutInputResponse
  [Serializable]
  internal class PostOrPutTimeResponse : RequestResponse
  {
    public int insert_id ;
    public ResponseContext<SessionContextKeys> context;
  }
  [Serializable]
  internal class GetTimeResponse : RequestResponse
  {
    public TimeRowData[] data;
    public ResponseContext<SessionContextKeys> context;
  }
#endregion

#region DeletionResponse
// DeletionResponse
  [Serializable]
  internal class DeletionResponse : RequestResponse
  {
    public DeletionInfo[] deletions ;
    public ResponseContext<SessionContextKeys> context;
  }
#endregion
}