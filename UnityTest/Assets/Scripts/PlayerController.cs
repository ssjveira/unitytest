using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.0f;

    [SerializeField]
    private Text m_CountText = null;

    [SerializeField]
    private Text m_WinText = null;

    private Rigidbody m_rigidBody = null;
    private int m_count = 0;

    private void Start()
    {
        m_count = 0;
        m_rigidBody = GetComponent<Rigidbody>();
        SetCountText();
        m_WinText.text = "";
    }

    void FixedUpdate()
    {
        var moveHorizontal = Input.GetAxis("Horizontal");
        var moveVertical = Input.GetAxis("Vertical");

        var movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        m_rigidBody.AddForce(movement * speed * Time.deltaTime);
    }

    private void SetCountText()
    {
        m_CountText.text = "Count: " + m_count.ToString();
        if(m_count >= 12)
        {
            m_WinText.text = "You win!";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false);
            m_count++;
            SetCountText();
        }
    }
}
