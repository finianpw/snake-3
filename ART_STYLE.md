# ART_STYLE — Snake Retro 90s (tekstowe assety)

## 1) Założenia
- Styl retro inspirowany latami 90: grube kontury, proste cieniowanie, lekki dithering, przygaszone barwy.
- Assety 100% oryginalne.
- W repo **brak binarnych PNG** — cała grafika opisana tekstowo.

## 2) Rozdzielczość i skala
- Wewnętrzna rozdzielczość: `320x300`
  - plansza: `320x240` (20x15 kafli 16x16)
  - UI: `320x60`
- Skala okna: 2x/3x/4x (nearest-neighbor)

## 3) Paleta
Plik: `SnakeRetro/AssetsSrc/palette.json`
- mapuje pojedynczy znak na kolor hex
- `.` = transparent
- przykładowe symbole: `K` (outline), `u/t/s` (cieniowanie snake), `w/v` (jabłko), `g/h/i` (trawa)

## 4) Format sprite’ów TXT
- lokalizacja: `SnakeRetro/AssetsSrc/**.txt`
- każdy sprite ma dokładnie 16x16 znaków
- każdy znak = kolor z palety
- dithering realizowany przez naprzemienne znaki o różnych odcieniach

## 5) Zestaw sprite’ów
- Snake: `AssetsSrc/Snake/{head,body,turn,tail}_{up,right,down,left}.txt`
- Apple: `AssetsSrc/Items/apple_blink_{a,b}.txt`
- Tła: `AssetsSrc/Tiles/{grass_a,grass_b,dirt_a,moss_a,stone_a}.txt`
- UI ikony: `AssetsSrc/UI/{icon_apple,icon_trophy}.txt`

## 6) UI
- Panel rysowany programowo (bevel + dithering)
- bitmapowy font 5x7
- tryb ścian: `WALL: CLASSIC` / `WALL: WRAP`
