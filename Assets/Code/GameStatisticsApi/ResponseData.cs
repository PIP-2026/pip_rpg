
using System;

namespace GameStatisticsApi.ResponseData
{
#region RequestResponse
  internal class RequestResponse
  {
    public bool ok ;
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
    public bool cacheable ;
    public string[] custody_chain ;
    public SessionRowData[] data;
    public ResponseContext<SessionContextKeys> context;
    public string _meta ;
  }
#endregion
}