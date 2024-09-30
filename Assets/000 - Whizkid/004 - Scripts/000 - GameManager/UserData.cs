using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "Whizkid/User/UserData")]
public class UserData : ScriptableObject
{
    [field: SerializeField] public string Token { get; set; }
    [field: SerializeField] public string Username { get; set; }

    private void OnEnable()
    {
        Username = "";
        Token = "";
    }
}
