#! /bin/sh

BASE_URL=https://beta.unity3d.com/download
HASH=46dda1414e51
VERSION=2017.2.0f3

download() {
  file=$1
  url="$BASE_URL/$HASH/$package"

  echo "Downloading from $url: "
  curl -o `basename "$package"` "$url"
}

install() {
  package=$1
  download "$package"

  echo "Installing "`basename "$package"`
  sudo installer -dumplog -package `basename "$package"` -target /
}

# See $BASE_URL/$HASH/unity-$VERSION-$PLATFORM.ini for complete list
# of available packages, where PLATFORM is `osx` or `win`


FILE=""
DIR="/Applications/Unity/"
# init
# look for empty dir 
if [ "$(ls -A $DIR)" ]; then
     echo "Unity Exists. Will not redownload and install. Clear cache for clean install"
else
    echo "Unity does not exist. Download and installing:"
	install "MacEditorInstaller/Unity-$VERSION.pkg"
	install "MacEditorTargetInstaller/UnitySetup-Windows-Support-for-Editor-$VERSION.pkg"
fi

