using UnityEngine;
/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///     Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/33">link to issue</a>
///   </para>
/// </remarks>
/// <summary>
///   Reacts to Editions of the Profile and Repopulates itself with the Save Buttons.
/// </summary>

public class SaveFilePanel : MonoBehaviour
{
  [SerializeField] GameObject saveDataButtonPrefab;
  private int _slotIndex ;
  private void Start()
  {
    _slotIndex = transform.GetSiblingIndex() ;
    OurEventSystem.ProfileEdited.AddListener(PopulateLayout) ;
  }
  private void OnDestroy()
  {
    OurEventSystem.ProfileEdited.RemoveListener(PopulateLayout) ;
  }
  private void PopulateLayout(UserProfile profile)
  {
    foreach(Transform child in transform) Destroy(child.gameObject) ;
    foreach(UserSaveData data in profile.UserSaveDatas)
    {
      if(data.SlotIndex == _slotIndex)
      {
        GameObject btn = Instantiate(saveDataButtonPrefab, transform) ;
        btn.GetComponent<SaveDataButton>().Initialize(_slotIndex, data) ;
      }
    }
  }
}
