# Quickstart: Ticket Generator

This document describes the primary user flow for the Ticket Generator application.

## Prerequisites

1.  A valid `.NET` runtime is installed.
2.  The application (`ConsoleAppUserInterface.exe`) is built.
3.  An input file named `tasks.txt` exists in the same directory as the executable, with the following format per line: `Theme;Type;Complexity;Text`.

## Execution Flow

1.  **Start the Application**:
    -   Run `ConsoleAppUserInterface.exe` from the command line.

2.  **Main Menu**:
    -   The application will display the main menu:
        ```
        Ticket Generator
        ---
        1. Generate Tickets
        2. View Generated Tickets
        3. Exit
        ```

3.  **Generate Tickets**:
    -   Select option `1`.
    -   The application will prompt for the **target complexity**:
        ```
        Enter target complexity:
        ```
    -   Enter an integer value (e.g., `5`).
    -   The application will then prompt for the **allowed tolerance**:
        ```
        Enter complexity tolerance (%):
        ```
    -   Enter an integer value for the percentage (e.g., `10`).

4.  **Generation Process**:
    -   The application will read `tasks.txt`, perform the generation algorithm, and write results to `tickets.txt`.
    -   Upon completion (or stoppage), a message will be displayed:
        ```
        Generation complete. 25 tickets were created and saved to tickets.txt.
        ```
        OR
        ```
        Generation stopped. Reason: Not enough tasks of type 'Practice' to maintain the required ratio.
        ```

5.  **View Tickets**:
    -   From the main menu, select option `2`.
    -   The content of the `tickets.txt` file will be printed to the console.

6.  **Exit**:
    -   From the main menu, select option `3` to close the application.