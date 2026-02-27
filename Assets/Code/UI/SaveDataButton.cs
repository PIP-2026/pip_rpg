using UnityEngine;
using TMPro;
/// <remarks>
///   <para>
///     Author: Christof Kloninger / <a href="mailto:gme.24.kloninger@gmail.com">gme.24.kloninger@gmail.com</a>
///   </para>
///   <para>
///     Issue: <a href="https://github.com/PIP-2026/pip_rpg/issues/33">link to issue</a>
///   </para>
/// </remarks>
/// <summary>
///   The Buttons are instantiated by the Slot Panel which reacts to Editions to the Profile.
/// </summary>

public class SaveDataButton : MonoBehaviour
{
  [SerializeField] TextMeshProUGUI nameText ;
  [SerializeField] TextMeshProUGUI dateText ;
  private int _slotIndex ;
  private UserSaveData _saveData ;
  public void Initialize(int index, UserSaveData data)
  {
    _slotIndex = index ;
    _saveData = data ;
    nameText.text = $"SaveSlot {index + 1}(ID: {data.SessionId})";
    if (data.statistics != null)
    {
      dateText.text = data.statistics.TimeStartedAt.ToString("dd/MM/yyyy HH/mm");
    }
  }
  public void OnClick()
  {
    SaveManager.SelectedSaveData = this._saveData;
  }
}
