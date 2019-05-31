using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Player = null;

    private Vector3 m_Offset = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        m_Offset = transform.position - m_Player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = m_Player.transform.position + m_Offset;
    }
}
