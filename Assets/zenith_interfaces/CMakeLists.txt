cmake_minimum_required(VERSION 3.5)
project(zenith_interfaces)

find_package(ament_cmake REQUIRED)
find_package(rosidl_default_generators REQUIRED)

rosidl_generate_interfaces(${PROJECT_NAME}
  "srv/ChangeState.srv"
)


ament_package()
