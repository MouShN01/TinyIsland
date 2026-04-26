# TinyIsland Devlog

## 2026-04-26 — Project setup

### Done
- Created the Unity project.
- Set up the GitHub repository.
- Created the feature/project-setup branch.
- Added the initial project folder structure.
- Added the Game Design Document.
- Installed required Unity packages: Input System, Cinemachine, TextMeshPro.

### Design / Technical Decisions
- The project uses a local Unity setup with GitHub version control.
- The game scope is limited to a short three-day survival-arcade prototype.
- The main gameplay loop is: Gather → React to Tide → Build → Climb → Survive → Escalate.
- The tower will use predefined levels instead of physics-based construction to keep the MVP controlled.
- The tide system will be the main pressure mechanic.

### Problems
- GitHub SSH authentication required additional setup.
- Unity assembly definition required a valid name field.
- The project still needs a playable prototype scene.

### Next
- Create the initial prototype scene.
- Implement basic player movement.
- Set up the orbit camera.
- Build the first greybox island.