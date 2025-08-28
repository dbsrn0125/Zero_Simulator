cmake_minimum_required(VERSION 3.8)
project(zenith_interfaces)

if(CMAKE_COMPILER_IS_GNUCXX OR CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

# 필요한 패키지를 찾습니다.
find_package(ament_cmake REQUIRED)
find_package(rosidl_default_generators REQUIRED)

# .srv 파일을 찾아서 C++/Python 코드를 생성하도록 지시합니다.
rosidl_generate_interfaces(${PROJECT_NAME}
  "srv/ChangeState.srv"
  "srv/StartLaunch.srv"
  "msg/LaunchResult.msg"
)

# 테스트 관련 설정
if(BUILD_TESTING)
  find_package(ament_lint_auto REQUIRED)
  ament_lint_auto_find_test_dependencies()
endif()

ament_package()