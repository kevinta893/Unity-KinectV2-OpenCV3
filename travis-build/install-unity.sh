#! /bin/sh





# See $BASE_URL/$HASH/unity-$VERSION-$PLATFORM.ini for complete list
# of available packages, where PLATFORM is `osx` or `win`
UNITY_DOWNLOAD_CACHE=$(pwd)/unity_download_cache
BASE_URL=https://beta.unity3d.com/download/46dda1414e51
VERSION=2017.2.0f3

UNITY_OSX_PACKAGE="MacEditorInstaller/Unity-$VERSION.pkg"
UNITY_WINDOWS_TARGET_PACKAGE="MacEditorTargetInstaller/UnitySetup-Windows-Support-for-Editor-$VERSION.pkg"


download() {
	
	file=$1
	url="$BASE_URL/$file"

	echo "Downloading from $url: "
	curl -o $UNITY_DOWNLOAD_CACHE/`basename "$file"` "$url"
}

install() {
	package=$1

	echo "Installing "`basename "$package"`
	sudo installer -dumplog -package $UNITY_DOWNLOAD_CACHE/`basename "$package"` -target /
}




# Check if unity was already downloaded, clear the cache if missing modules or need clean install
if [test "$(ls -A "$target")"]; then
	echo "Unity download cache does not exist. Download and installing:"
	rm -rf "$UNITY_DOWNLOAD_CACHE"
	mkdir "$UNITY_DOWNLOAD_CACHE"
	download "$UNITY_OSX_PACKAGE"
	download "$UNITY_WINDOWS_TARGET_PACKAGE"
else
	echo "Unity Exists. Will not redownload. Cache Contents:"
	ls "$UNITY_DOWNLOAD_CACHE"
	echo "Proceeding to install from cache..."
fi

echo "Installing Unity..."
install "$UNITY_OSX_PACKAGE"
install "$UNITY_WINDOWS_TARGET_PACKAGE"

