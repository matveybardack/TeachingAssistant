# Implementation Plan: Ticket Generator (MVP)

**Branch**: `1-ticket-generator` | **Date**: 2025-10-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/1-ticket-generator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

This plan outlines the implementation of a C# console application and class library for generating exam tickets from a text file based on a strict set of rules. The implementation will prioritize memory efficiency, write tickets to the output file as they are generated, and incorporate randomness to ensure non-deterministic results on subsequent runs.

## Technical Context

**Language/Version**: C# (.NET 9)
**Primary Dependencies**: None planned, standard .NET libraries are sufficient.
**Storage**: Text files (`tasks.txt` for input, `tickets.txt` for output).
**Testing**: Manual testing for MVP.
**Target Platform**: Windows Console.
**Project Type**: Console Application + Class Library.
**Constraints**: Memory usage should be minimized by not storing the full text of tasks in memory. The generation algorithm must use a random component to ensure varied output.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Project Scope**: All work is contained within the `TeachingAssistant` directory. (PASS)
- **File Safety**: The plan does not involve modifying `*.csproj` files. (PASS)
- **Code Structure**: The proposed structure ensures all C# methods will be within classes. (PASS)

## Project Structure

### Documentation (this feature)

```
specs/1-ticket-generator/
├── plan.md              # This file
├── spec.md              # The specification
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```
TeachingAssistant/
├── ClassLibraryTicketGenerator/  # Core logic, models, file I/O
│   ├── Models/
│   │   ├── Task.cs
│   │   └── Ticket.cs
│   ├── Services/
│   │   ├── TaskReader.cs
│   │   ├── TicketGenerator.cs
│   │   └── TicketWriter.cs
│   └── ClassLibraryTicketGenerator.csproj
│
└── ConsoleAppUserInterface/      # Console menu and user interaction
    ├── Program.cs
    └── ConsoleAppUserInterface.csproj
```

**Structure Decision**: The project will consist of two parts: a `ClassLibraryTicketGenerator` containing all business logic, models, and data access, and a `ConsoleAppUserInterface` which will handle all user interaction and act as the entry point.
