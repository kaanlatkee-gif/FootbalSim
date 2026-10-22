# Original Note — faithful English translation

Two revisions of the source note exist in this repository's history:

| File | Rows | Content |
|---|---|---|
| `Fotbalcareer.txt` | 176 | Original: Stages 1–5 |
| `Footbalcareer.txt` | 168 | **Current:** Stages 1–7, rewritten and expanded |

Below is a literal translation of the **current** revision, followed by the additions relative to the first one. Translator's notes are marked `[TN]`. Nothing is corrected here — typos and all.

---

## Stage 1

A nice intro screen

text

> *[image placeholder: ENTRANCE SCREEN PHOTO]*  
> Afterwards, key settings

Settings *[TN: source says "Atarlar", a typo for "Ayarlar"]* tab

Exit

New game tab → when pressed, a box will appear to name the career, with a confirm button → after confirming, it will spawn into the game

---

## Stage 2

**1 — The game will be 3D, looking from the character's perspective, i.e. first-person view.**

**2 — The facility and the gym interior will also be 3D; the gym trainings may be in the form of mini-games, but to do the training you will go to the 3D place and enter through the objects there — that is, everything in-game will be 3D.**

In-game map

These will of course also be 3D, but these should only be enterable on certain days (match day and training days); on the other kinds of day there should be a day-skipping logic, or the player may enter at their own wish, but can only go to the market.

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

**Bed** → skips the day and refills energy, or the sleep duration can be chosen and energy depends on the hour you slept. In the form:

```
E - SLEEP
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

---

## Stage 4

PLAYER OVERALL

**TECHNICAL** (MAIN HEADING AND FACILITY TRAININGS)

Trainings done in first person at the facility

- **FINISHING** (close-range shot)
- **DRIBBLING** (obstacle course)
- **HEADING** (head it in from a cross)
- **CROSSING** (you get the idea)
- **PASSING** (passing back and forth)
- **TACKLING** (taking the ball off a player with his back turned)
- **LONG SHOTS** (JUMP SQUAD)

**PHYSICAL** (MAIN HEADING AND GYM ACTIVITIES)

- **SPEED** (TREADMILL) → max speed
- **STAMINA** (TREADMILL)
- **BALANCE** (PULL-UP BAR)
- **STRENGTH** (DUMBBELL)
- **JUMPING** (JUMP SQUAD)
- **ACCELERATION** (WEIGHT PUSH) → time to reach max speed

---

## Stage 5

MINI GAME

---

## Stage 6 — *new in this revision*

The match will be played from the character's perspective.

When match day comes and you go out to the match, inside the match `W/A/S/D` will be used, `Shift` will be used; the match can be played like FC 27's open-world mode, but the dribbling/skill moves will be a bit more developed (`shift+shift` to knock the ball on and run, `space` to take a high ball, `ctrl` to slide, `shift` to sprint — these are already known things); there will be combos, i.e. you will be able to do skill moves, like `S+D → A+W` to pull it to your right and go past on your left.

Shooting and crossing will be: mouse looks/aims; right click pass, left click shot.

At the facility, the trainings will **not** be mini-games — they will be in this style — and instead of the player's attribute increasing directly in a single training, they must collect a certain amount of XP for 1 point to increase; for example 1000 to go from 65→66, 1250 for 66→67, increasing by 250 each time.

Trainings can be done **2 times a week**, and in these two sessions there is a right to do **3 trainings each**, but at most **two of these 3 trainings may be the same training**.

In trainings there will be a success rate: XP can be earned as **50/100/150/200/250**, and for the same training done on each different day there is a **1.02× XP multiplier** — i.e. if the first time you do a training you get 250, the second time you will get 255, assuming a top-level performance.

At the same time the player will get a **footballer rating** in matches; it starts at **6.5**; if he plays badly or loses the ball it drops; if he scores, assists or plays well it rises; at the end of the match he earns XP according to the score he got. This XP will **not** directly affect the attributes, but when the player levels up, at level 1 he gets a **0%** training XP multiplier, at level 2 **4%**, at level 3 **8%**, increasing like that — but **levelling up should be hard**.

Match days can be skipped from the interface; training days likewise. **Create a football fixture.**

There will be a **realistic transfer policy and countries**; when the country changes the map changes; there will be the **top 10 active football leagues**; and there will be transfer, contract, wage, and **market value according to the goals and assists he scores and the ratings in matches**.

You can **offer to the club you want to transfer to yourself**, or the **clubs will offer to you**. If the clubs offer, you have a chance regarding the wage; if you offer yourself, you have a chance of being accepted — this is of course according to the strength of the team and the player's market value. This chance thing should be **in the form of spinning a wheel**, and the player should be able to see whether it came or not. **As you increase the wage, the acceptance percentage should drop.** If it is not accepted, if he wants to accept the offer they made, he accepts; if not, he stays at his own team — but this self-offer system should also have a **negative consequence** if it is not accepted: **his relationship with the fans drops, his relationship with the head coach drops**.

---

## Stage 7 — *new in this revision*

The in-game map will be entered **5 or 6 days** [a week]: if the club he plays for has a European match, **6**; if there is no European match, **5** days. These will be **2 gym trainings, 2 facility trainings, and 1 or 2 match days**. On these days, if he wants he can go directly there and do the training, or if there is a match go directly to match time; or if he wants he can start the day in the morning, walk around the city, talk to NPCs, **get a girlfriend**.

---

## Final line of the source

> *(make this game in a way that I can publish it on Steam, and give me its code)*

---

## Translator's notes on the source

| Issue | Detail |
|---|---|
| Typos | "Atarlar" → *Ayarlar*; "Moıbilya" → *mobilya*; "oonayla" → *onayla*; "bilnlar" → *binalar*; "klotuk" → *koltuk*; "diyaznı" → *tasarımı*; "reyting" → *rating*; "fikstirü" → *fikstür*; "sözleşöme" → *sözleşme*; "piyase" → *piyasa*; "traftar" → *taraftar* |
| Empty section | Stage 5 ("MINI GAME") still has a heading and no content |
| Duplicate module | "JUMP SQUAD" names both Long Shots (technical) and Jumping (physical) |
| Ambiguous count | "10 TANE" — 10 drinks in one session, assumed |
| Ambiguous percentage | "130% enerji" — against a 100-point bar, read as 1.3× a standard meal |
| Internal conflict | Stage 6 says training happens "2 times a week, 3 trainings each"; Stage 7 says a week contains "2 gym + 2 facility" sessions. Read as 2 sessions **of each type** per week (GDD `D12`) |
| Unexplained | "day-skipping logic" on non-active days — implemented as the off-day skip (§GDD 4.1) |
| Request to the reader | The final line asks for a Steam-publishable build; treated as the project's delivery target (roadmap M9) |
