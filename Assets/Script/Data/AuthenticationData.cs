using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthenticationData : MonoBehaviour
{
    public readonly string Username;

    public AuthenticationData(string username)
    {
        Username = username;
    }
}
