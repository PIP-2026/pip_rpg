  /// <summary>
  /// Paneltypes are used to distinguish between single menu panels, serializable inventory panels, in case we extend the inventory system,
  /// and the dialogue panel which is treated a little bit differently since keeping track of it is important.
  /// </summary>
public enum PanelType
{
  None,
  Dialogue,
  Inventory,
  Menu,
  HUD
}
