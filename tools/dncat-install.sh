#!/bin/bash
#
#  dncat-install.sh
#  ----------------
#  DotnetCat installer script for ARM64 and x64 Linux systems.

APP_DIR="/opt/dncat"
BIN_DIR="${APP_DIR}/bin"
SHARE_DIR="${APP_DIR}/share"

# Write an error message to stderr and exit.
error() {
    echo -e "\033[91m[x]\033[0m ${*}" >&2
    exit 1
}

# Write a status message to stdout.
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

# Add the bin directory environment path export to a file.
add_bin_export() {
    local line_num
    local line="export PATH=\"\${PATH}:${BIN_DIR}\""

    if [[ -f $1 ]] && ! grep -q "${line}" "${1}"; then
        echo "${line}" >> "${1}"
        line_num=$(wc -l "${1}" | cut -d " " -f 1)

        status "Added environment path export at '${1}':${line_num}"
    fi
}

# Move application files to a new directory.
move_app_files() {
   local files=("${@:1:$#-1}")

    if ! sudo mv -v "${files[@]}" "${!#}"; then
        error "Failed to move application files to '${!#}'"
    fi
}

# Validate that an installer dependency is satisfied.
validate_dep() {
    if ! command -v "${1}" &> /dev/null; then
        error "Unsatisfied installer dependency: '${1}'"
    fi
}

ARCH=$(uname -m)
REPO_ROOT="https://raw.githubusercontent.com/vandavey/DotnetCat/master"

# Validate CPU architecture and set variables
if [[ $ARCH == "aarch64" ]]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_linux-arm64.zip"
elif [[ $ARCH == "x86_64" ]]; then
    ZIP_URL="${REPO_ROOT}/src/DotnetCat/bin/Zips/DotnetCat_linux-x64.zip"
else
    error "Unsupported processor architecture: '${ARCH}'"
fi

validate_dep 7z
validate_dep curl

# Require elevated shell privileges
if ! sudo -n true 2> /dev/null; then
    status "Elevated shell privileges required..."

    if ! sudo -v; then
        error "Failed to elevate shell privileges"
    fi
fi

# Remove existing installation
if [[ -d $APP_DIR ]]; then
    status "Removing existing installation..."

    if ! sudo rm -rv $APP_DIR; then
        error "Failed to remove existing installation from '${APP_DIR}'"
    fi
fi

status "Creating install directories..."

# Create install directories
if ! sudo mkdir -pv $BIN_DIR $SHARE_DIR; then
    error "Failed to create one or more directories in '${APP_DIR}'"
fi

ZIP_PATH="${APP_DIR}/dncat.zip"
HTTP_STATUS=$(curl -sILSw "%{http_code}" $ZIP_URL -o /dev/null)

# Failed to access download URL
if [[ $HTTP_STATUS -ne 200 ]]; then
    error "Unable to download zip file: HTTP ${HTTP_STATUS}"
fi

status "Downloading temporary zip file to '${ZIP_PATH}'..."

# Download application zip file
if ! sudo curl -sLS $ZIP_URL -o $ZIP_PATH; then
    error "Failed to download zip file from '${ZIP_URL}'"
fi

status "Unpacking zip file to '${APP_DIR}'..."

# Unpack application zip file
if ! sudo 7z x "${ZIP_PATH}" -bb0 -bd -o"${APP_DIR}" > /dev/null; then
    error "Failed to unpack zip file '${ZIP_PATH}'"
fi

status "Deleting temporary zip file..."

# Remove application zip file
if ! sudo rm -v $ZIP_PATH; then
    error "Failed to remove zip file '${ZIP_PATH}'"
fi

status "Installing application files to '${APP_DIR}'..."

move_app_files $APP_DIR/*.md $SHARE_DIR
move_app_files $APP_DIR/{dncat,*.sh} $BIN_DIR

status "Enabling execution of files in '${BIN_DIR}'..."

# Enable execute permissions
if ! sudo chmod +x $BIN_DIR/{dncat,*.sh}; then
    error "Failed to enable execution of files in '${BIN_DIR}'"
fi

# Create bash configuration file
if [[ ! -f ~/.bashrc && ! -f ~/.zshrc ]]; then
    status "Creating bash configuration file '${HOME}/.bashrc'..."
    : >> ~/.bashrc
fi

add_bin_export ~/.bashrc
add_bin_export ~/.zshrc

status "DotnetCat was successfully installed, please restart your shell"
