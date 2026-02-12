

using System;
using UnityEngine;

namespace GameStatisticsApi 
{

  internal class ApiEndpoint : MonoBehaviour
  {
    [SerializeField] public ApiAction GetAll ;
    [SerializeField] public ApiAction GetOne ;
    [SerializeField] public ApiAction Add ;
    [SerializeField] public ApiAction Update ;
    [SerializeField] public ApiAction Delete ;
  }

}