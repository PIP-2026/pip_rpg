using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameStatisticsApi.ResponseData;
using UnityEngine;

namespace GameStatisticsApi 
{

  public class SessionPath : ApiPath
  {
#region GET
    public override IEnumerator Get( int[] ids ) => throw new InvalidOperationException( "SessionPath does not handle more than one id." ) ;
    public override IEnumerator Get( int id )
    {
      yield return StartCoroutine( base.Get( id, (text) => {
        GetSessionResponse res = JsonUtility.FromJson<GetSessionResponse>(text) ;

        int entriesAdded = 0 ;
        int entriesUpdated = 0 ;
        foreach( SessionRowData data in res.data )
        {
          if( _api.CachedSessionData.TryGetValue( data.id, out SessionRowData cachedData ) )
          {
            if( true /* && data.recorded_at > cachedData.recorded_at */ )
            {
              // replace entry with updated data from server
              _api.CachedSessionData[data.id] = data ;
              entriesUpdated++ ;
            }
          }
          else
          {
            // create a new entry using server data
            _api.CachedSessionData.Add( data.id, data ) ;
            entriesAdded++;
          }
        }
        Debug.Log( $"Request successful! Entries added: {entriesAdded} ; Entries updated: {entriesUpdated} ; Total entries: {_api.CachedSessionData.Count}" ) ;
      } ) ) ;
    }
    public override IEnumerator Get() => Get(-1) ;
#endregion


#region POST
    public override IEnumerator Post( int id, WWWForm form, Action<string> onResult ) { throw new NotImplementedException() ; }
    public override IEnumerator Post( int id, WWWForm form ) { throw new NotImplementedException() ; }
#endregion


#region PUT
    public override IEnumerator Put( int id, WWWForm form, Action<string> onResult ) { throw new NotImplementedException() ; }
    public override IEnumerator Put( int id, WWWForm form) { throw new NotImplementedException() ; }
#endregion


#region DELETE
    public override IEnumerator Delete( int id, Action<string> onResult ) { throw new NotImplementedException() ; }
    public override IEnumerator Delete( int id ) { throw new NotImplementedException() ; }
#endregion
    
  }

}