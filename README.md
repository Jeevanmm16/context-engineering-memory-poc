# Context Engineering — Memory Layer POC

A proof-of-concept implementation of an **AI Agent Memory Layer** that demonstrates how an agent can maintain bounded working memory, persist important information as long-term memory, retrieve relevant memories, and provide them to an LLM as part of the current context.

## Overview

AI agents often need information from previous interactions to complete future tasks. However, keeping the entire conversation history in every request is inefficient and can introduce unnecessary context.

This POC separates memory into:

* **Working Memory** — Recent conversation and information required for the current task.
* **Long-Term Memory** — Important facts, events, and procedures that should persist beyond the current conversation context.

The system selectively stores important information in **SQL Server** and retrieves relevant memories based on the current user query.

## Architecture

```text
                         User
                          │
                          ▼
                   Current Question
                          │
                          ▼
                  ┌───────────────┐
                  │ Memory Manager│
                  └───────┬───────┘
                          │
                 Retrieve Relevant
                    Long-Term Memory
                          │
                          ▼
                  ┌───────────────┐
                  │   SQL Server  │
                  │ Long-Term     │
                  │ Memory Store  │
                  └───────┬───────┘
                          │
                 Semantic / Episodic /
                    Procedural Memory
                          │
                          ▼
                  Working Memory
                  Recent Context
                          │
                          ▼
                  Context Assembly
                          │
                          ▼
                         LLM
                          │
                          ▼
                       Response
```

## Memory Lifecycle

When information enters the system:

```text
Conversation / Event
        │
        ▼
Memory Extraction
        │
        ▼
Should this be remembered?
        │
       YES
        │
        ▼
Memory Classification
        │
        ├── Semantic
        ├── Episodic
        └── Procedural
        │
        ▼
Memory Manager
        │
        ▼
SQL Server
```

When the agent needs previous information:

```text
User Question
      │
      ▼
Memory Retrieval
      │
      ▼
SQL Server
      │
      ▼
Relevant Memory
      │
      ▼
Working Memory
      │
      ▼
LLM Context
      │
      ▼
Answer
```

## Memory Types

### Semantic Memory

Stores persistent facts or knowledge.

Examples:

```text
"My project uses SQL Server."

"The project is built using .NET 10."
```

### Episodic Memory

Stores information about past events or interactions.

Example:

```text
"PR #389 was reviewed and 2 issues were found."
```

### Procedural Memory

Stores procedures, rules, or instructions describing how something should be done.

Example:

```text
"During code review, check JWT issuer and audience."
```

## Working Memory

Working memory contains information required for the **current task**.

It can include:

* Recent conversation turns
* Current user query
* Relevant retrieved memories
* Tool results
* Retrieved documents
* Current instructions
* Other context required by the task

Working memory is bounded to prevent unnecessary context growth.

For example:

```text
Maximum working-memory turns = 5
```

When new turns are added, older turns can leave working memory while important information may still exist in long-term memory.

## Long-Term Memory

Long-term memory contains information that should survive beyond the current working context.

This POC uses **SQL Server** as the long-term memory store.

Example records:

```text
Semantic:
Project uses SQL Server.

Episodic:
PR #389 was reviewed and 2 issues were found.

Procedural:
Always check JWT validation during authentication review.
```

The entire conversation is **not automatically stored** as long-term memory.

The memory layer decides what information is worth persisting.

## Example — Long-Term Memory Recall

### Initial conversation

```text
User:
My project uses SQL Server.
```

The system identifies this as an important fact:

```text
MemoryType: Semantic
Content: My project uses SQL Server.
```

The information is persisted in SQL Server.

### Later conversation

After several other interactions, the original message is no longer present in working memory.

The user asks:

```text
Which database do I use for my project?
```

The system retrieves the relevant long-term memory:

```text
Project uses SQL Server.
```

The retrieved memory is added to the current working context and provided to the LLM.

Expected response:

```text
Your project uses SQL Server.
```

This demonstrates that the agent can recall information from a much earlier interaction through long-term memory.

## Example — Episodic Memory

```text
User:
Review PR #389.

Agent:
2 issues were found.
```

The system can persist:

```text
MemoryType: Episodic
Content: PR #389 was reviewed and 2 issues were found.
```

Later:

```text
User:
What issues did we find in PR #389?
```

The memory layer retrieves the previous event and provides it to the LLM.

## Memory Management Principle

This POC follows the principle:

> **Memory is explicit, typed, and bounded — the system decides what persists instead of keeping everything.**

### Explicit

Not every conversation becomes long-term memory.

### Typed

Persisted memories are classified as:

```text
Semantic
Episodic
Procedural
```

### Bounded

Memory should have appropriate limits and retention rules.

Examples:

```text
Recent conversation → Working Memory
Important facts → Long-Term Memory
Temporary tool results → Discard when no longer required
Large history → Summarize or remove when appropriate
```

## Technology Stack

* **.NET 10**
* **ASP.NET Core**
* **C#**
* **SQL Server**
* **Entity Framework Core**
* **LLM integration**
* **Langfuse** — tracing and observability

## Project Structure

```text
src/
│
├── API/
│
├── Memory/
│   ├── WorkingMemory/
│   ├── LongTermMemory/
│   ├── MemoryExtractor/
│   ├── MemoryClassifier/
│   └── MemoryManager/
│
├── LLM/
│
└── Data/

tests/
│
└── Memory/
```

## Key Components

### Memory Extractor

Identifies information that may be valuable for future tasks.

```text
Conversation
     ↓
Memory Extractor
     ↓
Candidate Memory
```

### Memory Classifier

Determines the type of a candidate memory.

```text
Candidate Memory
       ↓
Memory Classifier
       ↓
Semantic / Episodic / Procedural
```

### Memory Manager

Responsible for memory operations such as:

* Deciding whether memory should persist
* Storing memories
* Retrieving memories
* Updating memory metadata
* Managing memory lifecycle

### Working Memory

Maintains the recent context required by the current task.

### Long-Term Memory Store

SQL Server stores persisted memories that can be retrieved in future interactions.

## Design Principles

### 1. Selective Persistence

Do not automatically persist everything the agent sees.

### 2. Explicit Memory Types

Every persisted memory should have a clear purpose and type.

### 3. Bounded Working Memory

Only information relevant to the current task should be included in working memory.

### 4. Relevant Retrieval

Retrieve only the memories required to answer the current user query.

### 5. Separation of Memory and Context

Long-term memory is stored separately from the context currently sent to the LLM.

```text
Long-Term Memory
      ↓
   Retrieval
      ↓
Working Memory
      ↓
    Context
      ↓
     LLM
```

## Response Behavior

The LLM should answer the user's current question directly using the available context.

It should not expose internal memory operations.

For example:

### Avoid

```text
You've already answered this question yourself.
You mentioned earlier that...
```

### Prefer

```text
Your project uses SQL Server.
```

The model should use retrieved memory as context without discussing the internal retrieval process unless the user explicitly asks about it.

## Future Improvements

This POC can later be extended with:

* Vector-based semantic retrieval
* Hybrid SQL + vector retrieval
* Memory relevance scoring
* Memory expiration and retention policies
* Memory summarization
* Memory deduplication
* LLM-based memory extraction
* LLM-based memory classification
* Memory importance scoring
* User-specific memory
* Agent-specific memory
* Langfuse tracing for memory retrieval
* Integration with the Context Assembly Service

## Learning Objective

This POC demonstrates the complete memory lifecycle:

```text
Extract
   ↓
Decide
   ↓
Classify
   ↓
Persist
   ↓
Retrieve
   ↓
Working Memory
   ↓
Context Assembly
   ↓
LLM
   ↓
Response
```

The main objective is to demonstrate that an AI agent does **not need to keep everything in its active context**. Important information can be selectively persisted in long-term memory and retrieved only when it becomes relevant to a future task.
