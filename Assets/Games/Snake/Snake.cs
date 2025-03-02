using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MGUtilities;
public class Snake : MonoBehaviour
{
    [Header("Map setup")]
    [SerializeField] private int m_minX;
    [SerializeField] private int m_maxX;
    [SerializeField] private int m_minY;
    [SerializeField] private int m_maxY;
    [Header("Snake setup")]
    [SerializeField] private int m_moveStep;
    [SerializeField] private float m_updateTime;
    [SerializeField] private Transform m_segmentPrefab;
    [SerializeField] private LayerMask m_selfLayer;
    [SerializeField] private LayerMask m_foodLayer;
    [Header("Food setup")]
    [SerializeField] private int m_foodPoints;
    [SerializeField] private int m_winFoodPoints;
    [SerializeField] private Transform m_foodPrefab;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_scoreText;
    [SerializeField] private TextMeshProUGUI m_endScreenText;

    private bool m_up, m_down, m_left, m_right, m_gameEnded = false;
    private int m_score = 0;
    private float m_currentTime = 0f;

    private Vector3 m_direction;

    private Transform m_transform;

    private List<Transform> m_segments = new();
    private List<Transform> m_reserveFood = new();
    private List<Transform> m_activeFood = new();
    private void Start()
    {
        m_transform = transform;
        SpawnFood();
    }
    void Update()
    {
        if (m_gameEnded) return;
        m_up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        m_down = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        m_left = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        m_right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        if (m_up || m_down || m_left || m_right)
        {
            Vector3 newDirection = new((m_left ? -1 : 0) + (m_right ? 1 : 0), (m_down ? -1 : 0) + (m_up ? 1 : 0), 0f);
            if (newDirection != Vector3.zero && newDirection != -m_direction) m_direction = newDirection;
        }

        m_currentTime += Time.deltaTime;

        if (m_currentTime >= m_updateTime)
        {
            m_currentTime = 0f;
            if (m_segments.Count > 0)
            {
                for (int i = m_segments.Count - 1; i > 0; i--)
                {
                    m_segments[i].position = m_segments[i - 1].position;
                }
                m_segments[0].position = m_transform.position;
            }
            m_transform.position += m_direction * m_moveStep;

            Vector3 pos = m_transform.position;
            if (pos.x > m_maxX) pos.x = m_minX;
            if (pos.x < m_minX) pos.x = m_maxX;
            if (pos.y > m_maxY) pos.y = m_minY;
            if (pos.y < m_minY) pos.y = m_maxY;
            m_transform.position = pos;

            CheckCollisions();
        }
    }
    private void CheckCollisions()
    {
        Collider2D self = Physics2D.OverlapBox(m_transform.position, new(1f,1f), 0f, m_selfLayer);
        if (self != null)
        {
            m_endScreenText.text = "GAME OVER";
            m_endScreenText.color = Color.red;
            EndGame();
            return;
        }
        Collider2D food = Physics2D.OverlapBox(m_transform.position, new(1f,1f), 0f, m_foodLayer);
        if (food != null)
        {
            m_segments.Add(Instantiate(m_segmentPrefab, m_transform.position - m_direction, Quaternion.identity));
            m_score += m_foodPoints;
            m_scoreText.text = $"Score: {m_score}";
            if (m_score >= m_winFoodPoints)
            {
                m_endScreenText.text = "WIN";
                m_endScreenText.color = Color.green;
                EndGame();
                return;
            }
            ReturnFood(food.transform);
            SpawnFood();
        }
    }
    private void EndGame()
    {
        m_gameEnded = true;
        StartCoroutine(Coroutines.LerpFloatOverTime(1f, 0f, 3f, value => Time.timeScale = value));
        StartCoroutine(Coroutines.LoadSceneOverTime(0, 2f));
    }
    #region FoodPool
    private void ReturnFood(Transform food)
    {
        if (m_activeFood.Contains(food))
        {
            m_activeFood.Remove(food);
            m_reserveFood.Add(food);
            food.gameObject.SetActive(false);
        }
    }
    private Transform GetFood(Vector2 pos, Quaternion rot)
    {
        if (m_reserveFood.Count == 0)
            AddFood();

        Transform food = m_reserveFood[^1];
        m_activeFood.Add(food);
        m_reserveFood.Remove(food);
        food.transform.SetPositionAndRotation(pos, rot);
        food.gameObject.SetActive(true);
        return food;
    }
    private void AddFood()
    {
        m_reserveFood.Add(Instantiate(m_foodPrefab));
    }
    #endregion
    private void SpawnFood()
    {
        GetFood(new(Random.Range(m_minX, m_maxX), Random.Range(m_minY, m_maxY)), Quaternion.identity);
    }
}