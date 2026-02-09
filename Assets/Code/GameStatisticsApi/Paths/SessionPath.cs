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
#region Cache
    internal Dictionary<int,SessionRowData> Cache { get ; } = new () ;
#endregion


#region GET
    public override IEnumerator Get( int[] ids ) => throw new InvalidOperationException( "SessionPath GET does not handle more than one id." ) ;
    public override IEnumerator Get( int id )
    {
      yield return StartCoroutine( base.Get( id, (text) => {
        GetSessionResponse res = JsonUtility.FromJson<GetSessionResponse>(text) ;

        int entriesAdded = 0 ;
        int entriesUpdated = 0 ;
        foreach( SessionRowData data in res.data )
        {
          if( Cache.TryGetValue( data.id, out SessionRowData cachedData ) )
          {
            if( true /* && data.recorded_at > cachedData.recorded_at */ )
            {
              // replace entry with updated data from server
              Cache[data.id] = data ;
              entriesUpdated++ ;
            }
          }
          else
          {
            // create a new entry using server data
            Cache.Add( data.id, data ) ;
            entriesAdded++;
          }
        }
        Debug.Log( $"Request successful! Entries added: {entriesAdded} ; Entries updated: {entriesUpdated} ; Total entries: {Cache.Count}" ) ;
      } ) ) ;
    }
    public override IEnumerator Get() => Get(-1) ;
#endregion


#region POST
    public override IEnumerator Post( int[] ids, byte[] data ) => throw new InvalidOperationException( "SessionPath POST does not handle any id parameters." ) ;
    public override IEnumerator Post( int id, byte[] data ) => Post( new []{-1}, data ) ;
    public override IEnumerator Post( byte[] data )
    {
      yield return StartCoroutine( base.Post( data, (text) =>
        {
          PostSessionResponse res = JsonUtility.FromJson<PostSessionResponse>(text) ;

          Debug.Log( $"Inserted a new entry into session with an id of {res.insert_id}." ) ;
        } )
      ) ;
    }
#endregion


#region PUT
    public override IEnumerator Put( int id, byte[] data ) { throw new NotImplementedException() ; }
#endregion


#region DELETE
    public override IEnumerator Delete( int id ) { throw new NotImplementedException() ; }
#endregion
    
  }

}