from platform import system
from sys import exit
import subprocess
import uuid
import os
import requests
import hashlib

def auth() -> None:
    # 11 - id of Premium
    # 12 - id of Supreme
    # 93 - id of Infinity
    premiumPlus = ['11', '12', '93', '96', '97', '99', '100', '101', '4', '3', '6', '94', '92']
 
    systemID = subprocess.check_output('wmic csproduct get uuid').decode().split('\n')[1].strip()
    procID = subprocess.check_output('wmic cpu get ProcessorId').decode().split('\n')[1].strip()
    
    hwid = hashlib.sha256((procID + systemID).encode('utf-8')).hexdigest()
    
    if os.path.isfile('key.txt'):
        with open('key.txt') as f:
            auth_key = f.read().strip()
    else:
        auth_key = input('Enter Cracked.to Auth Key: ')
        with open('key.txt', 'w') as f:
            f.write(auth_key)
        
    data = {
        "a": "auth",
        "k": auth_key,
        "hwid": hwid
    }
 
    r = requests.post('https://cracked.io/auth.php', data=data)
    if r.ok:
        res = r.json()
 
        if 'error' in res:
             print(res["error"])
             if (os.path.exists("key.txt")):
                 os.remove("key.txt")
             exit(1)
        elif res['auth']:
            #Upgraded users check starts here
                if any(res['group'] in s for s in premiumPlus):
                    print(f'Auth Granted. Welcome {res["username"]}!')
                else:
                    print('You have to be at least Premium+ to be able to use this tool.')
                    exit()
            #Upgraded users check ends here
        else:
            print(res)
            exit(1)
    else:
        print(r.text, r.status_code)
        exit(1)
