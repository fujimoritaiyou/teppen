/*
 �쐬�ҁF��c
 ���̃X�N���v�g�͊�{�G��Ȃ��悤�ɂ��Ă��������I�I�I
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    //�v���C���[�I�u�W�F�N�g������ϐ�
    public GameObject Target1;
    public GameObject Target2;
    public GameObject VirtualTarget;

    Vector3 CenterPos;      //�v���C���[�Ԃ̒������W
    public float OffcetY = 4;   //�J������Y���W
    public float ratio;     //�J�����̔䗦
    public float Rot;       //�v���C���[�Ԃ̒������W

    public CameraShake Shake;   //�J������h�炷�X�N���v�g���擾
    [SerializeField] PlayerHP Php;
    [SerializeField] PlayerHP Php2;

    [SerializeField, Header("�U������")] float ShakeTime;
    [SerializeField, Header("�U����")] float ShakeMove;

    [SerializeField, Header("�J�����ő�")] float Maxratio;

    [SerializeField, Header("���[�J�����W�ł̑O��")] private Vector3 _foward = Vector3.forward;  //�O���̊�ƂȂ郍�[�J����ԃx�N�g��

    private void Start()
    {
        Shake = GetComponent<CameraShake>();
    }
    private void Update()
    {
        //�J�������v���C���[�̊ԂɈړ����鏈��
        CenterPos = (Target1.transform.position + Target2.transform.position) / 2;
        VirtualTarget.transform.position = CenterPos;       //�v���C���[�̒��S�ɋ󔒂̃I�u�W�F�N�g��z�u����


        //�v���C���[�P�����ɂ���Center�ւ̃x�N�g��
        Vector3 Centervec = new Vector3(CenterPos.x - Target1.transform.position.x,0, CenterPos.z - Target1.transform.position.z);
        float Centerrotate = Mathf.Atan2(Centervec.z, Centervec.x);

        //transform.root.position = Target1.transform.position;
        //transform.root.rotation = new Quaternion(transform.root.rotation.x, transform.root.rotation.y+Centerrotate, transform.root.rotation.z, transform.root.rotation.w);

        //�J�����ƃv���C���[�̋���
        ratio = Vector3.Distance(Target1.transform.position, Target2.transform.position);

        //�J�����̊g����k����
        if(ratio<10f)
        {
            ratio = 10f;
        }
        else if(ratio>Maxratio)
        {
            ratio = Maxratio;
        }

        transform.position = new Vector3(ratio*Mathf.Sin(Centerrotate)+CenterPos.x, CenterPos.y + OffcetY, -ratio*Mathf.Cos(Centerrotate)+CenterPos.z);


        Debug.Log(" Cos " + Mathf.Cos(Centerrotate) + " Sin " + Mathf.Sin(Centerrotate) + " �v���C���[�Ԃ̒��S�ʒu�̊p�x " + Centerrotate+" �v���C���[�Ԃ̒��S�ʒux "+CenterPos.x+" z "+CenterPos.z);
        var dir = new Vector3(CenterPos.x - transform.position.x, 0f, CenterPos.z - transform.position.z);
        //�^�[�Q�b�g�ւ̉�]�iCenterPos�j
        var lookatRotation = Quaternion.LookRotation(dir, Vector3.up);
        //��]�␳
        var offsetRotation = Quaternion.FromToRotation(_foward, Vector3.forward);
        transform.rotation = lookatRotation * offsetRotation;
        //�J������h�炷����
        if (Php._HitHardAttack == true || Php2._HitHardAttack == true)
        {
            Shake.Shake(ShakeTime, ShakeMove);
        }
    }
}

