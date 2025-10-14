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

### Session 2025-10-14
- Q: How should the "optimal" task type ratio be determined? → A: The algorithm should choose the ratio that **maximizes the total number of generated tickets** from the available task pool.

### Session 2025-10-12
- Q: How is ticket complexity defined? → A: It is the **total sum** of the complexities of its constituent tasks.
- Q: How is the target complexity determined? → A: The user manually enters the target **total complexity** for a single ticket.
- Q: Are there any strict rules for task types or themes? → A: No, the primary goal is to meet the complexity rule. Other distributions are secondary.
- Q: If tickets have already been generated and the user selects "1. Generate Tickets" again, what should the program do? → A: Immediately ask for new parameters and overwrite the old `tickets.txt` file upon successful generation.
- Q: How should the application handle semicolons that are part of the task's text? → A: For the MVP, it is assumed that the input `tasks.txt` file is always correctly formatted and task text does not contain semicolons.

## Requirements *(mandatory)*

### Functional Requirements

-   **FR-001**: System MUST read tasks from a text file (`tasks.txt`).
-   **FR-002**: Each line in the input file MUST represent a single task with the format: `Тема; Тип; Сложность; Текст задания`. It is assumed for the MVP that the task text itself will not contain semicolons.
-   **FR-003**: System MUST assign a unique integer ID to each task upon reading (e.g., based on line number).
-   **FR-004**: System MUST prompt the user (teacher) to input the **target total complexity for a ticket** and an allowed **complexity tolerance** (in percent) before generation.
-   **FR-005**: System MUST save the generated tickets to an output file (`tickets.txt`), overwriting any previous version of the file.
-   **FR-006**: The output format for a ticket in the file MUST be a single line: `Билет N; (ID) Тема; Тип; Сложность; Текст; (ID) ...`.
-   **FR-007**: System MUST stop generation if it's impossible to create a new ticket that satisfies all rules and inform the user of the reason.
-   **FR-008**: The ticket creation process MUST incorporate randomness to ensure that subsequent runs with the same input produce different (but still valid) sets of tickets.

### Generation Rules (Strict Priority Order)

-   **GR-A.1 (Ticket Total Complexity - HIGHEST PRIORITY)**: The **total sum** of the complexities of all tasks in a single ticket MUST be within a user-defined percentage tolerance of the target total complexity.
    - *Example*: If target is `10` and tolerance is `20%`, the sum must be between `8` and `12` (inclusive).
    - This is the primary and most important condition for forming a ticket.

-   **GR-B.1 (Type Diversity in Ticket - HIGH PRIORITY)**: A ticket MUST NOT consist entirely of tasks from a single type.

-   **GR-C.1 (Theme Diversity in Ticket - MEDIUM PRIORITY)**: A ticket MUST NOT consist entirely of tasks from a single theme.

-   **GR-D.1 (Task Uniqueness in Ticket)**: All tasks within a single ticket MUST be unique.

-   **GR-E.1 (Ticket Uniqueness)**: No two generated tickets can be identical (i.e., contain the exact same set of task IDs). The order of tasks does not matter.

-   **GR-F.1 (Randomness)**: The process of selecting tasks to form a ticket must be randomized. This ensures that running the generation process multiple times with the same input and parameters will produce a different, but equally valid, set of tickets each time.

### Key Entities *(include if feature involves data)*

-   **Task**: Represents a single assignment with properties: `ID`, `Theme`, `Type`, `Complexity`, `Text`.
-   **Ticket**: Represents a collection of `Task` objects.

## Success Criteria *(mandatory)*

### Measurable Outcomes

-   **SC-001**: The application generates the maximum possible number of tickets that satisfy all generation rules from a given input file.
-   **SC-002**: Every ticket in the output `tickets.txt` file successfully passes validation against the primary complexity rule (GR-A.1) and uniqueness rules (GR-B.1, GR-C.1).
-   **SC-003**: When generation stops because a rule cannot be met, the application provides a clear, correct message to the user identifying the blocking rule.