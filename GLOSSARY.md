# Plates

A coach assigns strength training programs to clients; after each logged workout, the progression engine decides the next session's weight for every exercise.

## Language

### Accounts

**Gym**:
An account whose coaches, clients and data no other gym can see; a self-employed coach has a Gym of their own.
_Avoid_: Tenant (outside code), organisation, club

**Plan**:
The Gym's subscription level, which sets how many clients it may have (5, 30 or unlimited); changed by the platform administrator by hand.
_Avoid_: Subscription, tier, package

**Invitation**:
A single-use link, valid for 7 days, that lets a person join a Gym in one role.
_Avoid_: Invite code, signup link

**Stuck**:
A client is stuck on an exercise when its latest Decision code is Failure, Deload or Limit.
_Avoid_: Plateaued, blocked

### Exercises

**Exercise**:
A named movement a program can contain, with its own Target and Record per client; either a Catalogue exercise or a Custom exercise.
_Avoid_: Movement, lift (as a term)

**Catalogue exercise**:
A well-known Exercise from the global list shared by every Gym; the list changes only with a new release.
_Avoid_: Standard exercise, global exercise

**Custom exercise**:
An Exercise a Gym's coach or owner creates (a variation or a new movement); visible only inside that Gym and never added to the catalogue. A variation has no link to its base Exercise.
_Avoid_: Variation (as a separate concept), private exercise

### Progression

**Progression scheme**:
A set of rules that decides an exercise's next Target from the sets logged in a workout. Double progression is the first one.
_Avoid_: Strategy, algorithm, method

**Target**:
The weight a client should lift for an exercise in the next workout.
_Avoid_: Goal, planned weight, tikslas (in code)

**Base weight**:
The lowest weight among the sets logged for one exercise in a workout; the starting point for the next Target.
_Avoid_: Working weight

**Failure counter**:
How many workouts in a row an exercise was failed (a set below the minimum reps, or fewer sets than planned).
_Avoid_: Counter, strikes, nesėkmių skaitiklis (in code)

**Record**:
The highest estimated one-rep max (Epley) for an exercise, not the heaviest weight actually lifted.
_Avoid_: PR, max, 1RM (as the stored value), rekordas (in code)

**Increment**:
The weight added to the Base weight on Increase; a multiple of 2.5 kg.
_Avoid_: Step, jump

**Failure limit**:
The Failure counter value that triggers a Deload (default 2).
_Avoid_: Max failures, strikes

**Deload percentage**:
The share of the Base weight removed on Deload (default 10 %).
_Avoid_: Reduction, drop

### Decisions

**Decision code**:
The outcome the engine gives for one exercise, each with a Lithuanian reason text.
_Avoid_: Result, status

- **Increase** (DIDINTI): Target goes up by the increment.
- **Limit** (RIBA): Target would pass 500 kg, so it is held at 500 kg and automatic progression stops.
- **Hold** (IŠLAIKYTI): Target stays at the Base weight.
- **Failure** (NESĖKMĖ): a failed workout below the failure limit; Target stays at the Base weight.
- **Deload** (MAŽINTI): the failure limit was reached; Target drops by the deload percentage.
