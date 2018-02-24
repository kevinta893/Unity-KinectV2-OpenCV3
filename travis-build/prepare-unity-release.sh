#! /bin/sh

version=$1

project="Unity+KinectV2+OpenCV3"
package_path=$(pwd)/current-package/$project.unitypackage
release_directory=$(pwd)/release
release_path=$release_directory/$project-$version.unitypackage

mkdir -p $release_directory

echo "Preparing release version $version."
cp "$package_path" "$release_path"
cp "./README.md" "$release_directory"
cp "./LICENSE" "$release_directory"
zip -r $release_directory/$project-v$version.zip $release_directory