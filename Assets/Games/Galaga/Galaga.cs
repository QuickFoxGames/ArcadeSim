using UnityEngine;
public class Galaga : MonoBehaviour
{
    [Header("Map setup")]
    [SerializeField] private int m_minX;
    [SerializeField] private int m_maxX;
    [SerializeField] private int m_minY;
    [SerializeField] private int m_maxY;
    [Header("Ship Setup")]
    [SerializeField] private float m_shipSpeed;
    [SerializeField] private Transform m_shipPrefab;
    [Header("Alien Setup")]
    [SerializeField] private Transform m_blueAlien;
    [SerializeField] private Transform m_redAlien;
    [SerializeField] private Transform m_greenAlien;

    private bool m_gameEnded = false;
    private float m_vertIn, m_horzIn;
    private Transform m_transform;
    private void Start()
    {
        m_transform = transform;
    }
    void Update()
    {
        if (m_gameEnded) return;
        m_vertIn = Input.GetAxisRaw("Vertical");
        m_horzIn = Input.GetAxisRaw("Horizontal");

        m_transform.position += m_shipSpeed * Time.deltaTime * new Vector3(m_horzIn, m_vertIn);

        Vector3 pos = m_transform.position;
        if (pos.x > m_maxX) pos.x = m_maxX;
        if (pos.x < m_minX) pos.x = m_minX;
        if (pos.y > m_maxY) pos.y = m_maxY;
        if (pos.y < m_minY) pos.y = m_minY;
        m_transform.position = pos;
    }
}