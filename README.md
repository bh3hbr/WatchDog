# Watchdog Process Monitor

A simple watchdog application to monitor and restart a specified process if it's not running.

## Table of Contents

1. [Overview](#overview)
2. [Usage](#usage)

## Overview

The Watchdog Process Monitor is designed to keep an eye on a specific process and restart it if it stops unexpectedly. It's particularly useful for ensuring that critical background processes continue to run without interruption.

## Usage

To use the Watchdog Process Monitor, you need to provide the process name, process path, and optionally, the check interval. Here's how to run the application:

```bash
WatchdogApp.exe <processName> <processPath> [<checkInterval>]
