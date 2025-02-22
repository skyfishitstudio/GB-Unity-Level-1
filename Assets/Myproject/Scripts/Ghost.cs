using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace LearnProject
{
    public class Ghost : MonoBehaviour,ITakeDamage
    {
        [SerializeField] private Player _player;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _spawnPositionBullet;
        [SerializeField] private int _enemylevel;// ������� ��������
        [SerializeField] private float _enemyhealth = 50f; // �������� ��������
        [SerializeField] private float _cooldown = 1f; // ����� ����� ����������
        [SerializeField] private bool _IsFire;
        [SerializeField] private bool _patroling = false;
        [SerializeField] private bool _harrasment = false;
        [SerializeField] private Transform[] _waypoints;
        private Transform _pp;
        private Rigidbody _rigidbody;
        private Vector3 _direction;
        private NavMeshAgent _agent;
        int m_CurrentWaypointIndex;
        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = FindObjectOfType<Player>();
        }
       
        void Start()
        {
             StartCoroutine(Patrol( _waypoints));
        }
                  
        public void Init(float enemyhealth,int enemylevel) //������������� �������� ��� ������ ��������
        {
            enemyhealth = _enemyhealth;
            enemylevel = _enemylevel;
            //Destroy(gameObject, 15f); ���� ����� ��� ������������ �������� ����� �������� �����
        }
        private void FixedUpdate()
        {
            if ((_agent.remainingDistance < _agent.stoppingDistance) && _patroling) //���� �������������� ������� � ����� �� ����� ����������� ����� �� ��������
            {
                
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % _waypoints.Length;
                _agent.SetDestination(_waypoints[m_CurrentWaypointIndex].position);
            }
            Ray ray = new Ray(_spawnPositionBullet.position, transform.forward); //������ �������� �� 2 ������

            if (Physics.Raycast(ray, out RaycastHit hit, 2))
            {
                Debug.DrawRay(_spawnPositionBullet.position, transform.forward * hit.distance, Color.blue); //����� ������� ��������
                if (hit.collider.CompareTag("Player")) //�������� � ������� ������ ���� ���������� ������ 2 � ���� ���������
                {
                    if (_IsFire)
                        Fire();
                }
            }
            //������� � ������� ������ ���� ���������� ������ 5 (�������� �����)
            if( Vector3.Distance(transform.position, _player.transform.position) < 5f)
            {
                StopCoroutine(Patrol(_waypoints)); //���� �������� ��������� �������
                StartCoroutine(Harassment(_player));//�������� �������������
            }
            else if (_patroling == false) // ���� ����� �� ���� ����� � �������������� ��� �� �������� �� ������ ������� ������
            {
                StopCoroutine(Harassment(_player)); //���� �� � ���� ���������� ��������� �������������
                StartCoroutine(Patrol(_waypoints));//�������� �������
            }
        }
        private void Fire()
        {
            _IsFire = false;
            var bulletObj = Instantiate(_bulletPrefab, _spawnPositionBullet.position, _spawnPositionBullet.rotation);//����� ����
            var bullet = bulletObj.GetComponent<Bullet>();//����� ������� ���� ��� ������� ��������� ����
            bullet.Init(_player.transform,10f,3f); //��������� :���� ����� � ������� ������ �� ��������� 3
            Invoke(nameof(Reloading), _cooldown);
            //_event?.Invoke();
        }

        private void Reloading()
        {
            _IsFire = true;
        }
        public void Hit(int damage)
        {
            _enemyhealth -= damage;//�������� �� �������� �������� ���������� ����
            if (_enemyhealth <=0) //����� �������� ������ ��� ����� 0
                Destroy(gameObject, 1f); // ������ ������ �� ����� ����� ���
        }
        IEnumerator Patrol(Transform[] _waypoints)
        {
            _agent.stoppingDistance = 0;
            _patroling = true;
            _harrasment = false;
            Debug.Log("Wait start");
            yield return new WaitForSeconds(0.5f); //����� ����� ������� �������
            Debug.Log("Start");
            foreach (Transform nextTrnsPp in _waypoints)
            {
                Debug.Log("Wait rotate");
               yield return new WaitForSeconds(1f); //����� ����� ������� ��������
                Debug.Log("Rotate");
                transform.LookAt(nextTrnsPp.transform.position);
                Debug.Log("Wait move");
                yield return new WaitForSeconds(1f); //����� ����� ������� �������� �� ��������
                Debug.Log("Move");
                _agent.SetDestination(nextTrnsPp.transform.position);
                yield return new WaitForSeconds(3f); //����� ����� ������������ �������� �� ��������
            }
            StartCoroutine(Patrol(_waypoints));
        }
        IEnumerator Harassment(Player _player)
        {
            _patroling = false;
            _harrasment = true;
            _agent.stoppingDistance = 2;
            yield return new WaitForSeconds(1f); //����� ����� ������� ��������
            transform.LookAt(_player.transform.position);
            yield return new WaitForSeconds(1f); //����� ����� ������� �������������
            _agent.SetDestination(_player.transform.position);
        }
    }
}

