# Escape Space Station – תחנת החלל 🚀

משחק חדר-בריחה דו-ממדי, עתידני ואווירתי, בנוי ב-C# עם **MonoGame**. תחנת חלל ניזוקה, כל המערכות קרסו — על השחקן לפתור 10 חידות שונות כדי להחזיר חשמל, לפתוח אזורים חדשים ולהימלט לפני שהזמן אוזל.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![MonoGame](https://img.shields.io/badge/MonoGame-3.8.1-orange)
![Platform](https://img.shields.io/badge/platform-Windows-blue)

---

## 🎮 תיאור המשחק

תחנת החלל שלך ניזוקה קשות. מסדרון מרכזי מחבר בין חמישה אזורים נעולים:

| אזור | חידות |
|---|---|
| **מסדרון מרכזי** (פתוח מההתחלה) | לוח בקרה ראשי, תיקון חיווט חשמלי |
| **חדר בקרה ראשי** | הפעלת רצף מנוע, פענוח קוד בינארי |
| **חדר מנועים** | ניתוב מעגל חשמלי, חיפוש רכיבים נסתרים |
| **מעבדה** | התאמת סמלים עתיקים, חיפוש קוד במסמכים |
| **חדר תקשורת** | כיוונון תדר שידור |
| **חדר חירום** | עקיפת מנעול פתח אוויר (החידה הסופית) |

פתרון שתי החידות בכל אזור פותח את האזור הבא. שני כרטיסי גישה (אדום, כחול) ותא כוח מוענקים אוטומטית בהתקדמות ונדרשים לחידה הסופית. קיים טיימר ספירה לאחור (45 דקות) שיוצר מתח — אם הזמן אוזל לפני שהשחקן בורח, מוצג מסך הפסד.

---

## 🗂️ מבנה הפרויקט

```
EscapeSpaceStation/
├── Game1.cs                    # נקודת חיבור ראשית של MonoGame
├── Program.cs                  # Entry point (Main)
├── EscapeSpaceStation.csproj
├── EscapeSpaceStation.sln
├── Icon.ico
│
├── Systems/                    # תשתית: שירותים, שמירה, מצב משחק
│   ├── AssetManager.cs         # טעינת טקסטורות/סאונד עם נפילה ל-placeholder
│   ├── AudioManager.cs         # מוזיקה + אפקטים
│   ├── SettingsManager.cs      # עוצמת קול (נפרד מקובץ שמירה)
│   ├── SaveManager.cs          # שמירה/טעינה (JSON, %APPDATA%)
│   ├── SceneManager.cs         # מתג בין מסכים
│   ├── Puzzle.cs               # מחלקת בסיס לכל חידה
│   ├── Room.cs                 # מודל חדר + פריט
│   ├── GameState.cs            # מצב משחק חי: חדרים, חידות, מלאי, טיימר
│   └── GameServices.cs         # צרור שירותים משותפים
│
├── Puzzles/                    # 10 החידות
│   ├── ControlPanelPuzzle.cs        # מתגי בקרת חשמל
│   ├── CableWiringPuzzle.cs         # חיבור כבלים לפי צבע
│   ├── SequenceActivationPuzzle.cs  # הפעלת מערכות ברצף נכון
│   ├── BinaryDecodePuzzle.cs        # פענוח קוד בינארי
│   ├── CircuitBreakerPuzzle.cs      # ניתוב מעגל (גריד 3x3)
│   ├── HiddenItemPuzzle.cs          # חיפוש רכיבים נסתרים בחדר
│   ├── SymbolMatchPuzzle.cs         # התאמת סמלים (זיכרון)
│   ├── DocumentCodePuzzle.cs        # מציאת קוד במסמכים
│   ├── FrequencyTunerPuzzle.cs      # כיוון תדר שידור
│   └── AirlockOverridePuzzle.cs     # החידה הסופית
│
├── Scenes/                     # מסכי המשחק
│   ├── MainMenuScene.cs        # תפריט ראשי
│   ├── SettingsScene.cs        # הגדרות קול
│   ├── GameplayScene.cs        # הליבה: חדרים, חידות, HUD
│   ├── VictoryScene.cs         # מסך ניצחון
│   └── DefeatScene.cs          # מסך הפסד
│
├── UI/                         # רכיבי ממשק לשימוש חוזר
│   ├── UiButton.cs              # כפתור לחיץ עם עיצוב סגנון מד"ב
│   └── UiPanel.cs               # פאנל ממוסגר, סורקי מסך, אפקט תקלה
│
├── Content/
│   ├── Content.mgcb            # צינור תוכן (רק לפונטים)
│   ├── Fonts/                  # SpriteFonts עם טווח עברית (U+0590-05FF)
│   ├── Images/{rooms,ui,items,fx}/   # רקעי תמונה (JPG/PNG)
│   └── Audio/{music,sfx}/            # קבצי שמע (OGG/WAV)
│
├── Assets guide.MD             # רשימת כל נכסי הגרפיקה + פרומפטים ליצירה
└── .github/workflows/build.yml # בנייה + Release אוטומטי
```

---

## 🖼️ נכסי גרפיקה (Assets)

כל הפרומפטים המדויקים ליצירת התמונות (עם AI כמו Midjourney/DALL-E) נמצאים ב-[`Assets guide.MD`](./Assets%20guide.MD). ששת תמונות רקע החדרים ושלוש תמונות ה-UI כבר נכללות בפרויקט. שאר הפריטים (אייקונים, אפקטים) עדיין חסרים בכוונה — **המשחק בכל זאת רץ ונבנה תקין בלעדיהם**: `AssetManager` נופל אוטומטית ל-placeholder צבעוני כשקובץ תמונה חסר, ול-"סאונד מדולג" כשקובץ שמע חסר. כך אפשר להריץ ולשחק את המשחק במלואו כבר עכשיו, ולהחליף בהדרגה תמונות/סאונד איכותיים יותר.

קבצי שמע חינמיים מומלצים: [freesound.org](https://freesound.org), [pixabay.com/music](https://pixabay.com/music).

---

## 🛠️ פיתוח מקומי (Windows)

### דרישות מוקדמות
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (אופציונלי) Visual Studio 2022 עם workload ‏".NET Desktop Development"

### הרצה
```powershell
git clone <your-repo-url>
cd EscapeSpaceStation
dotnet restore
dotnet run
```

בפעם הראשונה `dotnet run`/`dotnet build` יתקינו אוטומטית את כלי צינור התוכן של MonoGame (MGCB) הדרוש לקומפילציית הפונטים בעברית, דרך ה-`PackageReference` של `MonoGame.Content.Builder.Task`.

### בניית קובץ EXE נייד (Portable) ידנית
```powershell
dotnet publish EscapeSpaceStation.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish_output
```
קובץ `EscapeSpaceStation.exe` שנוצר הוא **עצמאי לחלוטין** — אינו דורש התקנת .NET Runtime במחשב היעד.

---

## 🤖 בנייה אוטומטית ב-GitHub Actions

בכל `push` ל-`main`, ה-workflow ב-`.github/workflows/build.yml`:
1. רץ על `windows-latest`
2. מתקין .NET 8 SDK וכלי MGCB
3. מריץ `dotnet publish` לבניית קובץ Windows עצמאי (`win-x64`, self-contained, single-file)
4. שוכפל את הפלט לשם `app.exe`
5. יוצר **GitHub Release** חדש עם `app.exe` מצורף כ-asset

לאחר ה-push, גשו ל-**Releases** בריפו כדי להוריד את `app.exe` העדכני ביותר.

---

## 💾 מערכת שמירה

השמירה נכתבת כ-JSON תחת `%APPDATA%\EscapeSpaceStation\savegame.json` וכוללת:
- החדר הנוכחי
- רשימת החידות שנפתרו
- רשימת האזורים הפתוחים
- הפריטים שנאספו
- זמן משחק כולל

הגדרות עוצמת קול (מוזיקה/אפקטים) נשמרות בנפרד תחת `settings.json`, כדי שלא יאבדו כשמתחילים משחק חדש.

---

## 🧩 רשימת 10 החידות

1. **חיבור כבלים** – התאמת כבלים צבעוניים לשקעים הנכונים
2. **פענוח קוד בינארי** – תרגום מחרוזת בינארית ל-ASCII
3. **התאמת סמלים** – חידת זיכרון בסגנון "Simon says"
4. **מציאת קוד במסמכים** – חיפוש קוד גישה חבוי בטקסט
5. **הפעלת רצף** – הפעלת מערכות בסדר בטיחות נכון
6. **מציאת פריטים מוסתרים** – נקודות אינטראקציה נסתרות ברקע החדר
7. **לוח בקרה ראשי** – מתגי הפצת חשמל
8. **ניתוב מעגל חשמלי** – גריד לוגי 3×3
9. **כיוונון תדר** – סליידר לכיוון תדר שידור מדויק
10. **עקיפת מנעול פתח אוויר** – החידה המסכמת, דורשת פריטים מכל האזורים

---

## 📄 רישיון

הקוד בפרויקט זה ניתן לשימוש חופשי. נכסי גרפיקה/שמע שתוסיפו כפופים לרישיון המקור שלהם (בדקו רישיון בעת הורדה מ-freesound/pixabay או תנאי שימוש בכלי ה-AI ביצירת התמונות).
