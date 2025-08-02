using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private List<Transform> players;


    private void Update()
    {
        Vector3 center = Vector3.zero;
        foreach (Transform t in players)
        {
            if (t == null)
                players.Remove(t);
            else
                center += new Vector3(t.position.x, 0, t.position.z);
        }
        transform.position = center/players.Count + offset;
    }
}
