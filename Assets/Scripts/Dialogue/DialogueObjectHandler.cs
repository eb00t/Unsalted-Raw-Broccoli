using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Piece", menuName = "ScriptableObjects/Dialogue Object", order = 3)]
public class DialogueObjectHandler : ScriptableObject
{
    [field: Tooltip("If the speaker is unknown, put ???? here.")]
    public List<string> whoIsSpeaking;

    [field: Tooltip("The contents of the dialogue.")] [TextArea(3, 10)]
    public List<string> dialogueBodyText;

    [field: Tooltip("If you don't want the dialogue to have a speaker, set this to false")]
    public bool isAnyoneSpeaking;
}