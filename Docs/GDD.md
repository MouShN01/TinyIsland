# TinyIsland — Game Design Document

## 1. Project Overview

**TinyIsland** is a short survival-arcade prototype developed in Unity. The game follows a tiny castaway stranded on a small empty island in the middle of the ocean after a shipwreck. The player must collect driftwood during low tide, build a tower, and survive rising tides until the tower eventually becomes a raft.

The project is designed as a compact portfolio game with a clear core loop, limited scope, readable mechanics, and a complete short-form experience.

---

## 2. One Sentence Pitch

**TinyIsland is a short survival-arcade game where a tiny castaway collects driftwood during low tide, builds a tower, and climbs above the rising water before the island is flooded.**

---

## 3. High Concept

The player controls a small toy-like character stranded on a tiny island after a shipwreck. The island is empty, with no palm trees or large structures — only sand, ocean, and the remains of the wreck.

During the day, the tide moves in and out. When the water goes down, pieces of driftwood appear on the sand. The player must quickly collect them before smaller daytime tides cover the beach again. These mini tides create constant pressure and force the player to decide which pieces of wood are worth chasing.

Collected driftwood is used to build a tower in the center of the island. At night, a major tide rises high enough to flood the entire island. The player must reach the tower and climb it using a timing-based directional input sequence. If the tower is high enough and the player climbs successfully, they survive until the next day.

After surviving three days, the tower breaks apart and becomes a raft, allowing the player to escape.

---

## 4. Design Pillars

### 4.1 Simple Survival Pressure

The player should immediately understand the main objective: collect wood, build higher, survive the tide. The main threat is the environment itself, not complex combat or resource systems.

### 4.2 Small Island, Clear Decisions

The game takes place on a very small island. The limited space makes the player's decisions more readable: where to run, which plank to collect, when to retreat, and when to build.

### 4.3 Tide as the Main Enemy

The tide is the central pressure system. It creates both short-term urgency during the day and a major survival check at night.

### 4.4 Active Climbing Finale

Reaching the tower is not enough. The player must climb it through a short timing-based input sequence, creating tension during the final moments before the water reaches them.

### 4.5 Small but Complete Experience

TinyIsland should feel like a complete short game, not a collection of unfinished systems. The target experience is 5–10 minutes of gameplay across three in-game days.

---

## 5. Target Experience

The player should feel:

> I have very little time. The water is coming back. I need to collect enough wood, build higher, and reach safety before the island disappears under the tide.

The intended emotional arc:

```text
Isolation → Discovery → Urgency → Pressure → Survival → Escape
```

The game should not feel like a hardcore survival simulator. It should feel like a compact, stylized arcade-survival experience with clear rules and visible escalation.

---

## 6. Core Loop

```text
Day begins
↓
Water level drops
↓
Driftwood appears on the beach
↓
Player collects driftwood
↓
Small daytime tides periodically return
↓
Some driftwood becomes temporarily unavailable or risky to collect
↓
Player uses driftwood to build the tower
↓
Night tide warning begins
↓
Water rises and floods the island
↓
Player reaches the tower
↓
Player completes a timing-based climbing sequence
↓
If the player is high enough, they survive
↓
Next day begins with higher pressure
```

Short version:

```text
Gather → React to Tide → Build → Climb → Survive → Escalate
```

---

## 7. Player Goals

### 7.1 Short-Term Goals

- Collect nearby driftwood.
- Avoid wasting time.
- Watch the shoreline for tide warnings.
- Return to the tower before night.
- Build the next tower segment.
- Climb above the water level.

### 7.2 Medium-Term Goals

- Prepare for the next night.
- Build the tower high enough.
- Handle increasingly difficult tide cycles.
- Avoid or push away hazards.
- Survive all three days.

### 7.3 Final Goal

- Survive the third night and escape the island on a raft.

---

## 8. Player Verbs

The player can:

- Move around the island.
- Collect driftwood.
- Build tower segments.
- React to incoming tide warnings.
- Avoid hazards.
- Push crabs away with a stick.
- Start tower climbing.
- Press directional inputs during climbing.
- Survive above the tide.

---

## 9. Gameplay Structure

TinyIsland is divided into three in-game days. Each day has two main phases:

```text
Daytime Gathering Phase
Nighttime Survival Phase
```

During the daytime phase, the player collects driftwood and builds the tower. Small tides periodically move in and out, covering parts of the beach.

During the nighttime phase, the major tide rises and floods the island. The player must climb the tower to survive.

---

## 10. Core Mechanics

### 10.1 Player Movement

The player moves around a small circular island. Movement should be responsive, simple, and readable.

Required movement:

- Move
- Turn
- Interact
- Push
- Enter climbing mode

The player does not need complex abilities such as jumping, swimming, crouching, stamina, or advanced parkour in the MVP.

---

### 10.2 Island and Camera

The island should feel like a small diorama in the middle of the ocean. The player is a tiny character moving around a compact space.

The intended camera style is an orbiting island camera. The camera should make it feel as if the island is rotating visually around the player, while still keeping controls understandable.

Recommended MVP implementation:

- The player moves normally on the island.
- The camera orbits around the island center.
- The camera keeps the player, tower, and nearby resources visible.

The island itself does not need to physically rotate in the MVP. Visual readability is more important than technical complexity.

---

### 10.3 Tide System

The tide is the main environmental pressure system in TinyIsland.

There are two types of tide movement:

- Daytime mini tides
- Nightly major tide

#### Daytime Mini Tides

During the day, the water does not stay fully low. Instead, smaller tides periodically rise and fall. These mini tides cover parts of the beach and make some driftwood temporarily unavailable.

Their purpose is to create urgency during resource gathering.

The player should feel:

> I need to grab this plank now, before the water covers it again.

Mini tides should not be instant death. They should create pressure, not unfair punishment.

Possible mini tide effects:

- Driftwood becomes temporarily unreachable.
- The player is slowed if caught near the shoreline.
- The player is pushed slightly toward the island center.
- The player loses time if they are too greedy.

Recommended MVP behavior:

- The water rises slightly during the day.
- Driftwood below the mini tide level becomes unavailable.
- The player receives a visual warning before the tide moves in.
- After a short time, the water recedes and the driftwood becomes available again.

#### Nightly Major Tide

At night, the major tide rises much higher than daytime mini tides. It floods the island and acts as the main survival check.

The player survives if:

- The tower is high enough.
- The player successfully climbs to the safe point.
- The player remains above the final water level.

The player loses if:

- The water reaches them before they are high enough.

---

### 10.4 Tide Warning Feedback

Before any tide movement begins, the water should clearly telegraph danger.

The main visual warning is **shoreline trembling**.

Before the tide rises:

- The edge of the water starts to shake or pulse.
- Small waves appear near the shoreline.
- The water surface becomes more active.
- A warning sound begins.

This applies to both daytime mini tides and the nightly major tide.

The warning sequence:

```text
Calm water → trembling shoreline → incoming tide → danger → water recedes or survival check
```

Design purpose:

- The player should never feel that the water rose randomly.
- The warning gives the player time to make a decision.
- The player can choose to grab one more plank or retreat.

Optional UI warning:

> The tide is coming in!

---

### 10.5 Driftwood Collection

Driftwood appears on the exposed sand during low tide. The player collects it by moving close and interacting with it, or by touching it directly.

MVP behavior:

- Driftwood spawns at predefined points.
- The player collects it.
- The wood counter increases.
- The driftwood object disappears.

No complex inventory is needed. Driftwood is represented as a simple resource counter.

Driftwood can have three states:

- Available
- Covered by water
- Collected

If covered by a mini tide, the driftwood does not disappear permanently. It becomes unavailable until the water recedes.

---

### 10.6 Tower Building

The tower is the central survival object.

The player spends collected driftwood to increase the tower height. The tower grows in visible segments. Each segment represents progress and raises the safe point for the nightly tide.

MVP behavior:

- The tower has levels.
- Each level requires a fixed number of driftwood pieces.
- When the player has enough wood, they can build the next level.
- A new visual tower segment appears.
- The required safe height increases each day.

The tower should not be physics-based in the MVP. It should be a controlled level-based structure.

Example:

- Level 1 tower: survives Day 1 tide.
- Level 2 tower: survives Day 2 tide.
- Level 3 tower: survives Day 3 tide.

---

### 10.7 Timing-Based Tower Climbing

Climbing the tower is an active timing-based mechanic.

When the player reaches the tower during the tide phase, they enter a climbing sequence. Directional prompts appear on screen, and the player must press the correct key at the correct time.

The mechanic is inspired by rhythm/timing input sequences, similar in principle to dance minigames where the player must press the shown direction in time.

#### Design Purpose

The climbing mechanic adds tension to the final seconds before the water reaches the player.

The player should feel:

> I reached the tower, but I still need to climb it before the water catches me.

Reaching the tower should reduce danger, but not remove it completely.

#### Input Style

Possible input prompts:

- W
- A
- S
- D

or:

- Arrow Up
- Arrow Left
- Arrow Down
- Arrow Right

For keyboard MVP, WASD is acceptable.

During climbing, normal movement is temporarily disabled and WASD becomes climbing input.

#### Climbing Behavior

The tower has predefined grip points arranged around its vertical axis.

Example structure:

```text
Tower Level 1
- Front grip
- Left grip
- Right grip
- Back grip

Tower Level 2
- Front grip
- Left grip
- Right grip
- Back grip

Tower Level 3
- Front grip
- Left grip
- Right grip
- Back grip
```

Each successful input moves the player to the next grip point or higher tower position.

Example:

```text
Prompt: W
Player presses W in time
Character jumps to the front upper grip

Prompt: D
Player presses D in time
Character jumps to the right upper grip

Prompt: A
Player presses A too late
Character stays in place and loses time
```

The movement should be controlled and animation-driven, not physics-driven.

#### Success and Failure

Recommended MVP rules:

- Correct input: player moves to the next climb point.
- Wrong input: player does not move and loses time.
- Missed input: player does not move and the next prompt is delayed.
- Too many mistakes: the player may fall one grip lower, but this can be reserved for later polish.

For the MVP, wrong inputs should not cause instant failure. The rising water already creates enough pressure.

#### Difficulty Scaling

The climbing sequence becomes harder each day.

Example:

| Day | Inputs | Timing Window | Pressure |
|---:|---:|---|---|
| 1 | 2 | Large | Low |
| 2 | 3 | Medium | Medium |
| 3 | 4–5 | Smaller | High |

---

### 10.8 Hazards

Hazards are secondary pressure elements. They should not replace the tide as the main threat.

#### Crabs

Crabs appear from Day 2 onward. They interfere with the player's movement and make resource gathering less predictable.

MVP behavior:

- Crabs move around the island.
- If a crab touches the player, it pushes or slows them.
- The player can push crabs away with a stick.

Crabs should be annoying, not deadly.

Their design purpose:

- Waste the player's time.
- Interrupt direct paths to driftwood.
- Increase pressure during mini tides.
- Force quick reactions without turning the game into combat.

#### Sharks / Tentacles

Sharks or tentacles can be considered stretch content for Day 3, but they are not required for the MVP.

They should only be added if the core loop, tides, tower building, and climbing already work well.

---

## 11. Day-by-Day Progression

### Day 1 — Learning the Loop

Purpose:

- Teach the player to collect driftwood, build the tower, react to tide warnings, and survive the first night.

Features:

- No enemies.
- Slow mini tides.
- Generous collection time.
- Low tower requirement.
- Short climbing sequence.

Example balance:

| Parameter | Value |
|---|---|
| Wood required | 3 |
| Wood available | 5 |
| Mini tides | Slow and predictable |
| Hazards | None |
| Climbing inputs | 2 |

---

### Day 2 — First Interference

Purpose:

- Add pressure and force the player to manage time more carefully.

Features:

- Higher night tide.
- More wood required.
- More frequent mini tides.
- Crabs appear.
- Longer climbing sequence.

Example balance:

| Parameter | Value |
|---|---|
| Additional wood required | 4 |
| Wood available | 7 |
| Mini tides | Moderate |
| Hazards | 1–2 crabs |
| Climbing inputs | 3 |

---

### Day 3 — Final Pressure

Purpose:

- Create the final challenge before escape.

Features:

- Highest night tide.
- More driftwood required.
- More frequent mini tides.
- More crabs.
- Longest climbing sequence.
- Final tower level required.

Example balance:

| Parameter | Value |
|---|---|
| Additional wood required | 5 |
| Wood available | 8–9 |
| Mini tides | Frequent |
| Hazards | 2–3 crabs |
| Climbing inputs | 4–5 |

After surviving Day 3, the tower breaks apart and becomes a raft.

---

## 12. Win and Lose Conditions

### Win Condition

The player wins after surviving the third night.

Win sequence:

```text
The final tide passes.
The tower becomes unstable.
The tower breaks apart.
The wood forms a simple raft.
The player escapes the island.
Win screen appears.
```

The transformation does not need to be fully physics-based. For the MVP, it can be a controlled animation or a simple object swap.

### Lose Condition

The player loses if the water reaches them before they are safely above the tide level.

Possible fail cases:

- The tower is not high enough.
- The player fails or delays the climbing sequence for too long.
- The player reaches the tower too late.
- The player stays on low ground during the night tide.

Example fail message:

> The tide was too high. Build faster next time.

---

## 13. Controls

### Normal Gameplay

| Input | Action |
|---|---|
| WASD / Left Stick | Move |
| E / South Button | Interact / Build / Start Climbing |
| Space / West Button | Push with stick |
| Esc / Start | Pause |

### Climbing Mode

| Input | Action |
|---|---|
| W / A / S / D | Directional climbing inputs |

During climbing mode:

- Normal movement is disabled.
- The camera focuses on the tower.
- One directional prompt appears at a time.
- The player must press the correct input within the timing window.

The transition between normal movement and climbing mode must be clear.

---

## 14. User Interface

The UI should be minimal and readable.

Required UI elements:

- Current day
- Time until major tide
- Wood count
- Tower level
- Required tower level
- Mini tide warning
- Night tide warning
- Build prompt
- Climbing prompts
- Win screen
- Lose screen

Example normal UI:

```text
Day 2 / 3
Wood: 4
Tower: Level 2
Night Tide In: 00:35
```

Example tide warning:

> The tide is coming in!

Example climbing UI:

```text
Press: W
Timing bar
```

The climbing UI should be large enough to read instantly, because the player will be under pressure.

---

## 15. Visual Style

TinyIsland should use a stylized, soft, toy-like visual style.

Visual direction:

- Small rounded island
- Empty sand surface
- Ocean surrounding the island
- Soft colors
- Readable silhouettes
- Small mascot-like player character
- Chunky wooden tower segments
- Clearly visible driftwood
- Simple crabs
- Exaggerated water movement

The island should feel isolated but not dark or realistic. The atmosphere is light survival pressure, not horror.

The character can be inspired by small stylized mascot characters: simple proportions, readable animations, and a cute silhouette.

---

## 16. Audio Direction

Audio should support both atmosphere and pressure.

Required sounds:

- Ocean ambience
- Wood pickup
- Tower build
- Mini tide warning
- Major tide warning
- Water rising
- Crab movement
- Crab push reaction
- Climbing correct input
- Climbing wrong input
- Night transition
- Win sequence
- Lose sequence

Audio priorities:

- The ocean should always be present.
- Tide warnings should be impossible to miss.
- Building should feel satisfying.
- Climbing inputs should have clear feedback.

---

## 17. MVP Scope

### Must Have

- Playable character
- Small island arena
- Orbiting island camera
- Day/night structure
- Daytime mini tides
- Nightly major tide
- Water edge trembling warning
- Driftwood pickups
- Wood counter
- Tower building by levels
- Timing-based tower climbing
- Survival check
- Three playable days
- Win screen
- Lose screen
- Basic UI

### Should Have

- Crabs on Day 2 and Day 3
- Stick push interaction
- Different climbing sequence length per day
- Audio warnings before tide movement
- Simple shoreline ripple/tremble effect
- Basic tower transformation into raft
- Basic character animations

### Could Have

- Intro shipwreck scene
- Animated raft escape
- Sharks or tentacles
- Foam particles
- Dynamic music intensity
- Perfect timing bonus during climbing
- Combo feedback
- More polished tower climbing animations

### Out of Scope for MVP

- Open-world exploration
- Complex crafting
- Inventory management
- Hunger or thirst
- Health system
- Procedural island generation
- Physics-based tower construction
- Multiple resources
- Save system
- Skill upgrades
- Complex enemy AI
- Full combat system
- Swimming system

---

## 18. Main Design Risks

### 18.1 Camera Readability

The rotating island camera is visually interesting, but it can make movement confusing.

Mitigation:

- Prototype the camera early.
- Keep the island small and readable.
- Avoid excessive camera rotation speed.
- Make the tower visually central.
- Make resources visible from the camera angle.

### 18.2 Daytime Tide Fairness

Mini tides can create good pressure, but they must not feel random or unfair.

Mitigation:

- Always telegraph tide movement before it starts.
- Use shoreline trembling as a clear warning.
- Do not instantly kill the player during mini tides.
- Allow covered driftwood to become available again.
- Use mini tides as pressure, not punishment.

### 18.3 Climbing Complexity

The timing-based climbing mechanic can become too difficult or frustrating.

Mitigation:

- Start with very simple sequences.
- Use large timing windows on Day 1.
- Make wrong inputs cost time, not instantly fail.
- Keep the number of climb points small.
- Use clear visual and audio feedback.

### 18.4 Control Mode Confusion

WASD is used both for movement and climbing inputs. This can confuse the player if the transition is unclear.

Mitigation:

- Freeze normal movement during climbing.
- Show a clear climbing UI.
- Move the camera closer to the tower.
- Display one prompt at a time.
- Use clear success and failure feedback.

### 18.5 Scope Creep

The concept can easily grow into a larger survival game.

Mitigation:

- Keep one resource.
- Keep one main building.
- Keep three days.
- Keep the tide as the main threat.
- Add only one enemy type for MVP.
- Avoid realistic physics-based construction.

---

## 19. Technical Structure

Recommended Unity systems:

- GameManager
- DayCycleController
- TideController
- MiniTideController
- TideWarningController
- PlayerController
- PlayerInteraction
- WoodSpawner
- WoodPickup
- TowerController
- ClimbingController
- CrabController
- CrabSpawner
- UIController
- AudioController

Recommended data structure:

```text
DayConfig ScriptableObject
```

Example `DayConfig` fields:

```text
dayNumber
lowTideDuration
nightTideDuration
miniTideInterval
miniTideWarningDuration
miniTideHeight
miniTidePeakDuration
nightTideWarningDuration
nightTideMaxHeight
requiredTowerLevel
woodSpawnCount
crabCount
climbingInputCount
climbingTimingWindow
```

This allows day balance to be tuned without hardcoding values.

---

## 20. Example Balance Table

| Day | Wood Required | Wood Available | Mini Tide Pressure | Hazards | Climbing Inputs | Night Tide Height |
|---:|---:|---:|---|---|---:|---|
| 1 | 3 | 5 | Low | None | 2 | Low |
| 2 | +4 | 7 | Medium | 1–2 crabs | 3 | Medium |
| 3 | +5 | 8–9 | High | 2–3 crabs | 4–5 | High |

This balance is only a starting point. It should be adjusted after playtesting.

---

## 21. Portfolio Value

TinyIsland is designed to demonstrate:

- Core loop design
- Short-form survival design
- Environmental pressure mechanics
- Telegraphed hazard design
- Time-based resource gathering
- Simple economy balancing
- Tower progression system
- Timing-based climbing minigame
- Input-mode switching
- Small-scale difficulty escalation
- Unity gameplay architecture
- ScriptableObject-based balancing
- Camera prototyping
- Player feedback systems
- Controlled MVP scope

The project is suitable for a portfolio because it is small enough to finish, but complete enough to show design thinking, implementation discipline, and an understanding of how mechanics support player pressure.

---

## 22. Portfolio Summary

**TinyIsland** is a compact Unity survival-arcade prototype about a tiny castaway stranded on a small island. During the day, the player collects driftwood exposed by low tide while smaller tide waves periodically cover parts of the beach. Before each tide movement, the shoreline trembles as a visual warning, forcing the player to decide whether to collect one more plank or retreat.

Collected driftwood is used to build a tower in the center of the island. At night, a major tide floods the entire island, and the player must climb the tower through a timing-based directional input sequence. Each day increases the tide pressure, tower requirements, and hazards. After surviving three days, the tower breaks apart into a raft, allowing the player to escape.

---

## 23. Short README Version

TinyIsland is a short survival-arcade prototype made in Unity.

The player controls a tiny castaway stranded on a small island after a shipwreck. During the day, the tide goes down and pieces of driftwood appear on the sand. The player must collect them and build a tower before the nightly tide floods the island.

Small daytime tides periodically move in and cover parts of the beach, creating pressure during resource gathering. Before each tide, the water edge trembles as a visual warning.

At night, the player must climb the tower through a timing-based directional input sequence. If the tower is high enough and the player climbs successfully, they survive. After three days, the tower breaks apart into a raft and the player escapes.

Core loop:

```text
Gather → React to Tide → Build → Climb → Survive → Escalate
```

---

## 24. Final Scope Formula

The project should stay within this frame:

```text
One small island
One resource
One main building
One environmental threat
One timing-based climbing mechanic
One secondary hazard type
Three days
One escape ending
```

This keeps TinyIsland realistic for a short production cycle while still giving it enough mechanical identity for a strong portfolio piece.
