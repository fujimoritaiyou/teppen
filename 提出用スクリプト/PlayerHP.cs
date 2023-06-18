using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : playerattack
{
    public int _PlayerHp;      //プレイヤーの体力
    private int _MaxHP = 100;
    public bool _HitHardAttack = false;            //強攻撃が当たったか  
    public bool _HitWeakAttack = false;            //弱攻撃が当たったか
    public bool _HitEXAttack = false;              //EX攻撃が当たったか
    public bool _HitStareAttack = false;           //

    [SerializeField, Header("HPバーのイメージ画像")] Image _HPBarImage;                       //HPBarのイメージ画像を追加
    private playerattack _pAttack;

    //サウンド追加用
    [SerializeField, Header("サウンド管理")] SoundManager soundManager;
    [SerializeField] AudioClip HardHit;
    [SerializeField] AudioClip WeakHit;

    //エフェクト追加用
    [SerializeField, Header("エフェクト管理")] EffectManager effectManager;
    [SerializeField] ParticleSystem HardHitEffect;          // 強攻撃のエフェクト
    [SerializeField] ParticleSystem WeakHitEffect;          // 弱攻撃のエフェクト

    int iAttack = 0;
    private bool HardHitAnim;
    private bool WeakHitAnim;

    // Start is called before the first frame update
    void Start()
    {
        _pAttack = GetComponent<playerattack>();
        //プレイヤーのHPを最大に
        _PlayerHp = _MaxHP;

        _playercontrol = GetComponent<playercontrol>();

        _Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        _Animator.SetBool("_HitHardAttack", _HitHardAttack);
        _Animator.SetBool("_HitWeakAttack", _HitWeakAttack);
        _Animator.SetBool("_HitStareAttack", _HitStareAttack);      // 掴み合いダメージのアニメーション



        //攻撃が当たったか
        if (_HitHardAttack)
        {

            if (soundManager != null)
            {
                soundManager.PlaySe(HardHit);
            }
            effectManager.InstantiateEffect(HardHitEffect, this.gameObject.transform);
            if (_playercontrol._GetMove() != 5)
            {
                _PlayerHp -= 10;
                
                _playercontrol._sethardKnockBackMAX(20);
            }
            _playercontrol._setKnockBackMAX(10);
            //Debug.Log("slider.value : " + slider.value);
            _HitHardAttack = false;
        }
        if (_HitWeakAttack)
        {
            
            if (soundManager != null)
            {
                soundManager.PlaySe(WeakHit);
            }
            effectManager.InstantiateEffect(WeakHitEffect, this.gameObject.transform);
            if (_playercontrol._GetMove() != 3)
            {
                _PlayerHp -= 3;
                _playercontrol._setKnockBackMAX(6);
            }

            //Debug.Log("slider.value : " + slider.value);
            _HitWeakAttack = false;
        }

        if (_HitStareAttack)
        {
            if (soundManager != null)
            {
                soundManager.PlaySe(WeakHit);
            }
            effectManager.InstantiateEffect(WeakHitEffect, this.gameObject.transform);
            _PlayerHp -= 4;

            _HitStareAttack = false;
        }

        //HPが0になったらリザルトに移動する
        if (_PlayerHp <= 0)
        {
            StartCoroutine("Defeat");
        }

        if (_HPBarImage != null)
        {
            _HPBarImage.fillAmount = (float)_PlayerHp / _MaxHP;
            //Debug.Log(_PlayerHp);
        }

    }
    private void FixedUpdate()
    {/*
        if (_HitEXAttack)
        {
            if (iAttack % 2 == 0)
            {
                if (_playercontrol._GetMove() == 5)
                {
                    _PlayerHp -= 1;
                }
                else
                {
                    _PlayerHp -= 5;
                }
            }
            iAttack++;

            if (iAttack % 16 == 0)
            { _HitEXAttack = false; }
        }*/
    }

    //攻撃の当たり判定
    private void OnTriggerEnter(Collider other)
    {

        //髪の毛が触れているか
        if (other.gameObject.CompareTag("Hit"))
        {
            //Debug.Log(other.gameObject.name + ":" + other.gameObject.tag);

            //Debug.Log(other.gameObject.transform.root.gameObject.name);     // オブジェクトの一番上のオブジェクトを参照する

            //Debug.Log("強攻撃：" + other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._HardAttackF);
            //Debug.Log("弱攻撃：" + other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._WeakAttackF);
            //Debug.Log("EX攻撃：" + other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._EXAttackF);

            //攻撃判定が出ている時Hitフラグを上げる
            // 強攻撃の当たり判定
            if (other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._HardAttackF)
            {
                Debug.Log("[2] other.gameObject.tag : " + other.gameObject.tag);
                _HitHardAttack = true;
            }
            // 弱攻撃の当たり判定
            if (other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._WeakAttackF)
            {
                Debug.Log("[2] other.gameObject.tag : " + other.gameObject.tag);
                _HitWeakAttack = true;
            }
            //掴みの当たり判定
            if (other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._stareAttackF)
            {
                //Debug.Log("[2] other.gameObject.tag : " + other.gameObject.tag);
                // 一方だけが掴み状態の場合
                if (!this.gameObject.GetComponent<playerattack>()._stareAttackF)
                {
                    _HitStareAttack = true;         // 掴み攻撃を行う
                }
                // 両プレイヤーが掴みをしていた場合
                else
                {
                    // 掴み愛イベントを開始する
                    stareStagingControl._stareEvent_Start = true;
                }
            }
            //if (other.gameObject.transform.root.gameObject.GetComponent<playerattack>()._EXAttackF)
            //{
            //    //Debug.Log("[2] other.gameObject.tag : " + other.gameObject.tag)
            //    _HitEXAttack = true;
            //}
        }
    }

    IEnumerator Defeat()
    {
        //処理を少し遅らせてKOとか出るようにする


        // 1秒遅らせる
        yield return new WaitForSeconds(1);

        //リザルトに移動
        //Debug.Log("出来てるよ！");
    }

    public void _setConsecutive(bool _Consecutive)
    {
        _HitStareAttack = _Consecutive;
    }

    //プレイヤーのHPを返すプログラム
    public int _getPlayerHP()
    {
        return _PlayerHp;
    }
}
