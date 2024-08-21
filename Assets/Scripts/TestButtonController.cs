using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtonController : MonoBehaviour   //≤‚ ‘”√
{
    [SerializeField] private Switchable door;
    private void OnGUI()
    {
        if (GUILayout.Button("Open", GUILayout.Width(200f), GUILayout.Height(50f))) door.Open();
        if (GUILayout.Button("Close", GUILayout.Width(200f), GUILayout.Height(50f))) door.Close();
        if (GUILayout.Button("Switch", GUILayout.Width(200f), GUILayout.Height(50f))) door.Switch();
    }
}
