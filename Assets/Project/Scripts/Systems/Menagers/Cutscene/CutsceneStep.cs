using UnityEngine;

[System.Serializable]
public class CutsceneStep
{
    public Camera camera;
    public float duration = 2f;
    [TextArea(2, 5)]
    public string dialogueText;
    public AudioClip audioClip;
}