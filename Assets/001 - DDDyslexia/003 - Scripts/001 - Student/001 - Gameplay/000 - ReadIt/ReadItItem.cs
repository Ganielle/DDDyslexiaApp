using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReadItData 1", menuName = "DDDyslexia/ReadIt/ReadItData")]
public class ReadItItem : ScriptableObject
{
    [field: SerializeField] public string Letter { get; private set; }
    [field: SerializeField] public AudioClip LetterClip { get; private set; }
}
