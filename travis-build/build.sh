#! /bin/sh

project_path=$(pwd)/Unity+KinectV2+OpenCV3
log_file=$(pwd)/build/unity-win.log

error_code=0
echo "Items in project path ($project_path):"
ls "$project_path"
echo "Building project for Windows."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile \
  -projectPath "$project_path" \
  -buildWindows64Player  "$(pwd)/build/win/ci-build.exe" \
  -quit \
  |& tee "$log_file"
if [ $? = 0 ] ; then
  echo "Building Windows exe completed successfully."
  error_code=0
else
  echo "Building Windows exe failed. Exited with $?."
  error_code=1
fi

echo 'Build logs:'
cat $log_file

echo "Finishing with code $error_code"
exit $error_code