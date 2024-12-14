#!/bin/bash
#
#  dncat-uninstall.sh
#  ------------------
#  DotnetCat uninstaller script for ARM64 and x64 Linux systems.

APP_DIR="/opt/dncat"
BIN_DIR="${APP_DIR}/bin"

# Write an error message to stderr and exit.
error() {
    echo -e "\033[91m[x]\033[0m ${*}" >&2
    exit 1
}

# Write a status message to stdout.
status() {
    echo -e "\033[96m[*]\033[0m ${*}"
}

# Remove the bin directory environment path export from a file.
remove_bin_export() {
    local line_num
    local line="export PATH=\"\${PATH}:${BIN_DIR}\""

    if [[ -f $1 ]] && grep -q "${line}" "${1}"; then
        line_num=$(cat -n "${1}" | grep "${line}" | awk '{print $1}')

        if [[ -n $line_num ]]; then
            if ! sed -i "${line_num}d" "${1}"; then
                error "Failed to remove path export at '${1}':${line_num}"
            fi
            status "Removed environment path export at '${1}':${line_num}"
        fi
    fi
}

ARCH=$(uname -m)

# Validate CPU architecture
if [[ ! $ARCH =~ ^(aarch64|x86_64)$ ]]; then
    error "Unsupported processor architecture: '${ARCH}'"
fi

# Require elevated shell privileges
if ! sudo -n true 2> /dev/null; then
    status "Elevated shell privileges required..."

    if ! sudo -v; then
        error "Failed to elevate shell privileges"
    fi
fi

# Remove all application files
if [[ -d $APP_DIR ]]; then
    status "Removing application files from '${APP_DIR}'..."

    if ! sudo rm -rv $APP_DIR; then
        error "Failed to remove application files from '${APP_DIR}'"
    fi
else
    status "No application files to remove from '${APP_DIR}'"
fi

remove_bin_export ~/.bashrc
remove_bin_export ~/.zshrc

status "DotnetCat was successfully uninstalled"
