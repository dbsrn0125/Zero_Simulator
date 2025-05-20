using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;//ROSConnection ��� ���� �ʿ�
//ROS ������ �����ϴ� �߾Ӱ����� ����(�̱���)
public class ROSConnectionManager : MonoBehaviour
{
    //�̱��� �ν��Ͻ�
    public static ROSConnectionManager Instance { get; private set; }

    //ROSConnection �ν��Ͻ��� ���� �����ϱ� ���� �Ӽ�
    public ROSConnection Ros { get; private set; }

    private void Awake()
    {
        //singletone
        if(Instance==null)
        {
            Instance = this;
            //�� ����Ǿ �� GameObject�� �ı����� �ʵ��� ����
            //DontDestroyOnLoad(gameObject);
            Ros = ROSConnection.GetOrCreateInstance();//ROSConnection �ν��Ͻ� ��������
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
