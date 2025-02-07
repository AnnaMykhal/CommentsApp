#!/usr/bin/env bash
# wait-for-it.sh

# This script will wait until a given host and port are available.
# Usage: wait-for-it.sh <host>:<port> [--timeout=<timeout>] [--] <command> [args...]
# Example: wait-for-it.sh db:5432 -- echo "Database is ready"

TIMEOUT=15
WAIT_FOR_PORT="false"
while [[ $# -gt 0 ]]; do
  case $1 in
    --timeout=*)
      TIMEOUT="${1#*=}"
      shift
      ;;
    --)
      shift
      WAIT_FOR_PORT="true"
      break
      ;;
    *)
      HOSTPORT=$1
      shift
      ;;
  esac
done

# Check if a host and port are given
if [ -z "$HOSTPORT" ]; then
  echo "Usage: $0 <host>:<port> [--timeout=<timeout>] [--] <command> [args...]"
  exit 1
fi

# Parse host and port
HOST=${HOSTPORT%:*}
PORT=${HOSTPORT#*:}

# Start waiting for the host:port to be available
echo "Waiting for $HOST:$PORT to be available..."

# Loop to check if the port is open
SECONDS=0
while ! nc -z "$HOST" "$PORT"; do
  if [ "$SECONDS" -ge "$TIMEOUT" ]; then
    echo "Timeout waiting for $HOST:$PORT"
    exit 1
  fi
  sleep 1
done

echo "$HOST:$PORT is available!"

# Execute the command
if [ "$WAIT_FOR_PORT" = "true" ]; then
  exec "$@"
fi