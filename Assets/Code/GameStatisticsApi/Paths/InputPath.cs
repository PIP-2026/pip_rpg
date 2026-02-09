using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameStatisticsApi.ResponseData;
using UnityEngine;

namespace GameStatisticsApi 
{

  public class InputPath : ApiPath
  {
#region Cache
    internal Dictionary<int,InputRowData> Cache { get ; } = new () ;
#endregion


#region GET
    public override IEnumerator Get( int[] ids ) => throw new InvalidOperationException( "SessionPath does not handle more than one id." ) ;
    public override IEnumerator Get( int id ) { throw new NotImplementedException() ; }
    public override IEnumerator Get() => Get(-1) ;
#endregion


#region POST
    public override IEnumerator Post( byte[] data ) { throw new NotImplementedException() ; }
#endregion


#region PUT
    public override IEnumerator Put( int id, byte[] data ) { throw new NotImplementedException() ; }
#endregion


#region DELETE
    public override IEnumerator Delete( int id ) { throw new NotImplementedException() ; }
#endregion
    
  }

}