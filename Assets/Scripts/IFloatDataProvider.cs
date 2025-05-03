//데이터를 제공하는 객체가 구현해야 할 인터페이스 정의
//double 타입의 데이터를 반환하는 GetDat()메서드 강제
public interface IFloatDataProvider
{
    //ROS의 Float64 메세지가 double 타입이므로 double 반환 권장
    double GetData();
}