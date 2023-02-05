#!/bin/bash
#
#  dncat-linux-setup.sh
#  --------------------
#  DotnetCat installer script for ARM64 and x64 Linux systems
#
ORIG_DIR=$PWD

# Write an error message to stderr and exit
error() {
    echo -e "\033[91m[x]\033[0m ${*}" > /dev/stderr
    cd "$ORIG_DIR" && exit 1 || exit 1
}

# Write a status message to stdout
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

# Check that 'curl' is installed
if ! command -v curl &> /dev/null; then
    error "Missing installer dependency 'curl'"
fi

# Check that 'unzip' is installed
if ! command -v unzip &> /dev/null; then
    error "Missing installer dependency 'unzip'"
fi

ZIP_URL=
ARCH=$(uname -m)

REPO_ROOT="https://raw.githubusercontent.com/vandavey/DotnetCat/master"

# Assign zip file URL and validate CPU architecture
if [ "$ARCH" == "aarch64" ]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_Linux-arm64.zip"
elif [ "$ARCH" == "x86_64" ]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_Linux-x64.zip"
else
    error "Unsupported processor architecture: '${ARCH}'"
fi

APP_DIR="/opt/dncat"
BIN_DIR="${APP_DIR}/bin"
SHARE_DIR="${APP_DIR}/share"

# Remove the existing installation
if [ -d $APP_DIR ]; then
    status "Removing existing installation from '${APP_DIR}'..."

    sudo rm -r $APP_DIR > /dev/null || {
        error "Failed to remove existing installation from '${APP_DIR}'"
    }
fi

# Create the installation directory
sudo mkdir $APP_DIR > /dev/null || {
    error "Failed to create directory '${APP_DIR}'"
}

# Create the bin directory
sudo mkdir -p $BIN_DIR > /dev/null || {
    error "Failed to create directory '${BIN_DIR}'"
}

# Create the share directory
sudo mkdir -p $SHARE_DIR > /dev/null || {
    error "Failed to create directory '${SHARE_DIR}'"
}

EXE_PATH="${BIN_DIR}/dncat"
ZIP_PATH="${APP_DIR}/dncat.zip"

status "Downloading temporary zip file to '${ZIP_PATH}'..."

# Download the temporary application zip file
sudo curl -sLS --ssl $ZIP_URL -o $ZIP_PATH > /dev/null || {
    error "Failed to download zip file from '${ZIP_URL}'"
}

# Navigate to installation directory before unpacking zip file
cd "$APP_DIR" || error "An unexpected error occurred"

status "Unpacking '${ZIP_PATH}' contents to '${APP_DIR}'..."

# Unpack the temporary zip file contents
sudo unzip -j $ZIP_PATH > /dev/null || {
    error "Failed to unpack contents of zip file '${ZIP_PATH}'"
}

status "Deleting temporary zip file '${ZIP_PATH}'..."

# Remove the temporary zip file
sudo rm $ZIP_PATH > /dev/null || {
    error "Failed to remove temporary zip file '${ZIP_PATH}'"
}

status "Installing application files to '${APP_DIR}'..."

# Move unpacked executable into the bin directory
sudo mv "${APP_DIR}/dncat" $EXE_PATH > /dev/null || {
    error "Failed to restructure application files in '${APP_DIR}'"
}

# Move license into the share directory
sudo mv "${APP_DIR}/LICENSE.txt" $SHARE_DIR > /dev/null || {
    error "Failed to restructure application files in '${APP_DIR}'"
}

# Enable application execute permission
sudo chmod +x $EXE_PATH > /dev/null || {
    error "Failed to enable execute permission on file '${EXE_PATH}'"
}

# Add bin directory to the environment path
if ! grep -q $APP_DIR <<< "$PATH"; then
    LINE="export PATH=\"\${PATH}:${BIN_DIR}\""

    # Avoid duplicate bash configuration lines
    if ! grep -q "$LINE" ~/.bashrc; then
        echo -e "\n${LINE}" | sudo tee -a ~/.bashrc > /dev/null
        status "Updated environment path configuration in '${HOME}/.bashrc'"
    fi

    # Avoid duplicate zsh configuration lines
    if ! grep -q "$LINE" ~/.zshrc; then
        echo -e "\n${LINE}" | sudo tee -a ~/.zshrc > /dev/null
        status "Updated environment path configuration in '${HOME}/.zshrc'"
    fi
else
    status "The local environment path already contains '${APP_DIR}'"
fi

status "DotnetCat was successfully installed, please restart your shell"
