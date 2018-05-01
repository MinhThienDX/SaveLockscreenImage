#!/bin/bash

set -e;

echo "Commit: ${TRAVIS_COMMIT}";
CHANGED_FILES=`git diff --name-only ${TRAVIS_BRANCH}...${TRAVIS_COMMIT}`;
TERMINATE=True;
MD=".md";
YML=".yml";
TRAVIS=".travis.yml";

echo "Committed files being processed:";

# Loop all committed files
for CHANGED_FILE in $CHANGED_FILES; do

    echo $CHANGED_FILE;

    # Yaml file
    if [[ $CHANGED_FILE =~ $YML ]]; then

        # Travis's Yaml file
        if [[ $CHANGED_FILE == $TRAVIS ]]; then
            echo "Won't skip build";
            TERMINATE=False;
            break;
        fi;

    # Not Yaml file and not .md file
    elif ! [[ $CHANGED_FILE =~ $MD ]]; then
        echo "Can't skip build";
        TERMINATE=False;
        break;
    fi;

# End loop
done;

# Skip build
if [[ $TERMINATE == True ]]; then
    echo "Skip build in Travis";
    travis_terminate 0;
    exit 1;
else
    echo "Build in Travis";
fi;