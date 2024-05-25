import webbrowser
import time

def otvori_url(url):
    webbrowser.open_new_tab(url)

def main():

    base_url = 'http://localhost:5050/fajl'
    urls = [f'{base_url}{i}.txt' for i in range(1,6)]


    for url in urls:
        otvori_url(url)

if __name__ == "__main__":
    main()
