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

echo "Export finished with code $error_code"
exit $error_code


#Preprare release unity package by packing into ZIP
VERSION_TAG="$TRAVIS_TAG"
PROJECT_NAME="Unity+KinectV2+OpenCV3"
RELEASE_DIRECTORY=$(pwd)/release
RELEASE_ZIP_FILE=$RELEASE_DIRECTORY/$PROJECT_NAME-v$VERSION_TAG.zip

mkdir -p $RELEASE_DIRECTORY

echo "Preparing release for version: $VERSION_TAG"
cp "$EXPORT_PATH" "$RELEASE_DIRECTORY/"`basename "$EXPORT_PATH"`
cp "./README.md" "$RELEASE_DIRECTORY"
cp "./LICENSE" "$RELEASE_DIRECTORY"

echo "Files in release directory:"
ls $RELEASE_DIRECTORY

zip -r $RELEASE_ZIP_FILE $RELEASE_DIRECTORY


echo "Release zip package ready. Zipinfo:"
zipinfo $RELEASE_ZIP_FILE
