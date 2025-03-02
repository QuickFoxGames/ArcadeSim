using UnityEngine;
using MGUtilities;
using System.Collections.Generic;
public class Asteroids : MonoBehaviour
{
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_moveAcceleration;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private float m_shotDelay;
    [SerializeField] private float m_bulletFlightTime;
    [SerializeField] private Transform m_bulletPrefab;
    private bool m_gameEnded = false, m_canShoot = true;
    private int m_rotateDirection;
    private Vector3 m_velocity = Vector2.zero;
    private Transform m_transform;

    private List<Bullet> m_reserveBullets = new();
    private List<Bullet> m_activeBullets = new();
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
            if (m_activeBullets[i].m_flightTime >= m_bulletFlightTime) ReturnBullet(m_activeBullets[i]);
        }
    }
    #endregion
}