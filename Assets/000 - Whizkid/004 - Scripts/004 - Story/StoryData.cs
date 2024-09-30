using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryData", menuName = "Whizkid/Story/StoryData")]
public class StoryData : ScriptableObject
{
    [field: SerializeField] public string Title { get; private set; }
    [field: TextArea][field: SerializeField] public string Content { get; private set; }
    [field: SerializeField] public Sprite StoryBG { get; private set; }
}
