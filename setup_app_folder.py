import os

def read_file(fileName):
    f = open(fileName,"r")
    content = f.readlines()
    f.close()
    return content[0]

def write_json_file(fileName, jsonString = r"{}"):
    f = open(fileName,"w")
    f.write(jsonString)
    f.close()
    return True

def get_tlt_location():
    user_name = os.getlogin()
    base_path = "C:/Users/" + user_name + "/AppData/Local/Packages" #os.path.expanduser("~AppData\\Local\\Packages")
    current_location = os.getcwd().replace("\\","/")
    os.chdir(base_path)
    location = False
    sub_folders = os.listdir()
    for sub_folder in sub_folders:
        try:
            os.chdir(sub_folder)
            if "LocalState" in os.listdir('.'):
                os.chdir("LocalState")
                if "label.txt" in os.listdir('.'):
                    if "ThunderbirdLiveTile" in read_file("label.txt"):
                        location = os.getcwd()
                os.chdir("../")
            if location:
                os.chdir(current_location)
                return location
            os.chdir("../")
        except:
            continue
    os.chdir(current_location)
    return location

def setup_app_folder():
    current_location = os.getcwd().replace("\\","/")
    location = get_tlt_location()
    if location:
        os.chdir(location)
        if "ThunderbirdData" not in os.listdir():
            os.mkdir("ThunderbirdData")
        os.chdir(current_location)
    return location.replace("\\","/")

if __name__ == "__main__":
    print(setup_app_folder())