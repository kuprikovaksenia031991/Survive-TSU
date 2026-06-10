using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFixer : MonoBehaviour
{
public Transform player; 
public LayerMask collisionLayers; 
public float offset = 0.2f;

void LateUpdate() 
{
    Vector3 dirToCamera = transform.position - player.position;
    float distToCamera = dirToCamera.magnitude;

    if (Physics.Raycast(player.position, dirToCamera.normalized, out RaycastHit hit, distToCamera, collisionLayers)) 
    {
        transform.position = hit.point + hit.normal * offset;
    }
}
}
