using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform[] players;


    private void Update()
    {
        Vector3 center = Vector3.zero;
        foreach (Transform t in players)
        {
            center += new Vector3(t.position.x, 0, t.position.z);
        }
        transform.position = center/players.Length + offset;
    }
}
