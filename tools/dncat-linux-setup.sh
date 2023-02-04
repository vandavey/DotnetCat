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
    cd "$ORIG_DIR" || exit 1
    exit 1
}

# Write a status message to stdout
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

# Check that 'curl' is installed
if [ -z "$(which curl)" ]; then
    error "Missing installation dependency 'curl'"
fi

# Check that 'unzip' is installed
if [ -z "$(which unzip)" ]; then
    error "Missing installation dependency 'unzip'"
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

INSTALL_DIR="/opt/dncat"
BIN_DIR="${INSTALL_DIR}/bin"
SHARE_DIR="${INSTALL_DIR}/share"

# Remove the existing installation
if [ -d $INSTALL_DIR ]; then
    status "Removing existing installation at '${INSTALL_DIR}'..."

    sudo rm -r $INSTALL_DIR > /dev/null || {
        error "Failed to remove existing installation at '${INSTALL_DIR}'"
    }
fi

# Create the installation directory
sudo mkdir $INSTALL_DIR > /dev/null || {
    error "Failed to create directory '${INSTALL_DIR}'"
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
ZIP_PATH="${INSTALL_DIR}/dncat.zip"

status "Downloading temporary application zip file to '${ZIP_PATH}'..."

# Download the temporary application zip file
sudo curl -s $ZIP_URL -o $ZIP_PATH > /dev/null || {
    error "Failed to download zip file from '${ZIP_URL}'"
}

status "Successfully downloaded zip file to '${ZIP_PATH}'"

# Navigate to installation directory before unpacking zip file
cd "${INSTALL_DIR}" || error "An unexpected error occurred"

status "Unpacking contents of temporary zip file '${ZIP_PATH}'..."

# Unpack the temporary zip file contents
sudo unzip -j $ZIP_PATH > /dev/null || {
    error "Failed to unpack contents of zip file '${ZIP_PATH}'"
}

status "Removing temporary zip file '${ZIP_PATH}'..."

# Remove the temporary zip file
sudo rm $ZIP_PATH > /dev/null || {
    error "Failed to remove temporary zip file '${ZIP_PATH}'"
}

status "Installing the application files to '${SHARE_DIR}'..."

# Move unpacked executable into the bin directory
sudo mv "${INSTALL_DIR}/dncat" $EXE_PATH > /dev/null || {
    error "Failed to restructure application files in '${INSTALL_DIR}'"
}

# Move license into the share directory
sudo mv "${INSTALL_DIR}/LICENSE.txt" $SHARE_DIR > /dev/null || {
    error "Failed to restructure application files in '${INSTALL_DIR}'"
}

# Enable application execute permssion
sudo chmod +x $EXE_PATH > /dev/null || {
    error "Failed to enable execute permission on file '${EXE_PATH}'"
}

# Add bin directory to the environment path
if ! grep -q $INSTALL_DIR <<< "$PATH"; then
    LINE="export PATH=\"\${PATH}:${BIN_DIR}\""

    # Avoid duplicate configuration lines
    if ! grep -q "$LINE" ~/.bashrc; then
        echo -e "\n${LINE}" | sudo tee -a ~/.bashrc > /dev/null
        status "Updated environment path configuration in '${HOME}/.bashrc'"
    fi

    # Avoid duplicate configuration lines
    if ! grep -q "$LINE" ~/.zshrc; then
        echo -e "\n${LINE}" | sudo tee -a ~/.zshrc > /dev/null
        status "Updated environment path configuration in '${HOME}/.zshrc'"
    fi
fi

status "DotnetCat was successfully installed, please restart your shell"
