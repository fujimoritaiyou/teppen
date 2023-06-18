/*
 作成者：坂田
 このスクリプトは基本触らないようにしてください！！！
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    //プレイヤーオブジェクトを入れる変数
    public GameObject Target1;
    public GameObject Target2;
    public GameObject VirtualTarget;

    Vector3 CenterPos;      //プレイヤー間の中央座標
    public float OffcetY = 4;   //カメラのY座標
    public float ratio;     //カメラの比率
    public float Rot;       //プレイヤー間の中央座標

    public CameraShake Shake;   //カメラを揺らすスクリプトを取得
    [SerializeField] PlayerHP Php;
    [SerializeField] PlayerHP Php2;

    [SerializeField, Header("振動時間")] float ShakeTime;
    [SerializeField, Header("振動量")] float ShakeMove;

    [SerializeField, Header("カメラ最大")] float Maxratio;

    [SerializeField, Header("ローカル座標での前方")] private Vector3 _foward = Vector3.forward;  //前方の基準となるローカル空間ベクトル

    private void Start()
    {
        Shake = GetComponent<CameraShake>();
    }
    private void Update()
    {
        //カメラをプレイヤーの間に移動する処理
        CenterPos = (Target1.transform.position + Target2.transform.position) / 2;
        VirtualTarget.transform.position = CenterPos;       //プレイヤーの中心に空白のオブジェクトを配置する


        //プレイヤー１を軸にしたCenterへのベクトル
        Vector3 Centervec = new Vector3(CenterPos.x - Target1.transform.position.x,0, CenterPos.z - Target1.transform.position.z);
        float Centerrotate = Mathf.Atan2(Centervec.z, Centervec.x);

        //transform.root.position = Target1.transform.position;
        //transform.root.rotation = new Quaternion(transform.root.rotation.x, transform.root.rotation.y+Centerrotate, transform.root.rotation.z, transform.root.rotation.w);

        //カメラとプレイヤーの距離
        ratio = Vector3.Distance(Target1.transform.position, Target2.transform.position);

        //カメラの拡大収縮制限
        if(ratio<10f)
        {
            ratio = 10f;
        }
        else if(ratio>Maxratio)
        {
            ratio = Maxratio;
        }

        transform.position = new Vector3(ratio*Mathf.Sin(Centerrotate)+CenterPos.x, CenterPos.y + OffcetY, -ratio*Mathf.Cos(Centerrotate)+CenterPos.z);


        Debug.Log(" Cos " + Mathf.Cos(Centerrotate) + " Sin " + Mathf.Sin(Centerrotate) + " プレイヤー間の中心位置の角度 " + Centerrotate+" プレイヤー間の中心位置x "+CenterPos.x+" z "+CenterPos.z);
        var dir = new Vector3(CenterPos.x - transform.position.x, 0f, CenterPos.z - transform.position.z);
        //ターゲットへの回転（CenterPos）
        var lookatRotation = Quaternion.LookRotation(dir, Vector3.up);
        //回転補正
        var offsetRotation = Quaternion.FromToRotation(_foward, Vector3.forward);
        transform.rotation = lookatRotation * offsetRotation;
        //カメラを揺らす処理
        if (Php._HitHardAttack == true || Php2._HitHardAttack == true)
        {
            Shake.Shake(ShakeTime, ShakeMove);
        }
    }
}

