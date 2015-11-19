#!/bin/bash

# dependencies
apt-get update -y 
apt-get -qq update && apt-get -qqy install unzip curl libicu-dev libunwind8 gettext libssl-dev libcurl3-gnutls zlib1g && rm -rf /var/lib/apt/lists/*

# setup .NET core - installs latest DNX and sets it to default
curl -sSL https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.sh | DNX_BRANCH=v1.0.0-rc1-final sh
source ~/.dnx/dnvm/dnvm.sh
dnvm install 1.0.0-rc1-final -alias default -r coreclr 