# Objet N°0

**Objet N°0** is an original, single-player investigation roguelite built
around a compact desk interface rather than animated characters or expansive
environments.

## Core loop

Inspect an impossible lost object, weigh its evidence against the current
protocol, and choose whether to return, archive, or seal it. Every decision
immediately changes the case score, office stability, and the next file.
Between cases, choose small protocol upgrades that alter the rules instead of
just increasing numbers. Once the standard shift is complete, the player can
bank the result or accept optional, riskier files for a higher score.

## Design goals

- Teach the basic classification rules in under one minute.
- Keep evidence deterministic and inspectable; randomness changes cases, not
  the truth of a case.
- Make failure teach a rule and preserve a small amount of permanent archive
  progress instead of demanding a long grind.
- Vary evidence patterns and choices so casework does not become repetitive
  paperwork.
- Use a deliberately designed desk interface, typography, symbols, and sound
  for identity; minimize custom character and environment art.

The design draws on broad loop patterns from card roguelites, deduction games,
short high-tension decisions, and transparent optimization puzzles. It does
not reuse other games' characters, writing, art, or layouts.

## Run

Open this folder in Unity **6000.6.4f1** and press Play. The default scene
launches the prototype. Controls are mouse and keyboard.

## Prototype rules

1. A confirmed impossible property means **seal** the object. This takes
   priority over a claimant.
2. With no anomaly, a verified claimant means **return** the object.
3. Otherwise, **archive** it.

Each file offers three evidence cards and two concentration points. Examine
the evidence that matters, make a ruling, and protect the office's stability.
Every two files, choose one tool. After four files, bank the score or risk two
sealed annexes; a mistake in the annex ends the service and costs points.

Correct files are saved in the permanent archive and unlock new starting
doctrines at 4, 8, and 12 preserved files. Doctrines change which correct
rulings earn a score bonus. The six-file order changes each service, but the
evidence and answer for each file remain fixed and verifiable.

## Versioning

`VERSION` is the source of truth for the project version. In-progress work is
tracked in `CHANGELOG.md`.

The Unity editor menu **Objet No 0 > Validate prototype** creates or repairs
the playable scene, synchronizes the Unity bundle version, and checks the
classification rules and case catalog.

## Early prototype scope

This alpha is a playable loop test, not a content-complete game. It focuses on
a short six-file service, evidence inspection, classification, controlled
uncertainty, two protocol drafts, push-your-luck extraction, and a persistent
best score. Additional cases, sound, and accessibility tuning will follow
only after playtests show that the basic loop is understandable and worth
restarting.
