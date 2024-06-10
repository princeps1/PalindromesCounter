# Palindrome Word Count Web Server

## Overview

This repository contains two mini projects developed using the .NET framework, both addressing the same problem. The first project solves the problem using threads, while the second project solves the problem using tasks. Both projects implement a server that accepts multiple simultaneous users and a cache memory for storing data about already opened files.

## Project Descriptions

### Thread-Based Implementation

This project uses threading to handle multiple simultaneous user requests.

### Task-Based Implementation

This project uses tasks to handle multiple simultaneous user requests.

## Problem Description

### [SERBIAN]

Kreirati Web server koji vrši brojanje reči u okviru fajla. Brojati samo reči koje su palindromi. Svi zahtevi serveru se šalju preko browser-a korišćenjem GET metode. U zahtevu se kao parametar navodi naziv fajla. Server prihvata zahtev, pretražuje root folder i sve njegove podfoldere za zahtevani fajl i vrši brojanje. Ukoliko traženi fajl ne postoji, vratiti grešku korisniku. Takođe, ukoliko nema reči koje su palindromi, vratiti odgovarajuću poruku korisniku.

### [ENGLISH]

Create a web server that counts palindrome words within a file. Only count words that are palindromes. All requests to the server are sent via the browser using the GET method. The request specifies the filename as a parameter. The server accepts the request, searches the root folder and all its subfolders for the requested file, and performs the counting. If the requested file does not exist, return an error to the user. Also, if there are no palindrome words, return an appropriate message to the user.

## Installation

1. Clone the repository to your local machine.

   ```bash
   git clone https://github.com/princeps1/PalindromesCounter.git
   ```

2. Open the solution in Visual Studio or your preferred .NET IDE.

3. Restore the required packages.

   ```bash
   dotnet restore
   ```

4. Build the project.
   ```bash
   dotnet build
   ```

## Running the Server

1. Run the project from your IDE or use the following command:

   ```bash
   dotnet run --project Palindrom.csproj
   ```

2. Access the server through your browser:
   ```
   http://localhost:5000/?filename=yourfile.txt or open the python script included in every mini project.
   ```

## Usage

- To count palindrome words within a file, send a GET request to the server with the filename as a parameter.

  ```
  http://localhost:5000/?filename=yourfile.txt
  ```

- If the file does not exist, you will receive an error message.

- If there are no palindrome words in the file, you will receive an appropriate message.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository.
2. Create a new branch.
   ```bash
   git checkout -b feature-branch
   ```
3. Make your changes and commit them.
   ```bash
   git commit -m "Description of changes"
   ```
4. Push to the branch.
   ```bash
   git push origin feature-branch
   ```
5. Create a new Pull Request.

## Contact

For any questions or suggestions, please open an issue or contact me at matejajovic2002@gmail.com.
