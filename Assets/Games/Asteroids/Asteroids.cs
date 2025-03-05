using UnityEngine;
using MGUtilities;
using System.Collections.Generic;
using System.Linq;
public class Asteroids : MonoBehaviour
{
    [Header("Ship Setup")]
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_moveAcceleration;
    [SerializeField] private float m_bulletDamage;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private float m_shotDelay;
    [SerializeField] private float m_bulletFlightTime;
    [SerializeField] private Transform m_bulletPrefab;
    [Header("Asteroid Setup")]
    [SerializeField] private float m_asteroidSpawnTime;
    [SerializeField] private float m_asteroidMinSpeed;
    [SerializeField] private float m_asteroidMaxSpeed;
    [SerializeField] private float m_asteroidFlightTime;
    [SerializeField] private Asteroid m_asteroidPrefab;
    [SerializeField] private LayerMask m_asteroidLayer;

    private bool m_gameEnded = false, m_canShoot = true;
    private int m_rotateDirection;
    private float m_currentSpawnTime = 0f;
    private Vector3 m_velocity = Vector2.zero;
    private Transform m_transform;
    private List<Bullet> m_reserveBullets = new();
    private List<Bullet> m_activeBullets = new();
    private List<Asteroid> m_reserveAsteroids = new();
    private List<Asteroid> m_activeAsteroids = new();
    void Start()
    {
        m_transform = transform;
    }
    void Update()
    {
        if (m_gameEnded) return;
        m_rotateDirection = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? -1 : 0;
        if (m_rotateDirection != 0) m_transform.Rotate(Vector3.forward, m_rotateDirection * m_rotateSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W)) m_velocity = Vector2.Lerp(m_velocity, m_moveSpeed * m_transform.up, m_moveAcceleration * Time.deltaTime);
        m_transform.position += m_velocity * Time.deltaTime;
        m_velocity = Vector2.Lerp(m_velocity, Vector2.zero, 0.1f * m_moveAcceleration * Time.deltaTime);

        if (m_canShoot && Input.GetKey(KeyCode.Space)) Shoot();
        HandleBullets();
        HandleAsteroids();
        m_currentSpawnTime += Time.deltaTime;
        if (m_currentSpawnTime >= m_asteroidSpawnTime) SpawnAsteroid();
    }
    private void Shoot()
    {
        Bullet bullet = GetBullet(m_transform.position, Quaternion.LookRotation(m_transform.forward, m_transform.up));
        bullet.m_velocity = m_bulletSpeed * m_transform.up;
        StartCoroutine(Coroutines.DelayBoolChange(false, true, m_shotDelay, value => m_canShoot = value));
    }
    #region BulletPool
    private class Bullet
    {
        public float m_flightTime;
        public Vector3 m_velocity;
        public Transform m_transform;
    }
    private void ReturnBullet(Bullet bullet)
    {
        if (m_activeBullets.Contains(bullet))
        {
            m_activeBullets.Remove(bullet);
            m_reserveBullets.Add(bullet);
            bullet.m_transform.gameObject.SetActive(false);
            ResetBullet(bullet);
        }
    }
    private Bullet GetBullet(Vector2 pos, Quaternion rot)
    {
        if (m_reserveBullets.Count == 0)
            AddBullet();

        Bullet bullet = m_reserveBullets[^1];
        m_activeBullets.Add(bullet);
        m_reserveBullets.Remove(bullet);
        bullet.m_transform.SetPositionAndRotation(pos, rot);
        bullet.m_transform.gameObject.SetActive(true);
        return bullet;
    }
    private void AddBullet()
    {
        m_reserveBullets.Add(new() { m_transform = Instantiate(m_bulletPrefab), m_velocity = Vector2.zero });
    }
    private void ResetBullet(Bullet b)
    {
        b.m_transform.position = Vector2.zero;
        b.m_velocity = Vector2.zero;
        b.m_flightTime = 0;
    }
    private void HandleBullets()
    {
        for (int i = 0; i < m_activeBullets.Count; i++)
        {
            m_activeBullets[i].m_transform.position += m_activeBullets[i].m_velocity * Time.deltaTime;
            m_activeBullets[i].m_flightTime += Time.deltaTime;
            if (m_activeBullets[i].m_flightTime >= m_bulletFlightTime)
            {
                ReturnBullet(m_activeBullets[i]);
                return;
            }
            Collider2D asteroid = Physics2D.OverlapBox(m_activeBullets[i].m_transform.position, new Vector2(0.65f, 0.35f), m_activeBullets[i].m_transform.rotation.eulerAngles.z, m_asteroidLayer);
            if (asteroid != null)
            {
                ReturnBullet(m_activeBullets[i]);
                Asteroid a = asteroid.GetComponent<Asteroid>();
                a.m_hp -= m_bulletDamage;
                if (a.m_hp <= 0)
                {
                    Vector3 parentPosition = a.m_transform.position;
                    Vector3 parentVelocity = a.m_velocity;
                    Vector3 parentScale = a.m_transform.localScale;

                    ReturnAsteroid(a);

                    int num = Random.Range(2, 6);
                    for (int j = 0; j < num; j++)
                    {
                        Asteroid a1 = GetAsteroid(parentPosition, Quaternion.Euler(0f, 0f, Random.Range(-360f, 360f)));
                        a1.m_transform.localScale = parentScale / num;
                        a1.m_velocity = (1f + (num / 10f)) * parentVelocity.magnitude * (parentVelocity.normalized + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1))).normalized;
                    }
                }
            }
        }
    }
    #endregion
    #region AsteroidPool
    private void ReturnAsteroid(Asteroid a)
    {
        if (m_activeAsteroids.Contains(a))
        {
            m_activeAsteroids.Remove(a);
            m_reserveAsteroids.Add(a);
            a.m_transform.gameObject.SetActive(false);
            ResetAsteroid(a);
        }
    }
    private Asteroid GetAsteroid(Vector2 pos, Quaternion rot)
    {
        if (m_reserveAsteroids.Count == 0)
            AddAsteroid(pos, rot);

        Asteroid a = m_reserveAsteroids[^1];
        m_activeAsteroids.Add(a);
        m_reserveAsteroids.Remove(a);
        a.m_transform.SetPositionAndRotation(pos, rot);
        a.m_transform.gameObject.SetActive(true);
        return a;
    }
    private void AddAsteroid(Vector2 pos, Quaternion rot)
    {
        Asteroid a = Instantiate(m_asteroidPrefab);
        a.m_hp = 100f;
        a.m_transform = a.transform;
        a.m_velocity = Vector3.zero;
        a.m_flightTime = 0;
        a.m_transform.SetPositionAndRotation(pos, rot);
        m_reserveAsteroids.Add(a);
    }
    private void ResetAsteroid(Asteroid a)
    {
        a.m_transform.position = Vector2.zero;
        a.m_velocity = Vector2.zero;
        a.m_hp = 100;
        a.m_flightTime = 0;
        a.m_transform.localScale = Vector3.one;
    }
    private void HandleAsteroids()
    {
        for(int i = 0; i < m_activeAsteroids.Count; i++)
        {
            m_activeAsteroids[i].m_transform.position += m_activeAsteroids[i].m_velocity * Time.deltaTime;
            m_activeAsteroids[i].m_flightTime += Time.deltaTime;
            if (m_activeAsteroids[i].m_flightTime >= m_asteroidFlightTime)
            {
                ReturnAsteroid(m_activeAsteroids[i]);
            }
        }
    }
    private void SpawnAsteroid()
    {
        m_currentSpawnTime = 0f;
        Asteroid a = GetAsteroid(25f * new Vector3(Random.Range(-1, 1), Random.Range(-1, 1)) + Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(-360f, 360f)));
        a.m_velocity = (m_transform.position - a.m_transform.position).normalized * Random.Range(m_asteroidMinSpeed, m_asteroidMaxSpeed);
    }
    #endregion
}