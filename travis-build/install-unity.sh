#! /bin/sh





# See $BASE_URL/$HASH/unity-$VERSION-$PLATFORM.ini for complete list
# of available packages, where PLATFORM is `osx` or `win`
UNITY_DOWNLOAD_CACHE=$(pwd)/unity_download_cache
BASE_URL=https://beta.unity3d.com/download/46dda1414e51
VERSION=2017.2.0f3

UNITY_OSX_PACKAGE="MacEditorInstaller/Unity-$VERSION.pkg"
UNITY_WINDOWS_TARGET_PACKAGE="MacEditorTargetInstaller/UnitySetup-Windows-Support-for-Editor-$VERSION.pkg"


download() {
	
	mkdir "$UNITY_DOWNLOAD_CACHE"
	file=$1
	url="$BASE_URL/$file"

	echo "Downloading from $url: "
	curl -o `basename "$package"` "$url"
}

install() {
	package=$1

	echo "Installing "`basename "$package"`
	sudo installer -dumplog -package `basename "$package"` -target /
}




# Check if unity was already downloaded
if [ ! "$(ls -A $UNITY_DOWNLOAD_CACHE)" ]; then
	echo "Unity does not exist. Download and installing:"
	download "$UNITY_OSX_PACKAGE"
	download "$UNITY_WINDOWS_TARGET_PACKAGE"
else
	echo "Unity Exists. Will not redownload. Proceeding to install from cache..."
fi

echo "Installing Unity..."
install "$UNITY_OSX_PACKAGE"
install "$UNITY_WINDOWS_TARGET_PACKAGE"

