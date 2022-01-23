using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamAnchor : MonoBehaviour
{
    [SerializeField]
    private Rigidbody target;

    private Vector3 newOffset = Vector3.zero;
    private Vector3 oldOffset;

    // Update is called once per frame
    void LateUpdate()
    {
        oldOffset = target.transform.position;
        newOffset += (oldOffset + (target.velocity) - newOffset) * Time.deltaTime* 2f;

        Vector3 offset = Vector3.Lerp(newOffset, oldOffset, 0.1f);

        transform.position = offset;


        transform.rotation = target.rotation;
    }
}
