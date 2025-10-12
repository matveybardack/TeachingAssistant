# Data Model: Ticket Generator

**Input**: `spec.md`

## Entities

### 1. Task

Represents the metadata of a single task read from the input file. The full text of the task is not stored in this object to conserve memory.

| Field | Type | Description |
|---|---|---|
| `Id` | `int` | The unique identifier of the task (line number). |
| `Theme` | `string` | The theme or topic of the task. |
| `Type` | `string` | The type of the task (e.g., "Theory", "Practice"). |
| `Complexity` | `int` | The complexity value of the task (integer >= 1). |

### 2. Ticket

Represents a single generated ticket. It only stores the IDs of the tasks it contains, not the full task objects.

| Field | Type | Description |
|---|---|---|
| `TicketNumber` | `int` | The sequential number of the ticket (e.g., 1, 2, 3...). |
| `TaskIds` | `List<int>` | A collection of task IDs that make up the ticket. |