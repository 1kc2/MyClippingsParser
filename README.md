# MyClippingsParser

A parser to make the Kindle `My Clippings.txt` file more usable.

This parser will take your `My Clippings.txt` file, remove duplicates (like those created when changing a highlight when reading), split them up by book, and output them in two formats (split up by page number, and all in one file).

# Usage

- Clone this repository

- Place `My Clippings.txt` in the project's root directory
    ```
    .
    ├── My Clippings.txt
    ├── MyClippingsParser.csproj
    ├── Program.cs
    ├── bin/
    └── obj/
    ```

- Run the program
    ```
    $ dotnet run
    ```
