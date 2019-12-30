using Unity.Entities;

/// <summary>
/// Each agent get access on Input information.
/// </summary>
public struct InputComponent : IComponentData
{
    public bool keyOnePressedDown; // Bool for key "1" (when pressed down) -> manager reacts and adds a tag component to crowd entity
    public bool keyOnePressedUp; // Bool for key "1" (when pressed up) -> spawns agents when pressing key "1" up (UnitSpawnerSystem)
    public bool keyTwoPressedUp;
    public bool keyThreePressedUp; // Bool for key "3" (when pressed up) after Barriers rotated outside
    public bool keyFourPressedUp; // Bool for key "4" (when pressed up) after Barriers rotated inside
    public bool keyFivePressedUp;
    public bool keySixPressedUp;
    public bool keySevenPressedUp;
}
