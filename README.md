# Snake Retro 90s (WinForms .NET 8, tekstowe sprite’y)

Oryginalna gra Snake w klimacie retro, ale **bez binarnych assetów w repo**.
Grafika jest opisana tekstowo w plikach `*.txt` (16x16) + `palette.json`.

## Wymagania
- Windows 10/11
- .NET 8 SDK

## Uruchomienie
```bash
cd SnakeRetro
dotnet run
```

## Sterowanie
- Strzałki / WASD — ruch
- `P` — pauza
- `R` — restart
- Kliknięcie `WALL: ...` — przełączanie:
  - `CLASSIC` (kolizja ze ścianą)
  - `WRAP` (przejście przez krawędź)

## Edycja sprite’ów TXT
Assety są w `SnakeRetro/AssetsSrc/`.

- Każdy sprite to **16 linii po 16 znaków**.
- Każdy znak to kolor z `SnakeRetro/AssetsSrc/palette.json`.
- `.` oznacza przezroczystość.

Przykład fragmentu:
```txt
....KKKKKKKK....
....KwwwwwwK....
....KwwdwwwK....
....KKKKKKKK....
```

Zmiana koloru symbolu odbywa się przez edycję `palette.json`.

## Co jest renderowane tekstowo
- snake: `head/body/turn/tail` w 4 kierunkach
- jabłko blink (2 klatki)
- tilemapa 16x16
- ikony UI

Renderer rysuje piksele jako `FillRectangle(pixelSize)` i zachowuje retro wygląd (kontur, proste cieniowanie, dithering).
