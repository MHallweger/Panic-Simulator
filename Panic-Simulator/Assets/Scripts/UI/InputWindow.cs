using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeMonkey;
using CodeMonkey.MonoBehaviours;
using CodeMonkey.Utils;

/// <summary>
/// Script that handles different things for the Input window.
/// </summary>
public class InputWindow : MonoBehaviour
{
    #region Variables
    // Singleton instance variable
    public static InputWindow instance;

    // Consts
    private const string validCharacters = "0123456789";

    // Input window Game Objects
    public TMP_InputField inputField;
    private Button button;
    #endregion // Variables

    /// <summary>
    /// Assign instance variable.
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Get current AmountToSpawn and set it to inputField text.
    /// </summary>
    private void Start()
    {
        inputField.text = UnitSpawnerProxy.instance.AmountToSpawn.ToString();
    }

    void Update()
    {
        ValidateInput();
        CheckEnterKeyInput();
    }

    /// <summary>
    /// Checks if a given Character is i inside the validCharacters string.
    /// https://www.youtube.com/watch?v=4n6RT805rCc&t=268s
    /// </summary>
    /// <param name="validCharacters">All valid characters for example 0123456789</param>
    /// <param name="addedChar">The test character</param>
    /// <returns></returns>
    private char ValidateCharacter(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }

    /// <summary>
    /// Call method that checks valid int input.
    /// </summary>
    private void ValidateInput()
    {
        inputField.characterLimit = 7;
        inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateCharacter(validCharacters, addedChar);
        };
    }

    /// <summary>
    /// Method that checks if the save button was pressed.
    /// If the button was pressed, save the new value.
    /// </summary>
    public void SaveValue()
    {
        UnitSpawnerProxy.instance.AmountToSpawn = int.Parse(inputField.text);
        //CMDebug.TextPopup("Entity Anzahl angepasst!", new Vector3(button.gameObject.transform.position.x + 20f, button.gameObject.transform.position.y,button.gameObject.transform.position.z));
    }

    /// <summary>
    /// Checks if the user pressed one of the enter buttons to save his input. When pressed, call SaveValue() Method.
    /// </summary>
    private void CheckEnterKeyInput()
    {
        if (Input.GetKeyDown("return") || Input.GetKeyDown("enter"))
        {
            SaveValue();
        }
    }
}
