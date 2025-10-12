# Feature Specification: Ticket Generator (MVP)

**Feature Branch**: `1-ticket-generator`
**Created**: 2025-10-12
**Status**: Draft
**Input**: User description: "Техническое задание (MVP): Генератор билетов (консольное приложение + библиотека классов на C#)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage Ticket Generation via Console Menu (Priority: P1)

As a teacher, I want to use a simple console menu to control the ticket generation process, so I can generate, view, and regenerate tickets as needed.

**Why this priority**: This defines the primary user interaction with the application.

**Independent Test**: The application starts and displays a menu. The user can select the "Generate" option, and the generation process described in User Story 2 is triggered.

**Acceptance Scenarios**:

1.  **Given** the application is started, **When** no action is taken, **Then** a console menu with options like "1. Generate Tickets", "2. View Generated Tickets", "3. Exit" is displayed.
2.  **Given** the menu is displayed, **When** the user selects the "Generate Tickets" option (regardless of whether tickets already exist), **Then** the application proceeds to ask for new generation parameters.
3.  **Given** tickets have been generated, **When** the user selects "View Generated Tickets", **Then** the content of `tickets.txt` is displayed in the console.

---

### User Story 2 - Generate and Save Tickets (Priority: P2)

As a teacher, I want to trigger the generation of a set of exam tickets from a list of tasks and have them saved to a file, so that I can prepare for an exam.

**Why this priority**: This is the core logic of the application, triggered by the menu.

**Independent Test**: When the generation function is called, it takes `tasks.txt` as input and produces a `tickets.txt` file with a non-zero number of tickets, following all generation rules.

**Acceptance Scenarios**:

1.  **Given** a valid `tasks.txt` file and user-provided parameters (target complexity, tolerance), **When** the generation process is run, **Then** a `tickets.txt` file is created and populated with a randomized but valid set of tickets, and the console reports the number of tickets created. Re-running the generation should produce a different set of tickets.
2.  **Given** an input file where it's impossible to meet the generation criteria, **When** the generation is run, **Then** the process stops, and the console displays a clear message explaining why.

## Clarifications

### Session 2025-10-12
- Q: How should the target complexity for ticket generation be calculated? → A: The user (teacher) manually enters the desired target complexity value at the start.
- Q: How should the program determine the ratio of task types for the tickets? → A: Calculate the ratio based on the overall count of each task type in the entire input file.
- Q: How should the program determine the fixed number of tasks that go into each ticket? → A: The number of tasks is the sum of the smallest integers that satisfy the calculated type ratio (e.g., a 2:1 ratio results in 3 tasks per ticket).
- Q: If tickets have already been generated and the user selects "1. Generate Tickets" again, what should the program do? → A: Immediately ask for new parameters and overwrite the old `tickets.txt` file upon successful generation.
- Q: How should the application handle semicolons that are part of the task's text? → A: For the MVP, it is assumed that the input `tasks.txt` file is always correctly formatted and task text does not contain semicolons.

## Requirements *(mandatory)*

### Functional Requirements

-   **FR-001**: System MUST read tasks from a text file (`tasks.txt`).
-   **FR-002**: Each line in the input file MUST represent a single task with the format: `Тема; Тип; Сложность; Текст задания`. It is assumed for the MVP that the task text itself will not contain semicolons.
-   **FR-003**: System MUST assign a unique integer ID to each task upon reading (e.g., based on line number).
-   **FR-004**: System MUST prompt the user (teacher) to input the **target complexity** and the **allowed complexity tolerance** (in percent) before generation.
-   **FR-005**: System MUST save the generated tickets to an output file (`tickets.txt`), overwriting any previous version of the file.
-   **FR-006**: The output format for a ticket in the file MUST be a single line: `Билет N; (ID) Тема; Тип; Сложность; Текст; (ID) ...`.
-   **FR-007**: System MUST stop generation if it's impossible to create a new ticket that satisfies all rules and inform the user of the reason.
-   **FR-008**: The ticket creation process MUST incorporate randomness to ensure that subsequent runs with the same input produce different (but still valid) sets of tickets.

### Generation Rules (Strict Priority Order)

-   **GR-A.1 (Ticket Complexity)**: The average arithmetic complexity of each ticket MUST be within a user-defined percentage tolerance of a target value. Generation stops if a new ticket cannot meet this criterion.
-   **GR-B.1 (Thematic Distribution)**: A ticket MUST NOT contain tasks that are all from the same theme. It must contain tasks from at least two different themes.
-   **GR-C.1 (Type Ratio & Task Count)**: All tickets MUST have a fixed number of tasks and the same ratio of task types. The ratio is determined by the program based on the overall count of each task type in the input file. The number of tasks per ticket is the smallest possible integer sum that satisfies this ratio (e.g., a 2:1 ratio means 3 tasks per ticket). All tasks within a single ticket MUST be unique (a task cannot appear more than once in the same ticket).
-   **GR-D.1 (Ticket Uniqueness)**: No two generated tickets can be identical.
    - A ticket is defined by the set of its task IDs; the order of tasks within a ticket does not matter for comparison.
    - While individual tasks can appear in multiple different tickets, a complete set of task IDs cannot be repeated.
    - The system must check for uniqueness before adding a newly generated ticket to the final list.

### Key Entities *(include if feature involves data)*

-   **Task**: Represents a single assignment with properties: `ID`, `Theme`, `Type`, `Complexity`, `Text`.
-   **Ticket**: Represents a collection of `Task` objects.

## Success Criteria *(mandatory)*

### Measurable Outcomes

-   **SC-001**: The application generates the maximum possible number of tickets that satisfy all generation rules from a given input file.
-   **SC-002**: Every ticket in the output `tickets.txt` file successfully passes validation against all specified generation rules (GR-A.1 to GR-D.1).
-   **SC-003**: When generation stops because a rule cannot be met, the application provides a clear, correct message to the user identifying the blocking rule.