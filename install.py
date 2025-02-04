import os
import winapps
import json

installer_location = os.getcwd()
user_name = os.getlogin()


continue_bool = False
program_location = None
for program in winapps.search_installed("thunderbird"):
    if 'Mozilla Thunderbird' in program.name:
        continue_bool = True
        program_location = r"C:\\Users\\"+user_name+r"\\thunderbird_live_tile"
        
        os.chdir("C:\\Users\\"+user_name)
        if not os.path.isdir('thunderbird_live_tile'):
            os.makedirs('thunderbird_live_tile')
        break

os.chdir(installer_location)

if continue_bool:

    email_address = input("Email Address: ")

    with open("background-base.js", "r", encoding="utf-8") as file:
        data = file.read()

    first_line = 'emailAccountName = "' + str(email_address) + '"\n'

    background_js_output = first_line + '\n' + data

    with open("native_host-base.py","r") as file:
        data = file.read()

    first_line = 'programLocation = "' + program_location + '"\n'

    native_host_output = first_line + '\n' + data

    with open("manifest.json","r") as file:
        manifest_output = file.read()

    with open("background.html", "r") as file:
        background_output = file.read()

    with open("thunderbird_live_tile.json","r") as file:
        reg_manifest = json.loads(file.read())

    reg_manifest['path'] = "C:\\Users\\"+user_name+"\\thunderbird_live_tile\\native_host.bat"
   
    os.chdir(program_location)

    with open("background.js","w") as file:
        file.write(background_js_output)

    with open("native_host.py", "w") as file:
        file.write(native_host_output)

    with open("thunderbird_unread.json", "w") as file:
        file.write(str(dict()))

    with open("thunderbird-live-tile-extension.error.log", "w") as file:
        file.write("")

    with open("manifest.json","w") as file:
        file.write(manifest_output)

    with open("background.html","w") as file:
        file.write(background_output)

    with open("thunderbird_live_tile.json","w") as file:
        json.dump(reg_manifest,file)

    bat_string = "@echo off\ncall python3 C:\\Users\\"+user_name+"\\thunderbird_live_tile\\native_host.py"
    with open("native_host.bat","w") as file:
        file.write(bat_string)

    #TODO: Need to place background.html and manifest.json into the same folder

    #It seems that placing thunderbird_live_tile.json into a folder doesn't inform Thunderbird that a native host exists. 
    #It seems that a registry edit needs to be made.
        #Resources: 
            #https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_manifests#manifest_location
            #https://www.geeksforgeeks.org/manipulating-windows-registry-using-winreg-in-python/
            
