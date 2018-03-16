#! /bin/sh

project_path=$(pwd)/Unity+KinectV2+OpenCV3
log_file=$(pwd)/build/unity-linux.log

error_code=0
echo "Items in project path ($project_path):"
ls "$project_path"
echo "Building project for Mac OS."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$project_path" \
  -buildLinux64Player  "$(pwd)/build/osx/ci-build.app" \
  -quit
if [ $? = 0 ] ; then
  echo "Building Mac OS completed successfully."
  error_code=0
else
  echo "Building Mac OS failed. Exited with $?."
  error_code=1
fi

echo 'Build logs:'
cat $log_file

echo "Finishing with code $error_code"
exit $error_code