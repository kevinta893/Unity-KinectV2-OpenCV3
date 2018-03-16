#! /bin/sh

BASE_URL=https://beta.unity3d.com/download
HASH=ad31c9083c46
VERSION=2017.2.0f1

download() {
  file=$1
  url="$BASE_URL/$HASH/$package"

  echo "Downloading from $url: "
  curl -o "$package" "$url"
}

install() {
  package=$1
  download "$package"

  echo "Installing "$package""
  chmod 777 -c "$package"
  sudo apt -y install ./"$package"
}

# See main beta page: https://beta.unity3d.com/download/aea5ecb8f9fd+/public_download.html

install "unity-editor_amd64-$VERSION.deb"
