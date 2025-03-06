using UnityEngine;
using TMPro;
public class Pacman : MonoBehaviour
{
    [SerializeField] private int m_pointsPerPellet;
    [SerializeField] private float m_radius;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private Transform m_pelletPrefab;
    [SerializeField] private TextMeshProUGUI m_scoreText;
    private int m_score;
    private Vector3 m_direction;
    private Transform m_transform;
    void Start()
    {
        m_transform = transform;
    }
    void Update()
    {
        m_direction = Input.GetKey(KeyCode.W) ? Vector2.up : Input.GetKey(KeyCode.S) ? Vector2.down : Input.GetKey(KeyCode.A) ? Vector2.left : Input.GetKey(KeyCode.D) ? Vector2.right : m_direction;
        m_transform.position += m_moveSpeed * Time.deltaTime * m_direction;

        CheckCollisions();
    }
    private void CheckCollisions()
    {
        Collider2D hit = Physics2D.OverlapCircle(m_transform.position, m_radius);
        if (hit != null)
        {
            int layer = hit.gameObject.layer;
            if (layer == 6) // Pellet
            {
                m_score += m_pointsPerPellet;
                m_scoreText.text = m_score.ToString();
                Destroy(hit.gameObject);
            }
            if (layer == 7) // Enemy
            {
                
            }
            if (layer == 8) // Wall
            {

            }
        }
    }
}