---

description: "Task list for Proof Envelope Format"
---

# Tasks: Proof Envelope Format

**Input**: Design documents from /specs/001-proof-envelope-format/
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Not requested for this feature.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: [ID] [P?] [Story] Description

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Documentation scaffolding

- [x] T001 [US1] Create contracts directory at specs/001-proof-envelope-format/contracts/ (supports FR-001 to FR-005)
- [x] T002 [US1] Draft research notes in specs/001-proof-envelope-format/research.md (supports FR-001a to FR-007)
- [x] T003 [US1] Draft data model outline in specs/001-proof-envelope-format/data-model.md (supports FR-001 to FR-010)
- [x] T004 [US1] Draft quickstart outline in specs/001-proof-envelope-format/quickstart.md (supports FR-011 to FR-015)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Align spec requirements with planned artifacts

- [x] T005 Align functional requirements with planned contract fields in specs/001-proof-envelope-format/spec.md
- [x] T006 Document offline-only and fail-closed constraints in specs/001-proof-envelope-format/quickstart.md
- [x] T007 Document deterministic failure handling for unknown identifiers in specs/001-proof-envelope-format/quickstart.md
- [x] T008 Document proofBytes non-logging policy in specs/001-proof-envelope-format/quickstart.md

---

## Phase 3: User Story 1 - Define a proof envelope (Priority: P1) 🎯 MVP

**Goal**: Provide a versioned JSON envelope contract and documentation for offline verification.

**Independent Test**: Validate a minimal envelope against the schema and confirm it is accepted.

### Implementation for User Story 1

- [x] T009 [P] [US1] Define top-level envelope schema fields in specs/001-proof-envelope-format/contracts/proof-envelope.schema.json
- [x] T010 [US1] Document ProofEnvelope fields and rules in specs/001-proof-envelope-format/data-model.md
- [x] T011 [US1] Add minimal and full envelope examples in specs/001-proof-envelope-format/quickstart.md

**Checkpoint**: User Story 1 documentation and schema are sufficient to validate a minimal envelope.

---

## Phase 4: User Story 2 - Enforce strict public inputs (Priority: P2)

**Goal**: Ensure public inputs are strict and schema-validated for v1.

**Independent Test**: Validate an envelope with an extra `publicInputs` field and confirm failure.

### Implementation for User Story 2

- [x] T012 [P] [US2] Enforce strict publicInputs schema (required fields, no extras) in specs/001-proof-envelope-format/contracts/proof-envelope.schema.json
- [x] T013 [US2] Document publicInputs validation rules in specs/001-proof-envelope-format/data-model.md
- [x] T014 [US2] Add strict publicInputs validation notes in specs/001-proof-envelope-format/quickstart.md

**Checkpoint**: User Story 2 documentation reflects strict publicInputs validation for v1.

---

## Phase 5: User Story 3 - Forward-compatible top-level envelope (Priority: P3)

**Goal**: Allow unknown top-level fields without breaking validation.

**Independent Test**: Validate an envelope with an extra top-level field and confirm acceptance.

### Implementation for User Story 3

- [x] T015 [P] [US3] Ensure top-level forward compatibility in specs/001-proof-envelope-format/contracts/proof-envelope.schema.json
- [x] T016 [US3] Add forward-compatibility guidance in specs/001-proof-envelope-format/quickstart.md

**Checkpoint**: User Story 3 guidance clarifies how unknown top-level fields are handled.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation consistency and discoverability

- [x] T017 [P] [US1] Add spec index entry in specs/README.md (supports discoverability for all stories)
- [x] T018 Update quickstart validation notes for expiry handling in specs/001-proof-envelope-format/quickstart.md
- [x] T019 Update agent context via .specify/scripts/bash/update-agent-context.sh copilot


---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Phase 6)**: Depends on completion of desired user stories

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2, no dependencies on other stories
- **User Story 2 (P2)**: Starts after Phase 2, independent of US1
- **User Story 3 (P3)**: Starts after Phase 2, independent of US1/US2

### Parallel Opportunities

- Phase 1 tasks can run in parallel (different files)
- Schema updates in different phases can be coordinated but should avoid overlapping edits
- Documentation tasks in data-model.md and quickstart.md can run in parallel when not touching the same section

---

## Parallel Example: User Story 1

```text
Task: "Define top-level envelope schema fields in specs/001-proof-envelope-format/contracts/proof-envelope.schema.json"
Task: "Document ProofEnvelope fields and rules in specs/001-proof-envelope-format/data-model.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate minimal envelope against schema

### Incremental Delivery

1. Setup + Foundational
2. User Story 1 → validate minimal envelope
3. User Story 2 → validate strict publicInputs
4. User Story 3 → validate forward-compatible top-level fields
5. Polish updates
