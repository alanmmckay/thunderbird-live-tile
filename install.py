email_address = input("Email Address: ")

with open("background-base.js", "r", encoding="utf-8") as file:
    data = file.read()

first_line = 'emailAccountName = "' + str(email_address) + '"\n'

output = first_line + '\n' + data

with open("background.js","w") as file:
    file.write(output)

