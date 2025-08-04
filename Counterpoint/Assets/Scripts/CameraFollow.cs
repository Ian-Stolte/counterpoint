using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private List<Transform> players;

    [Header("Zoom")]
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float minDist;
    [SerializeField] private float maxDist;
    [SerializeField] private float zoomSpeed;


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
        transform.position = center / players.Count + offset;

        //zoom based on player distance from each other
        if (players.Count > 1)
        {
            float dist = Mathf.Abs(players[0].position.x - players[1].position.x) + 2 * Mathf.Abs(players[0].position.z - players[1].position.z);
            float targetZoom = Mathf.Lerp(minZoom, maxZoom, Mathf.InverseLerp(minDist, maxDist, dist));
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }
}
