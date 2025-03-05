using UnityEngine;
using TMPro;
using MGUtilities;
public class PongPlayer : MonoBehaviour
{
    [Header("Map setup")]
    [SerializeField] private float m_minY;
    [SerializeField] private float m_maxY;
    [Header("Game setup")]
    [SerializeField] private int m_scorePerHit;
    [SerializeField] private Paddle m_playerPaddle;
    [SerializeField] private Paddle m_aiPaddle;
    [SerializeField] private Ball m_ball;
    [SerializeField] private TextMeshProUGUI m_endText;

    private bool m_gameEnded = false;

    private float m_vertIn;
    private float m_deltaTime;
    [System.Serializable]
    public class Paddle
    {
        public int _score;
        public int _lives;
        public float _moveSpeed;
        public float _offset;
        public Vector3 _lastPos;
        public Transform _transform;
        public TextMeshProUGUI _scoreText;
        public TextMeshProUGUI _livesText;
    }
    [System.Serializable]
    public class Ball
    {
        public float _paddleInfluence;
        public float _offset;
        public Vector3 _velocity;
        public Transform _transform;
    }
    void Start()
    {
        m_ball._velocity = Vector2.left * 5f;
        m_playerPaddle._livesText.text = m_playerPaddle._lives.ToString();
        m_aiPaddle._livesText.text = m_aiPaddle._lives.ToString();
    }
    void Update()
    {
        if (m_gameEnded) return;
        m_deltaTime = Time.deltaTime;
        m_vertIn = Input.GetAxisRaw("Vertical");

        m_playerPaddle._transform.position += m_playerPaddle._moveSpeed * m_deltaTime * m_vertIn * Vector3.up;
        Vector3 playerPos = m_playerPaddle._transform.position;
        if (playerPos.y > m_maxY - m_playerPaddle._offset) playerPos.y = m_maxY - m_playerPaddle._offset;
        if (playerPos.y < m_minY + m_playerPaddle._offset) playerPos.y = m_minY + m_playerPaddle._offset;
        m_playerPaddle._transform.position = playerPos;

        m_ball._transform.position += m_deltaTime * m_ball._velocity;
        Vector3 ballPos = m_ball._transform.position;
        if (ballPos.y > m_maxY - m_ball._offset || ballPos.y < m_minY + m_ball._offset) m_ball._velocity = new Vector3(m_ball._velocity.x, -m_ball._velocity.y);
        m_ball._transform.position = ballPos;

        CheckCollision();

        m_playerPaddle._lastPos = m_playerPaddle._transform.position;
        m_aiPaddle._lastPos = m_aiPaddle._transform.position;

        if (m_playerPaddle._lives <= 0) EndGame("GAME OVER", Color.red);
        if (m_aiPaddle._lives <= 0) EndGame("WIN", Color.green);
    }
    private void CheckCollision()
    {
        Collider2D paddle = Physics2D.OverlapCircle(m_ball._transform.position, m_ball._offset * 0.5f);
        if (paddle != null)
        {
            if (paddle.gameObject.layer == 6) // Player
            {
                if (m_ball._transform.position.x - (0.5f * m_ball._offset) > m_playerPaddle._transform.position.x)
                {
                    CalculateBounceAngle(m_playerPaddle, Vector2.right);
                    m_playerPaddle._score += m_scorePerHit;
                    m_playerPaddle._scoreText.text = m_playerPaddle._score.ToString();
                }
                else LoseLife(m_playerPaddle);
            }
            if (paddle.gameObject.layer == 7) // AI
            {
                if (m_ball._transform.position.x + (0.5f * m_ball._offset) < m_aiPaddle._transform.position.x)
                {
                    CalculateBounceAngle(m_aiPaddle, Vector2.left);
                    m_aiPaddle._score += m_scorePerHit;
                    m_aiPaddle._scoreText.text = m_aiPaddle._score.ToString();
                }
                else LoseLife(m_aiPaddle);
            }
            if (paddle.gameObject.layer == 8) // Bounds
            {
                if (m_ball._transform.position.x < 0) LoseLife(m_playerPaddle);
                if (m_ball._transform.position.x > 0) LoseLife(m_aiPaddle);
            }
        }
    }
    private void CalculateBounceAngle(Paddle paddle, Vector2 normal)
    {
        Vector2 paddleVelocity = (paddle._transform.position - paddle._lastPos) / m_deltaTime;

        Vector2 oldBallVelocity = m_ball._velocity;

        Vector2 reflectedVelocity = Vector2.Reflect(oldBallVelocity, normal);

        float relativeVerticalSpeed = paddleVelocity.y - oldBallVelocity.y;
        reflectedVelocity.y += relativeVerticalSpeed * m_ball._paddleInfluence;

        m_ball._velocity = reflectedVelocity;
    }
    private void LoseLife(Paddle paddle)
    {
        paddle._lives--;
        paddle._livesText.text = paddle._lives.ToString();
        m_ball._transform.position = Vector2.zero;
        m_ball._velocity = Vector2.left * 5f;
    }
    private void EndGame(string text, Color c)
    {
        m_gameEnded = true;
        m_endText.text = text;
        m_endText.color = c;
        StartCoroutine(Coroutines.LerpFloatOverTime(1f, 0f, 3f, value => Time.timeScale = value));
        StartCoroutine(Coroutines.LoadSceneOverTime(0, 2f));
    }
}