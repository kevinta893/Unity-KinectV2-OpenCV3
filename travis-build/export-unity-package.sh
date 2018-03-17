#! /bin/sh

PROJECT_PATH=$(pwd)/Unity+KinectV2+OpenCV3
UNITY_BUILD_DIR=$(pwd)/Build
LOG_FILE=$UNITY_BUILD_DIR/unity-win.log
EXPORT_PATH=$(pwd)/Unity+KinectV2+OpenCV3-v"$TRAVIS_TAG"-build"$TRAVIS_BUILD_NUMBER".unitypackage

error_code=0

echo "Creating package at=$EXPORT_PATH"
mkdir $UNITY_BUILD_DIR
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile \
  -projectPath "$PROJECT_PATH" \
  -exportPackage "Assets" "$EXPORT_PATH" \
  -quit \
  | tee "$LOG_FILE"
  
if [ $? = 0 ] ; then
  echo "Created package successfully."
  ls
  error_code=0
else
  echo "Creating package failed. Exited with $?."
  ls
  error_code=1
fi

#echo 'Build logs:'
#cat $LOG_FILE

echo "Finishing with code $error_code"
exit $error_code