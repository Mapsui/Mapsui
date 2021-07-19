#/bin/sh

# remove 'bin' and 'obj' directories
find . -type d \( -name 'bin' -o -name 'obj' \) -prune -exec rm -rf {} \;

# remove Resource.designer.cs files
find . -name 'Resource.designer.cs' -exec rm -rf {} \;
