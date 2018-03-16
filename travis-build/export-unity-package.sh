#! /bin/sh

project_path=$(pwd)/Unity+KinectV2+OpenCV3
log_file=$(pwd)/build/unity-win.log
export_path=$(pwd)/Unity+KinectV2+OpenCV3-v"$TRAVIS_TAG"-build"$TRAVIS_BUILD_NUMBER".unitypackage

error_code=0

echo "Creating package at=$export_path"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile "$log_file" \
  -projectPath "$project_path" \
  -exportPackage "Assets/" "$export_path" \
  -quit
if [ $? = 0 ] ; then
  echo "Created package successfully."
  error_code=0
else
  echo "Creating package failed. Exited with $?."
  error_code=1
fi

echo 'Build logs:'
cat $log_file

echo "Finishing with code $error_code"
exit $error_code