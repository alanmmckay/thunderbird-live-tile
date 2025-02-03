import os
import winapps

user_name = os.getlogin()

continue_bool = False
program_location = None
for program in winapps.search_installed("thunderbird"):
    if 'Mozilla Thunderbird' in program.name:
        continue_bool = True
        program_location = program.install_location

if continue_bool:

    email_address = input("Email Address: ")

    with open("background-base.js", "r", encoding="utf-8") as file:
        data = file.read()

    first_line = 'emailAccountName = "' + str(email_address) + '"\n'

    background_js_output = first_line + '\n' + data

    with open("native_host-base.py","r") as file:
        data = file.read()

    first_line = 'programLocation = "' +str(program_location) + '"\n'

    native_host_output = first_line + '\n' + data

    os.chdir(str(program_location))
    if not os.path.isdir('live-tile'):
        os.makedirs('live-tile')
    os.chdir('live-tile')

    with open("background.js","w") as file:
        file.write(background_js_output)

    with open("native_host.py", "w") as file:
        file.write(native_host_output)

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
            

    