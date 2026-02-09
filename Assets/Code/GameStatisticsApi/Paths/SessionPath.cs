using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStatisticsApi.ResponseData;
using UnityEngine;

namespace GameStatisticsApi 
{

  public class SessionPath : ApiPath
  {
#region Cache
    internal Dictionary<int,SessionRowData> Cache { get ; } = new () ;
    /** TEST */
    public int MySessionId = -1 ;
    /** End */
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
          PostOrPutSessionResponse res = JsonUtility.FromJson<PostOrPutSessionResponse>(text) ;

          Debug.Log( $"Inserted a new entry into session with an id of {res.insert_id}." ) ;
          MySessionId = res.insert_id ; // TEST
        } )
      ) ;
    }
#endregion


#region PUT
    public override IEnumerator Put( int[] ids, byte[] data ) => throw new InvalidOperationException( "SessionPath PUT does not handle more than one id." ) ;
    public override IEnumerator Put( int id, byte[] data )
    {
      yield return StartCoroutine( base.Put( id, data, (text) =>
        {
          PostOrPutSessionResponse res = JsonUtility.FromJson<PostOrPutSessionResponse>(text) ;

          Debug.Log( $"Updated the session entry with the id of {res.insert_id}." ) ;
        } )
      ) ;
    }
    public IEnumerator Put( byte[] data ) // TEST
    {
      yield return StartCoroutine( base.Put( MySessionId, data, (text) =>
        {
          PostOrPutSessionResponse res = JsonUtility.FromJson<PostOrPutSessionResponse>(text) ;

          Debug.Log( $"Updated the session entry with the id of {res.insert_id}." ) ;
        } )
      ) ;
    }
#endregion


#region DELETE
    public override IEnumerator Delete( int[] ids ) => throw new InvalidOperationException( "SessionPath DELETE does not handle more than one id." ) ;
    public override IEnumerator Delete( int id )
    {
      yield return StartCoroutine( base.Delete( id, (text) =>
        {
          DeleteSessionResponse res = JsonUtility.FromJson<DeleteSessionResponse>(text) ;

          Debug.Log( $"Deleted {res.deletions.Sum( (d) => d.count )} entries from session and related tables." ) ;
        } )
      ) ;
    }
#endregion
    
  }

}