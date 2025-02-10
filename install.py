import os
import winapps
import json
from setup_app_folder import setup_app_folder

installer_location = os.getcwd()
user_name = os.getlogin()

continue_bool = False
json_location = setup_app_folder()

program_location = "C:/Users/"+user_name+"/AppData/Local/Packages/thunderbird_live_tile"
        
os.chdir("C:/Users/"+user_name+"/AppData/Local/Packages")
if not os.path.isdir('thunderbird_live_tile'):
    os.mkdir('thunderbird_live_tile')

os.chdir(installer_location)

continue_bool = True
if continue_bool:

    email_address = input("Email Address: ")

    with open("background-base.js", "r", encoding="utf-8") as file:
        data = file.read()

    #Append email address to extension's javascript file:
    first_line = 'emailAccountName = "' + str(email_address) + '"\n'
    background_js_output = first_line + '\n' + data

    with open("native_host-base.py","r") as file:
        data = file.read()

    #Append program location to python native host file:
    first_line = 'jsonLocation = "' + json_location + '/ThunderbirdData"\n' #Realistically this should be labeled as jsonLocation
    native_host_output = first_line + '\n' + data

    with open("manifest.json","r") as file:
        manifest_output = file.read()

    with open("background.html", "r") as file:
        background_output = file.read()

    with open("thunderbird_live_tile.json","r") as file:
        reg_manifest = json.loads(file.read())

    reg_manifest['path'] = program_location + "/native_host.bat"

    os.chdir(program_location)

    with open("background.js","w") as file:
        file.write(background_js_output)

    with open("native_host.py", "w") as file:
        file.write(native_host_output)

    with open("manifest.json","w") as file:
        file.write(manifest_output)

    with open("background.html","w") as file:
        file.write(background_output)

    with open("thunderbird_live_tile.json","w") as file:
        json.dump(reg_manifest,file)

    bat_string = "@echo off\ncall python3 " + program_location + "/native_host.py"
    with open("native_host.bat","w") as file:
        file.write(bat_string)

    os.chdir(json_location + '/ThunderbirdData')

    with open("thunderbird_unread.json", "w") as file:
        file.write(str(dict()))

    with open("thunderbird-live-tile-extension.error.log", "w") as file:
        file.write("")
        
    #TODO: Need to place background.html and manifest.json into the same folder

    #It seems that placing thunderbird_live_tile.json into a folder doesn't inform Thunderbird that a native host exists. 
    #It seems that a registry edit needs to be made.
        #Resources: 
            #https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_manifests#manifest_location
            #https://www.geeksforgeeks.org/manipulating-windows-registry-using-winreg-in-python/
            
