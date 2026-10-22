# Original Note — faithful English translation

Source: `Fotbalcareer.txt` (Turkish, 176 lines).
This is the **literal translation**, kept verbatim in structure so the polished design in `GDD.md` can be checked against it. Translator's notes are marked `[TN]`. Nothing has been added, removed or corrected here — typos and all.

---

## Stage 1

A nice intro screen

> *[image placeholder: ENTRANCE SCREEN PHOTO]*

After that, key settings

Settings *[TN: source says "Atarlar", a typo for "Ayarlar"]* tab

Exit

New game tab → when pressed, there will be a box to name the career and a confirm button → after confirming, it will spawn into the game

---

## Stage 2

In-game map

First city design, then interactive buildings

**GYM** → furniture / interior design

**HOUSE** → 3 houses as cheap / mid / expensive, only design, shape, size

**BAR** → interior design

**MARKET** → interior design

**GALLERY** → interior design

. . . (we can add more if I think of any, think about it)

**FACILITY** → interior design

---

## Stage 3

The house's design and interaction

**Bed** → skips the day and refills energy, or the sleep duration can be chosen and energy depends on the hour you slept

```
E - SLEEP
      IN THE FORM
G - REST
```

**Wardrobe/cupboard** → energy drink, food and ingredients can be stored, prevents them spoiling (like a CHEST)

**Stove** → with ingredients you can cook meals that give extra energy. For example: Kelle paça: 130% energy

**Shower** → there will be a dirtiness bar, it rises after training or matches, you get in the shower to clean up

**Toilet** → at a certain hour the need for the toilet comes and again shows as a bar

**Couch and TV** → the couch can be sat on. The TV can broadcast news about what you did that day. For example: good performance in the match: (Player name) showed an outstanding performance in today's match

### MARKET INTERIOR DESIGN

**Shelves** → non-interactive but will look full

**Register/cashier** → will be an NPC, when you interact it sells goods for money like Minecraft villager trade

### BAR INTERIOR DESIGN

NPCS

**Bar stool** → can be sat on

**Bartender** → again interactive like the cashier, drinks can be bought

**Drink** → makes you drunk, lowers energy, and if drunk too much (10 OF THEM) a piece of news in the TV news section

### GALLERY INTERIOR DESIGN

**Interactive cars** → when you interact with the cars and buy one, there's a cutscene of shaking hands with the NPC

**NPC** → instead of interacting with the cars, can be bought directly, the cutscene plays again

### FACILITY INTERIOR DESIGN

Teammates

**HEAD COACH** → you interact and choose the training you want to do (written below, in a moment)

### GYM INTERIOR DESIGN

LET'S SAVE THIS FOR LATER, FIRST LET'S ADD THE PLAYER OVERALL SYSTEM

.

---

## Stage 4

PLAYER OVERALL

**TECHNICAL** (MAIN HEADING AND FACILITY TRAININGS)

Trainings done in first person at the facility

* **FINISHING** (close-range shot)
* **DRIBBLING** (obstacle course)
* **HEADING** (head it in from a cross)
* **CROSSING** (you get the idea)
* **PASSING** (passing back and forth)
* **TACKLING** (taking the ball off a player with his back turned)
* **LONG SHOTS** (JUMP SQUAD)

**PHYSICAL** (MAIN HEADING AND GYM ACTIVITIES)

* **SPEED** (TREADMILL) → max speed
* **STAMINA** (TREADMILL)
* **BALANCE** (PULL-UP BAR)
* **STRENGTH** (DUMBBELL)
* **JUMPING** (JUMP SQUAD)
* **ACCELERATION** (WEIGHT PUSH) → time to reach max speed

---

## Stage 5

MINI GAME

> *[TN: the source ends here. Nothing follows the heading.]*

---

## Translator's notes on the source

| Issue | Detail |
|---|---|
| Typos | "Atarlar" → *Ayarlar* (Settings); "Moıbilya" → *mobilya* (furniture); "oonayla" → *onayla* (confirm); "bilnlar" → *binalar* (buildings); "klotuk" → *koltuk* (couch) |
| Empty sections | Stage 5 has a heading and no content; gym interior is explicitly postponed by the author |
| Duplicate module | "JUMP SQUAD" is named under both Long Shots (technical) and Jumping (physical) |
| Unclear quantity | "10 OF THEM" — 10 drinks in one session, assumed |
| Unclear percentage | "130% energy" — ambiguous against a 100-point bar |
| Implied but unstated | no clock, no economy ledger, no save system, no match gameplay, no overall formula, no failure states |
