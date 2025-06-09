using System.Collections;
using System.Collections.Generic;
using JUTPS;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    [SerializeField] private JUHealth _bossHealth;
    [SerializeField] private JUHealth _handHealth;
    [SerializeField] private GameObject _handGo;
    [SerializeField] private float _handSelfDamage;
    [SerializeField] private float firstPhaseHealthBorder = 50;
    [SerializeField] private float _attackTimerCooldown = 5;
    [SerializeField] private int _switchAttackCounter = 5;
    [SerializeField] private HomingMissile MissilePrefab;
    [SerializeField] private Transform MissileSpawnTransform;
    [SerializeField] private int _missileCountPerAttack = 3;
    [SerializeField] private float _missileDelay = 0.3f;
    [SerializeField] private List<Transform> _vulnerabilityPhaseOnePositionList;
    [SerializeField] private Transform _vulnerabilityPhaseTwoPosition;
    [SerializeField] private List<float> _vulnerabilityDurationList;
    [SerializeField] private List<float> _specialAttackDurationList;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform ThrowStoneHandTransformParent;
    [SerializeField] private Stone StonePrefab;
    [SerializeField] private float _bossPullingForce = 1000f;
    [SerializeField] private float _delayAfterSpecialAttack = 5f;
    [SerializeField] private float _pullingTime = 10f;
    [SerializeField] private float _delayBeforePulling = 2f;
    [SerializeField] private GameObject _whirlwindParticles;
    [SerializeField] private GameObject _objectToDestroyOnPhaseChange;

    private float _currentAttackTimer;
    private int _currentAttackCounter;
    private int _phaseCounter;
    private JUHealth _playerHealth;
    private bool _specialAttackActive;
    private bool _pulling;
    private Stone _stone;
    private Rigidbody _playerRigidBody;

    void Start()
    {
        var player = GameObject.Find("TPS Character");
        if (player != null)
        {
            _playerHealth = player.GetComponent<JUHealth>();
            _playerRigidBody = player.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("Игрок с именем 'TPS Character' не найден!");
        }
        
        ReviveHand();
        SetPhase(0);

        _handHealth.OnUnitDeath += VulnerabilityDeath;
        _bossHealth.OnUnitDeath += OnBossDeath;
    }

    private void OnBossDeath()
    {
        _animator.SetTrigger("Death");
        _whirlwindParticles.SetActive(false);
        _pulling = false;
    }

    private void SetPhase(int phaseCounter)
    {
        if (phaseCounter == 1 && _objectToDestroyOnPhaseChange != null)
        {
            Destroy(_objectToDestroyOnPhaseChange);
        }
        _phaseCounter = phaseCounter;
        _currentAttackTimer = _attackTimerCooldown;
        SetVulnerability(false);
    }

    private void SetVulnerabilityPosition(int phaseCounter)
    {
        var parentTransform = GetVulnerabiltyParentTransform(phaseCounter);
        _handGo.transform.parent = parentTransform;
        _handGo.transform.localPosition = Vector3.zero;
    }

    private Transform GetVulnerabiltyParentTransform(int phaseCounter)
    {
        if (phaseCounter == 0)
        {
            return _vulnerabilityPhaseOnePositionList[Random.Range(0, _vulnerabilityPhaseOnePositionList.Count)];
        }

        return _vulnerabilityPhaseTwoPosition;
    }

    private void ReviveHand()
    {
        _handHealth.Health = _handHealth.MaxHealth;
    }

    private void VulnerabilityDeath()
    {
        _bossHealth.DoDamage(_handSelfDamage);
        if (!_bossHealth.IsDead)
        {
            _animator.SetTrigger("HitBoss");
        }
        SetVulnerability(false);
        _specialAttackActive = false;
        
        if(_bossHealth.Health <= firstPhaseHealthBorder)
            SetPhase(1);
        
        if(!_bossHealth.IsDead)
            ReviveHand();
    }

    private void OnDestroy()
    {
        _handHealth.OnUnitDeath -= VulnerabilityDeath;
        _bossHealth.OnUnitDeath -= OnBossDeath;
    }

    void Update()
    {
        if (_pulling)
        {
            var playerTransform = _playerHealth.transform;
            var bossPosition = new Vector3(transform.position.x, playerTransform.position.y, transform.position.z);
            var pullDirection = bossPosition - playerTransform.position;
            _playerRigidBody.AddForce(pullDirection * _bossPullingForce);
        }
        
        Vector3 direction = _playerHealth.transform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
        
        if (_playerHealth.IsDead || _bossHealth.IsDead)
            return;
        
        if (_currentAttackTimer <= 0)
        {
            AttackPlayer();
            _currentAttackTimer = _attackTimerCooldown;
            return;
        }

        if (_specialAttackActive)
            return;
        
        _currentAttackTimer -= Time.deltaTime;
    }

    private void AttackPlayer()
    {
        if (_currentAttackCounter == _switchAttackCounter)
        {
            _currentAttackCounter = 0;
            SpecialAttack(_phaseCounter);
        }
        else
        {
            MainAttack();
            _currentAttackCounter++;
        }
    }

    private void SpecialAttack(int phaseCounter)
    {
        _specialAttackActive = true;
        StartCoroutine(ActivateBossVulnerability(phaseCounter));
    }

    private IEnumerator ActivateBossVulnerability(int phaseCounter)
    {
        if (phaseCounter == 1)
        {
            StartCoroutine(StartPulling());
        }
        _animator.SetTrigger($"SpecialAttack{phaseCounter}");
        _specialAttackActive = true;
        var animationDuration = phaseCounter == 0 ? 7 : 3.33f;
        yield return new WaitForSeconds(animationDuration);
        SetVulnerabilityPosition(phaseCounter);
        SetVulnerability(true);
        yield return new WaitForSeconds(_vulnerabilityDurationList[phaseCounter]);
        SetVulnerability(false);
    }

    public void SpecialAttackEnd()
    {
        StartCoroutine(StopSpecialAttack());
    }

    private IEnumerator StopSpecialAttack()
    {
        yield return new WaitForSeconds(_delayAfterSpecialAttack);
        _currentAttackTimer = 0;
        _specialAttackActive = false;
    }

    private IEnumerator StartPulling()
    {
        yield return new WaitForSeconds(_delayBeforePulling);
        _pulling = true;
        _whirlwindParticles.SetActive(true);
        StartCoroutine(StopPulling());
    }

    private IEnumerator StopPulling()
    {
        yield return new WaitForSeconds(_pullingTime);
        _pulling = false;
        _whirlwindParticles.SetActive(false);
    }

    public void SpawnStone()
    {
        var stone = Instantiate(StonePrefab, MissileSpawnTransform);
        stone.transform.parent = ThrowStoneHandTransformParent;
        stone.transform.localPosition = Vector3.zero;
        _stone = stone;
    }

    public void ThrowStone()
    {
        if (_stone != null)
        {
            _stone.Throw();
            _stone = null;
        }
    }

    private void MainAttack()
    {
        _animator.SetTrigger("MainAttack");
        StartCoroutine(LaunchMissiles());
    }

    private IEnumerator LaunchMissiles()
    {
        for (int i = 0; i < _missileCountPerAttack; i++)
        {
            var missile = Instantiate(MissilePrefab, MissileSpawnTransform);
            missile.transform.localPosition = Vector3.zero;
            missile.transform.parent = null;
            yield return new WaitForSeconds(_missileDelay);
        }
    }

    private void SetVulnerability(bool state)
    {
        _handGo.SetActive(state);
    }
}
