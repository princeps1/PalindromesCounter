import webbrowser
import threading

def otvori_url(url):
    webbrowser.open_new_tab(url)

def main():
    base_url = 'http://localhost:5050/fajl'
    urls = [f'{base_url}{i}.txt' for i in range(1,6)]

    threads = []
    for url in urls:
        thread = threading.Thread(target=otvori_url, args=(url,))
        thread.start()
        threads.append(thread)

    # Čekamo da se svi tabovi otvore
    for thread in threads:
        thread.join()

if __name__ == "__main__":
    main()
