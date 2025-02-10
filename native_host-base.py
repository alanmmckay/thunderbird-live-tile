import sys
import json
import os
import struct
#https://github.com/mdn/webextensions-examples/blob/main/native-messaging/app/ping_pong.py
#https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_messaging
#https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_manifests#manifest_location


json_data_file = jsonLocation + "/thunderbird_unread.json"
error_file = jsonLocation + "/thunderbird-live-tile-extension.error.log"

def write_data(data):
    with open(json_data_file, "w", encoding="utf-8") as file:
        json.dump(data, file, indent = 2)

def read_message():
    rawLength = sys.stdin.buffer.read(4)
    if len(rawLength) == 0:
        sys.exit(0)
    messageLength = struct.unpack('@I', rawLength)[0]
    message = sys.stdin.buffer.read(messageLength).decode('utf-8')
    return json.loads(message)


if __name__ == "__main__":
    while True:
        try:
            message = read_message()
            if message:
                write_data(message)
        except Exception as e:
            with open(error_file, "a") as error_file:
                error_file.write(str(e) + "\n")
