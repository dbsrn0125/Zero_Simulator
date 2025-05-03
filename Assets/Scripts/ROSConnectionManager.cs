using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;//ROSConnection 사용 위해 필요
//ROS 연결을 관리하는 중앙관리자 역할(싱글톤)
public class ROSConnectionManager : MonoBehaviour
{
    //싱글톤 인스턴스
    public static ROSConnectionManager Instance { get; private set; }

    //ROSConnection 인스턴스에 쉽게 접근하기 위한 속성
    public ROSConnection Ros { get; private set; }

    private void Awake()
    {
        //singletone
        if(Instance==null)
        {
            Instance = this;
            //씬 변경되어도 이 GameObject가 파괴되지 않도록 설정
            //DontDestroyOnLoad(gameObject);
            Ros = ROSConnection.GetOrCreateInstance();//ROSConnection 인스턴스 가져오기
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
