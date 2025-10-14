# Tasks: Ticket Generator (MVP)

**Input**: Design documents from `specs/1-ticket-generator/`
**Prerequisites**: `plan.md`, `spec.md`, `data-model.md`

**Organization**: Tasks are grouped by user story and implementation priority to enable incremental development.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Define the core data structures for the project.

- [ ] T001 [US1, US2] Request user to create the directory `ClassLibraryTicketGenerator/Models` and the file `ClassLibraryTicketGenerator/Models/Task.cs`.
- [ ] T002 [US1, US2] Request user to create the file `ClassLibraryTicketGenerator/Models/Ticket.cs`.

---

## Phase 2: Foundational - Data I/O

**Purpose**: Implement the logic for reading and parsing the input file efficiently.

- [ ] T003 [US2] Request user to create the directory `ClassLibraryTicketGenerator/Services` and the file `ClassLibraryTicketGenerator/Services/TaskReader.cs`.
- [ ] T004 [US2] Edit `TaskReader.cs` to add a method for parsing a task line (metadata only).
- [ ] T005 [US2] Request user to create the file `ClassLibraryTicketGenerator/Services/TicketWriter.cs`.
- [ ] T006 [US2] Edit `TicketWriter.cs` to add a method for appending a ticket to the output file.

---

## Phase 3: User Story 1 - Console Interface

**Purpose**: Create the user-facing console menu and connect it to placeholder logic.

- [ ] T007 [US1] Edit `ConsoleAppUserInterface/Program.cs` to implement the main application loop and menu.
- [ ] T008 [US1] Edit `Program.cs` to add logic for the "View Generated Tickets" option.
- [ ] T009 [US1] Edit `Program.cs` to add logic for the "Generate Tickets" option to prompt for parameters.
- [ ] T010 [US1] Edit `Program.cs` to add a call to a placeholder/stub method in the `TicketGenerator` service.

**Checkpoint**: At this point, the console application should be fully interactive, though the core generation logic is just a stub.

---

## Phase 4: User Story 2 - Generation Algorithm

**Purpose**: Implement the core ticket generation logic, following all rules from the specification.

- [ ] T011 [US2] Request user to create the `TicketGenerator` service class in `ClassLibraryTicketGenerator/Services/TicketGenerator.cs` with a placeholder method.
- [ ] T012 [US2] Remove the `CalculateTypeRatio` method from `TicketGenerator.cs` as the pre-calculation approach is incorrect.
- [ ] T013 [US2] Modify the main `Generate` loop in `TicketGenerator.cs`. It should repeatedly call a recursive search function to find the next valid ticket combination from the entire pool of tasks until no more unique, valid tickets can be formed.
- [ ] T014 [US2] Rewrite the recursive search logic (`FindCombinationRecursive` and `IsValidCombination`). The new logic must build and validate ticket candidates on the fly, checking for complexity, theme diversity, and task uniqueness, without relying on a pre-calculated type ratio.
- [ ] T015 [US2] Ensure the main loop in `TicketGenerator.cs` correctly integrates with the `TicketWriter` to save valid tickets as they are found.
- [ ] T016 [US2] Edit `TicketGenerator.cs` to add logic for gracefully stopping generation when no more valid tickets can be formed and reporting the reason to the user.
- [ ] T017 [US1, US2] Edit `Program.cs` to replace the placeholder stub call with the actual `TicketGenerator` call, passing all required parameters.

---

## Dependencies & Execution Order

1.  **Phase 1 (Setup)**: Can be done first.
2.  **Phase 2 (Data I/O)**: Depends on Phase 1. This is a critical foundation.
3.  **Phase 3 (Console Interface)**: Can be done in parallel with Phase 2, as it initially depends only on the existence of the service classes, not their full implementation.
4.  **Phase 4 (Generation Algorithm)**: Depends on all previous phases. This is the final and most complex part.