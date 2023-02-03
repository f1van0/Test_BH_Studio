using UnityEngine;

namespace Data
{
    public class AuthenticationData : MonoBehaviour
    {
        public readonly string Username;

        public AuthenticationData(string username)
        {
            Username = username;
        }
    }
}
