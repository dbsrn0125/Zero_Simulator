public interface ISetpointStateProvider
{
    double GetTargetAngle();//목표 각도 반환
    double GetCurrentAngle();//현재 각도 반환
}
