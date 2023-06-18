using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playercontrol : MonoBehaviour
{
    [SerializeField, Header("移動速度")] float movespeed;                  // キャラクターの移動速度を管理する変数
    [SerializeField, Header("ジャンプ力")] float jumppower;                // キャラクターのジャンプ力を管理する変数
    [SerializeField, Header("重力")] float gravityScale;                  // キャラクターの重力を管理する変数 
    [SerializeField, Header("相手プレイヤー")] GameObject _EnemyPlayer;    // 相手プレイヤーのgameobjectを管理する変数
    [SerializeField, Header("ローカル座標での前方")] private Vector3 _foward = Vector3.forward;  //前方の基準となるローカル空間ベクトル
    Rigidbody rb;   //リギッドボディを管理する変数

    playerattack _playerAttack;

    private Vector2 _inputMove;             // InputSystem のコントローラーのスティックの座標を管理する変数
    private float _inputMove_Degrees;       //InputSystemのスティック入力を度に変換した値を管理する変数


    private int player_move_F = 8;              //（4下段ガード、しゃがみ　5しゃがみ　6しゃがみ）0前方ジャンプ　1垂直ジャンプ　2後ろジャンプ 3ガード、後退　8ニュートラル　7前進　

    private bool _jumpF = false;            // ジャンプ中のフラグを管理する変数
    private bool _pressJumpKey;
    private bool _airF = true;              // 空中にいる状態のフラグを管理する変数
    private int air_count;

    private bool _controlF = true;          // キャラクターの操作の可能状態のフラグを管理する

    private int _MoveStop_Count_MAX;        //動きを止めるフレーム数
    private int _MoveStop_Count;            //動きを止めるフレーム数のカウント    
    private bool _MoveStopF = false;        //動きを止めるフラグ

    private float _Move_Rad;            //動きの角度の変数

    private int _LogMove = 8;                   //移動キーのログを管理する変数

    private bool _GuardF = false;          //ガードするフラグ
    private bool _MovefixF = false;          //動き反転しているフラグ

    private GameObject _EnemyGuardColl = null;


    //バックステップの変数一覧
    private int _BackStep = 0;              
    private int _BackStepCount = 0;         
    private int _BackStepCancelCount = 60;
    private bool _BackStepF = false;

    private float _Vertical;            //垂直
    private float _Horizontal;          //水平
    private bool _FreeMoveF=false;      //フリーランができるかどうかのフラグ

    private Animator _Animator;
    // Start is called before the first frame update
    void Start()
    {
        //変数代入
        rb = GetComponent<Rigidbody>();
        _playerAttack = GetComponent<playerattack>();

        _Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //アニメーション管理
        _Animator.SetBool("_BackStepAnim", _BackStepF);
        _Animator.SetBool("_Air", _airF);
        _Animator.SetInteger("_MoveFAnim", player_move_F);
        


        if (Input.GetKeyDown(KeyCode.A))
        {
            _MoveStop(60);
        }

        if (_GetViewport() > _EnemyPlayer.GetComponent<playercontrol>()._GetViewport())
        {
            _MovefixF = true;
        }
        else
        {
            _MovefixF = false;
        }


        //Debug.Log(name + "ビュー視点" + _GetViewport());
        //Debug.Log(name + " _MovefixF " + _MovefixF);

        if (_EnemyGuardColl != null)
        {

            if (_EnemyGuardColl.GetComponent<BoxCollider>().enabled == false)
            {
                _GuardF = false;
                //Debug.Log(_EnemyGuardColl.GetComponent<BoxCollider>().enabled);
            }
        }
        if (_BackStepCancelCount <= 0)
        {
            _BackStepCancelCount = 90;
            _BackStep = 0;
        }




        //自身と相手の二点間で角度を求める
        if(_airF==false)
        {
            _Move_Rad = Mathf.Atan2(_EnemyPlayer.transform.position.z - transform.position.z, _EnemyPlayer.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            _Move_Rad = _Move_Rad * Mathf.Deg2Rad;
        }



        //プレイヤーの向きを補正
        //プレイヤーの向きを合わせる
        if(_airF==false&&_FreeMoveF==false)
        {
            var dir = new Vector3(_EnemyPlayer.transform.position.x - transform.position.x, 0f, _EnemyPlayer.transform.position.z - transform.position.z);
            var lookatRotation = Quaternion.LookRotation(dir, Vector3.up);
            var offsetRotation = Quaternion.FromToRotation(_foward, Vector3.forward);
            transform.rotation = lookatRotation * offsetRotation;
        }

    }
    private void FixedUpdate()
    {
        //プレイヤー
        if (_airF)
        {
            rb.position += new Vector3(0, air_count * -gravityScale, 0);
            air_count++;
        }
        // 操作可能な状態のみ処理の更新をする
        if (_controlF)
        {
            _Move();        // 移動処理をする関数
        }

        // 入力したときにジャンプする
        if ((player_move_F == 6 || player_move_F == 7 || player_move_F == 0) && _jumpF == false&&_FreeMoveF==false)
        {
            _jumpF = true;
            _MoveStop(3);
        }
        //ジャンプさせる関数
        if (_jumpF)
        {
            _Jump();
        }

        //動きを止める
        if (_MoveStopF)
        {
            _MoveStop_Count++;
            if (_MoveStop_Count_MAX <= _MoveStop_Count)
            {
                _MoveStopF = false;
                _MoveStop_Count = 0;
                _MoveStop_Count_MAX = 0;
                if (player_move_F == 9)
                {
                    player_move_F = 8;
                }
                if (_BackStepF)
                {
                    _BackStepF = false;
                    _BackStepCount = 0;
                }
            }
        }
        // Debug.Log(player_move_F);
        if (_BackStep != 0)
        {
            _BackStepCancelCount--;
        }
        if (_BackStepF)
        {
            _BackStepCount++;
            if (_BackStepCount == 15)
            {
                player_move_F = 10;
            }
            if (_BackStepCount == 20)
            {
                player_move_F = 8;
            }
        }
    }

    private void _Move()
    {

        if (_FreeMoveF)
        {
            // カメラの方向から、X-Z平面の単位ベクトルを取得
            Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

            // 方向キーの入力値とカメラの向きから、移動方向を決定
            Vector3 moveForward = cameraForward * _Vertical + Camera.main.transform.right * _Horizontal;

            // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
            rb.velocity = moveForward * movespeed + new Vector3(0, rb.velocity.y, 0);

            // キャラクターの向きを進行方向に
            if (moveForward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveForward);
            }
        }
        //_FreeMoveがfalseになっているとき
        else
        {

            //Debug.Log(player_move_F);
            //前進又は前方ジャンプ
            if (player_move_F == 1 || player_move_F == 0)
            {
                rb.velocity = new Vector3(1f * movespeed*Mathf.Cos(_Move_Rad), 0f, 1f * movespeed * Mathf.Sin(_Move_Rad));
                Debug.Log(name + "Rad:" + _Move_Rad + "Sin:" + Mathf.Sin(_Move_Rad) + "Cos:" + Mathf.Cos(_Move_Rad));
            }
            //後退又は後方ジャンプの時
            else if (player_move_F == 5 || player_move_F == 6)
            {
                if (_GuardF == false)
                {
                    rb.velocity = new Vector3(-0.8f * movespeed * Mathf.Cos(_Move_Rad), 0f, -0.8f * movespeed * Mathf.Sin(_Move_Rad));

                }
                else
                {
                    rb.velocity = new Vector3(0f, 0f, 0f);
                }
            }
            //ノックバック
            else if (player_move_F == 9)
            {
                rb.velocity = new Vector3(-3f * movespeed * Mathf.Cos(_Move_Rad), 0f, -3f * movespeed * Mathf.Sin(_Move_Rad));
            }
            //ノックバック
            else if (player_move_F == 11)
            {
                rb.velocity = new Vector3(-12f * movespeed * Mathf.Cos(_Move_Rad), 0f, -12f * movespeed * Mathf.Sin(_Move_Rad));
            }
            //backstep
            else if (player_move_F == 10)
            {
                rb.velocity = new Vector3(-5f * movespeed * Mathf.Cos(_Move_Rad), 0f, -5f * movespeed * Mathf.Sin(_Move_Rad));
            }
            else
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }


        }

    }

    //ジャンプを管理する変数
    private void _Jump()
    {

        rb.position += new Vector3(0, jumppower, 0);

    }

    private void OnCollisionEnter(Collision collision)
    {
        //ステージの地面とぶつかったとき
        if (collision.gameObject.CompareTag("stage"))
        {
            //ジャンプを初期化
            _jumpF = false;
            _airF = false;
            air_count = 0;
            player_move_F = 8;

        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("stage"))
        { _airF = true; }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("guard"))
        {
            _EnemyGuardColl = other.gameObject;
            _GuardF = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("guard"))
        {
            _GuardF = false;
        }
    }


    // コントローラーでの入力を取得する関数
    public void _OnMove(InputAction.CallbackContext context)
    {

        // コントローラーの入力のベクトルを取得する
        _inputMove = context.ReadValue<Vector2>();
        //Debug.Log(context.ReadValue<Vector2>());
        //スティック入力の角度を求める
        _inputMove_Degrees = Mathf.Atan2(_inputMove.x, _inputMove.y) * Mathf.Rad2Deg;

        if (_inputMove_Degrees < 0f)
        {
            _inputMove_Degrees += 360f;
        }
        //Debug.Log(_inputMove_Degrees);

        if (_airF == false && _MoveStopF == false)
        {
            //垂直ジャンプしていた場合
            if ((_inputMove_Degrees >= 337.5f && _inputMove_Degrees <= 360f) || (_inputMove_Degrees >= 0 && _inputMove_Degrees < 22.5))
            {
                player_move_F = 7;
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    if (_inputMove_Degrees >= 22.5f + 45f * i && _inputMove_Degrees < 22.5 + 45f * (i + 1))
                    {
                        if (i == 5 && player_move_F != 5 && _BackStep == 0 && transform.localEulerAngles.y <= 100.0f)
                        {
                            _BackStep++;
                        }
                        player_move_F = i;
                        break;
                    }
                }
            }

            if (player_move_F != 9)
            {
                _LogMove = player_move_F;           // 移動キーの入力ログを更新する
            }

            //逆向きのとき
            if (_MovefixF)
            {
                if (player_move_F == 0)
                {
                    player_move_F = 6;
                }
                else if (player_move_F == 1)
                {
                    if (player_move_F != 1 && _BackStep == 0)
                    {
                        _BackStep++;
                    }
                    player_move_F = 5;
                }
                else if (player_move_F == 2)
                {
                    player_move_F = 4;
                }
                else if (player_move_F == 4)
                {
                    player_move_F = 2;
                }
                else if (player_move_F == 5)
                {
                    player_move_F = 1;
                }
                else if (player_move_F == 6)
                {
                    player_move_F = 0;
                }
                _MovefixF = true;
            }
            if (_BackStep == 2 && player_move_F == 5)
            {
                _MoveStop(30);
                _BackStepF = true;
                _BackStepCancelCount = 60;
            }
            _Vertical = _inputMove.y;
            _Horizontal = _inputMove.x;
        }

        if (context.canceled && _airF == false)
        {
            if (_BackStep == 1)
            {
                _BackStep++;
            }
            player_move_F = 8;
            _LogMove = 8;
        }
    }

    // プレイヤーの移動の状態を返すメソッド
    public int _GetMove()
    {
        return player_move_F;
    }

    public int _GetLogMove()
    {
        return _LogMove;
    }


    //プレイヤーの状態を変えるメソッド
    public void _changeMove(int N)
    {
        if (_airF == false)
        {
            player_move_F = N;
        }

    }

    //動きをストップさせるメソッド 引数iはストップさせるフレーム数
    public void _MoveStop(int i)
    {
        _MoveStop_Count_MAX = i;
        _MoveStopF = true;
    }


    //_MoveStopFがtrueかどうか返すメソッド
    public bool _Get_MoveStopF()
    {
        return _MoveStopF;
    }

    // ポーズ画面の処理
    public void _OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {

        }
    }

    // ボタンヒントの表示・非表示を切り替えるメソッド
    public void _OnBottunHint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            LogController._bActive = !LogController._bActive;
        }
    }


    //ノックバックを設定するメソッド　
    //※nはフレーム数
    public void _setKnockBackMAX(int n)
    {
        _MoveStop(n);
        player_move_F = 9;
    }
    public void _sethardKnockBackMAX(int n)
    {
        _MoveStop(n);
        player_move_F = 11;
    }

    //フリーランのフラグを管理するメソッド
    public void _setFreemove(bool b)
    {
        _FreeMoveF = b;
    }

    public float _GetViewport()
    {
        return Camera.main.WorldToViewportPoint(transform.position).x;
    }

    //-------------------------------------------------
    // 6/12　小田健人　更新
    // PrologueControl　で使用するメソッド
    //-------------------------------------------------
    //敵の情報を返すメソッド
    public GameObject _getEnemyObj()
    {
        return _EnemyPlayer;
    }

    // 移動速度を返すメソッド
    public float _getMovespeed()
    { return movespeed; }
}
