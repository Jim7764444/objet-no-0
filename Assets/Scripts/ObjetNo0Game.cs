using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjetNo0Game : MonoBehaviour
{
    private const int StandardFiles = 4;
    private const int TotalFiles = 6;
    private const int BaseFocus = 2;
    private const string ArchiveKey = "ObjetNo0.Archive";
    private const string BestScoreKey = "ObjetNo0.BestScore";
    private const string RunsKey = "ObjetNo0.Runs";
    private const float CanvasWidth = 1600f;
    private const float CanvasHeight = 900f;

    public enum Disposition
    {
        Restituer,
        Archiver,
        Sceller
    }

    private enum ScreenState
    {
        Title,
        File,
        Draft,
        Intermission,
        Report
    }

    private sealed class Evidence
    {
        public readonly string Title;
        public readonly string Observation;

        public Evidence(string title, string observation)
        {
            Title = title;
            Observation = observation;
        }
    }

    private sealed class CaseFile
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly string RecoveryNote;
        public readonly string AnomalyFinding;
        public readonly string ClaimFinding;
        public readonly string DetailFinding;
        public readonly bool IsAnomalous;
        public readonly bool HasVerifiedClaim;

        public CaseFile(
            string id,
            string name,
            string description,
            string recoveryNote,
            string anomalyFinding,
            string claimFinding,
            string detailFinding,
            bool isAnomalous,
            bool hasVerifiedClaim)
        {
            Id = id;
            Name = name;
            Description = description;
            RecoveryNote = recoveryNote;
            AnomalyFinding = anomalyFinding;
            ClaimFinding = claimFinding;
            DetailFinding = detailFinding;
            IsAnomalous = isAnomalous;
            HasVerifiedClaim = hasVerifiedClaim;
        }

        public Disposition CorrectDisposition
        {
            get { return RequiredDisposition(IsAnomalous, HasVerifiedClaim); }
        }

        public Evidence GetEvidence(int index)
        {
            switch (index)
            {
                case 0:
                    return new Evidence("LANTERNE DE CONTRÔLE", AnomalyFinding);
                case 1:
                    return new Evidence("REGISTRE NOMINATIF", ClaimFinding);
                default:
                    return new Evidence("EXAMEN DES TRACES", DetailFinding);
            }
        }
    }

    private sealed class Doctrine
    {
        public readonly string Name;
        public readonly string Description;
        public readonly int UnlockAt;
        public readonly Disposition? BonusDisposition;

        public Doctrine(string name, string description, int unlockAt, Disposition? bonusDisposition)
        {
            Name = name;
            Description = description;
            UnlockAt = unlockAt;
            BonusDisposition = bonusDisposition;
        }
    }

    private sealed class Upgrade
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;

        public Upgrade(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }

    private static readonly CaseFile[] CaseCatalog =
    {
        new CaseFile(
            "A-014", "La tasse de veille",
            "Une tasse ébréchée, récupérée sous le plancher d'une chambre murée.",
            "Le fond est couvert d'une poussière qui sent la pluie froide.",
            "L'ombre suit la lanterne et garde une forme ordinaire.",
            "Aucun nom ni sceau de réclamation ne correspond au registre.",
            "Une ligne de sel s'est déposée à l'intérieur, sans autre effet.",
            false, false),
        new CaseFile(
            "B-022", "La clé de Madame Vey",
            "Une clé de laiton remise par une femme qui affirme l'avoir perdue enfant.",
            "Le métal est tiède, même après une nuit dans le coffre.",
            "La clé projette une seule ombre, dans le bon sens.",
            "Le sceau de Madame Vey correspond à deux reçus antérieurs.",
            "La dent la plus usée porte des initiales gravées à l'envers.",
            false, true),
        new CaseFile(
            "C-031", "La poire de cire",
            "Une petite poire jaune, parfaitement conservée dans un panier vide.",
            "Le panier a été retrouvé au milieu d'un puits condamné.",
            "L'ombre pointe vers la lanterne, quelle que soit sa position.",
            "Aucune réclamation n'a été déposée.",
            "La cire sent le fruit mûr, mais ne laisse aucune empreinte.",
            true, false),
        new CaseFile(
            "D-009", "Le portrait retourné",
            "Un portrait de famille dont les visages sont tournés vers le mur.",
            "Une fiche de réclamation était agrafée au cadre.",
            "Le reflet montre la pièce vide, même lorsque quelqu'un s'y tient.",
            "Le sceau familial est authentique et la demande est encore active.",
            "Le bois du cadre est sec; le revers est couvert de moisissure.",
            true, true),
        new CaseFile(
            "E-044", "La boussole fendue",
            "Une boussole arrêtée, apportée par un coursier sans adresse de retour.",
            "L'aiguille ne tremble qu'à proximité des archives fermées.",
            "L'aiguille indique le nord; son ombre suit la lumière normalement.",
            "Le numéro de livraison est faux et aucun propriétaire ne répond.",
            "Une fente traverse le verre sans toucher le cadran.",
            false, false),
        new CaseFile(
            "F-003", "La cloche muette",
            "Une petite cloche de cuivre retrouvée dans un puits sans fond visible.",
            "Personne ne l'a entendue sonner, mais trois témoins ont sursauté.",
            "Elle sonne dans la lanterne, bien qu'aucun son ne soit produit.",
            "Le nom du dernier détenteur est rayé du registre depuis longtemps.",
            "Une poussière noire tombe de l'intérieur à chaque inspection.",
            true, false)
    };

    private static readonly Doctrine[] Doctrines =
    {
        new Doctrine("CODE GRIS", "Aucun parti pris. Le bureau applique le protocole à la lettre.", 0, null),
        new Doctrine("MANDAT DES PROCHES", "+60 points pour une restitution correcte.", 4, Disposition.Restituer),
        new Doctrine("VEILLEUSE NOIRE", "+60 points pour un scellement correct.", 8, Disposition.Sceller),
        new Doctrine("MAIN DE PAPIER", "+60 points pour un archivage correct.", 12, Disposition.Archiver)
    };

    private static readonly Upgrade[] AvailableUpgrades =
    {
        new Upgrade("loupe", "LOUPE À FUMÉE", "La première pièce à conviction de chaque dossier ne coûte aucun point de concentration."),
        new Upgrade("buvard", "BUVARD D'ÉCHO", "+1 point de concentration par dossier."),
        new Upgrade("sceau", "SCEAU DE CUIVRE", "+70 points pour chaque anomalie correctement scellée."),
        new Upgrade("rabat", "RABAT DE SÛRETÉ", "La première erreur du service ne fait perdre aucun point de stabilité.")
    };

    private static readonly Color Ink = new Color(0.055f, 0.067f, 0.067f);
    private static readonly Color Panel = new Color(0.10f, 0.12f, 0.115f);
    private static readonly Color PanelRaised = new Color(0.14f, 0.16f, 0.145f);
    private static readonly Color Paper = new Color(0.83f, 0.78f, 0.65f);
    private static readonly Color Muted = new Color(0.57f, 0.57f, 0.48f);
    private static readonly Color Rust = new Color(0.67f, 0.30f, 0.20f);
    private static readonly Color Brass = new Color(0.72f, 0.56f, 0.30f);
    private static readonly Color SickGreen = new Color(0.47f, 0.56f, 0.39f);
    private static readonly Color Danger = new Color(0.70f, 0.23f, 0.18f);

    private readonly List<int> chosenUpgrades = new List<int>();
    private readonly List<int> fileOrder = new List<int>();
    private readonly bool[] revealedEvidence = new bool[3];

    private Texture2D whiteTexture;
    private GUIStyle titleStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle monoStyle;
    private GUIStyle buttonTextStyle;
    private GUIStyle invisibleButtonStyle;

    private ScreenState screen = ScreenState.Title;
    private int caseIndex;
    private int focus;
    private int stability;
    private int correctFiles;
    private int completedFiles;
    private int currentStreak;
    private int bestStreak;
    private int selectedDoctrine;
    private int runScore;
    private int freeInspectionsUsed;
    private bool usedSafetyRabat;
    private bool resolved;
    private bool paused;
    private string resultText = string.Empty;
    private string resultDetail = string.Empty;
    private string lastMistake = string.Empty;
    private int lastAward;
    private CaseFile CurrentCase
    {
        get { return CaseCatalog[fileOrder[caseIndex]]; }
    }

    private int ArchiveCount
    {
        get { return PlayerPrefs.GetInt(ArchiveKey, 0); }
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    private void OnDestroy()
    {
        if (whiteTexture != null)
            Destroy(whiteTexture);
    }

    private void OnGUI()
    {
        EnsureStyles();

        float scale = Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight);
        float offsetX = (Screen.width - CanvasWidth * scale) * 0.5f;
        float offsetY = (Screen.height - CanvasHeight * scale) * 0.5f;
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        GUI.matrix = Matrix4x4.TRS(
            new Vector3(offsetX, offsetY, 0f),
            Quaternion.identity,
            new Vector3(scale, scale, 1f));

        Fill(new Rect(0f, 0f, CanvasWidth, CanvasHeight), Ink);
        DrawGrain();
        HandleKeyboard();

        switch (screen)
        {
            case ScreenState.Title:
                DrawTitle();
                break;
            case ScreenState.File:
                DrawFileScreen();
                break;
            case ScreenState.Draft:
                DrawDraftScreen();
                break;
            case ScreenState.Intermission:
                DrawIntermission();
                break;
            case ScreenState.Report:
                DrawReport();
                break;
        }

        if (paused)
            DrawPauseOverlay();

        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    public static bool ValidateCaseCatalog(out string message)
    {
        if (RequiredDisposition(true, true) != Disposition.Sceller ||
            RequiredDisposition(true, false) != Disposition.Sceller ||
            RequiredDisposition(false, true) != Disposition.Restituer ||
            RequiredDisposition(false, false) != Disposition.Archiver)
        {
            message = "The classification priority table is inconsistent.";
            return false;
        }

        if (CaseCatalog.Length < TotalFiles)
        {
            message = "The catalog does not contain enough files for a full shift.";
            return false;
        }

        var ids = new HashSet<string>();
        int returns = 0;
        int archives = 0;
        int seals = 0;

        foreach (CaseFile file in CaseCatalog)
        {
            if (string.IsNullOrEmpty(file.Id) || !ids.Add(file.Id))
            {
                message = "Case identifiers must be present and unique.";
                return false;
            }

            if (string.IsNullOrEmpty(file.Name) ||
                string.IsNullOrEmpty(file.Description) ||
                string.IsNullOrEmpty(file.AnomalyFinding) ||
                string.IsNullOrEmpty(file.ClaimFinding) ||
                string.IsNullOrEmpty(file.DetailFinding))
            {
                message = "Every case needs a description and three findings.";
                return false;
            }

            switch (file.CorrectDisposition)
            {
                case Disposition.Restituer:
                    returns++;
                    break;
                case Disposition.Archiver:
                    archives++;
                    break;
                case Disposition.Sceller:
                    seals++;
                    break;
            }
        }

        if (returns == 0 || archives == 0 || seals == 0)
        {
            message = "The catalog must exercise all three dispositions.";
            return false;
        }

        message = string.Format(
            "Validated {0} unique files across all three dispositions.",
            CaseCatalog.Length);
        return true;
    }

    public static Disposition RequiredDisposition(bool isAnomalous, bool hasVerifiedClaim)
    {
        if (isAnomalous)
            return Disposition.Sceller;
        return hasVerifiedClaim ? Disposition.Restituer : Disposition.Archiver;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = CreateStyle(56, Paper, FontStyle.Bold, TextAnchor.MiddleLeft);
        headingStyle = CreateStyle(24, Paper, FontStyle.Bold, TextAnchor.MiddleLeft);
        bodyStyle = CreateStyle(19, Paper, FontStyle.Normal, TextAnchor.UpperLeft);
        smallStyle = CreateStyle(15, Muted, FontStyle.Normal, TextAnchor.UpperLeft);
        monoStyle = CreateStyle(17, Brass, FontStyle.Bold, TextAnchor.MiddleLeft);
        buttonTextStyle = CreateStyle(18, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
        invisibleButtonStyle = new GUIStyle(GUIStyle.none);
    }

    private GUIStyle CreateStyle(int size, Color color, FontStyle weight, TextAnchor anchor)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = weight,
            alignment = anchor,
            wordWrap = true,
            richText = false
        };
        style.normal.textColor = color;
        style.hover.textColor = color;
        return style;
    }

    private void DrawGrain()
    {
        Fill(new Rect(0f, 0f, CanvasWidth, 2f), new Color(0.42f, 0.27f, 0.15f, 0.65f));
        for (int i = 0; i < 16; i++)
        {
            float x = 70f + i * 97f;
            float y = 110f + ((i * 73) % 680);
            Fill(new Rect(x, y, 2f, 2f), new Color(0.75f, 0.64f, 0.43f, 0.10f));
        }
    }

    private void HandleKeyboard()
    {
        Event currentEvent = Event.current;
        if (currentEvent.type != EventType.KeyDown || currentEvent.keyCode != KeyCode.Escape)
            return;

        if (screen == ScreenState.File && !resolved)
            paused = !paused;
        else if (paused)
            paused = false;

        currentEvent.Use();
    }

    private void DrawTitle()
    {
        DrawHeader("REGISTRE CENTRAL  /  SERVICE DE NUIT");
        DrawPanel(new Rect(76f, 156f, 920f, 670f), Panel);
        DrawPanel(new Rect(1020f, 156f, 504f, 670f), PanelRaised);

        DrawText(new Rect(120f, 190f, 800f, 30f), "BUREAU DES OBJETS IMPOSSIBLES", monoStyle);
        DrawText(new Rect(120f, 234f, 800f, 76f), "OBJET N°0", titleStyle);
        Fill(new Rect(120f, 324f, 640f, 2f), Rust);
        DrawText(
            new Rect(120f, 352f, 740f, 112f),
            "Tout objet a une place.\nMême ceux qui n'auraient jamais dû exister.",
            headingStyle);
        DrawText(
            new Rect(120f, 494f, 735f, 91f),
            "Vous êtes le dernier commis d'un bureau sans adresse. Examinez les preuves, appliquez le protocole et décidez du sort de chaque trouvaille.",
            bodyStyle);

        DrawArtifact(new Rect(748f, 540f, 180f, 180f), (PlayerPrefs.GetInt(RunsKey, 0) + ArchiveCount) % 6);
        DrawText(new Rect(120f, 618f, 580f, 27f), "DÉCIDEZ. TENEZ. RECOMMENCEZ.", monoStyle);
        DrawButton(
            new Rect(120f, 690f, 430f, 68f),
            "OUVRIR UN NOUVEAU SERVICE",
            true,
            Rust,
            BeginShift);

        DrawText(new Rect(1060f, 196f, 400f, 26f), "PROTOCOLE DE SERVICE", monoStyle);
        DrawText(
            new Rect(1060f, 239f, 418f, 83f),
            CurrentDoctrine().Name + "\n" + CurrentDoctrine().Description,
            bodyStyle);
        DrawButton(
            new Rect(1060f, 340f, 418f, 54f),
            "CHANGER DE DOCTRINE",
            UnlockedDoctrineCount() > 1,
            Panel,
            CycleDoctrine);

        Fill(new Rect(1060f, 420f, 418f, 1f), new Color(0.4f, 0.37f, 0.29f));
        DrawText(new Rect(1060f, 448f, 400f, 27f), "REGISTRE PERSONNEL", monoStyle);
        DrawStat(new Rect(1060f, 495f, 418f, 32f), "DOSSIERS CONSERVÉS", ArchiveCount.ToString("D2"));
        DrawStat(new Rect(1060f, 541f, 418f, 32f), "MEILLEUR SERVICE", PlayerPrefs.GetInt(BestScoreKey, 0).ToString("D4"));
        DrawStat(new Rect(1060f, 587f, 418f, 32f), "SERVICES TERMINÉS", PlayerPrefs.GetInt(RunsKey, 0).ToString("D2"));

        int nextUnlock = NextDoctrineUnlock();
        string archiveProgress = nextUnlock < 0
            ? "Tous les protocoles sont disponibles."
            : string.Format("Prochain protocole : {0} / {1} dossiers.", ArchiveCount, nextUnlock);
        DrawText(new Rect(1060f, 648f, 416f, 47f), archiveProgress, smallStyle);
        DrawText(new Rect(1060f, 752f, 416f, 40f), "Une vérité claire. Trois décisions. Six dossiers.", smallStyle);
        DrawText(new Rect(76f, 847f, 900f, 24f), "CLIQUEZ POUR AGIR  ·  ÉCHAP POUR SUSPENDRE  ·  BUILD ALPHA", smallStyle);
    }

    private void DrawFileScreen()
    {
        DrawHeader(string.Format("SERVICE {0:D2}  /  DOSSIER {1:D2}-{2:D2}", PlayerPrefs.GetInt(RunsKey, 0) + 1, caseIndex + 1, TotalFiles));
        DrawPanel(new Rect(64f, 142f, 326f, 690f), Panel);
        DrawPanel(new Rect(410f, 142f, 682f, 690f), Panel);
        DrawPanel(new Rect(1112f, 142f, 424f, 690f), Panel);

        DrawText(new Rect(90f, 168f, 270f, 25f), "RÈGLES DE CLASSEMENT", monoStyle);
        DrawRuleRow(90f, 220f, "01", "SCELLER", "Toute anomalie confirmée.", Danger);
        DrawRuleRow(90f, 313f, "02", "RESTITUER", "Réclamation vérifiée, sans anomalie.", Brass);
        DrawRuleRow(90f, 424f, "03", "ARCHIVER", "Tout objet ordinaire sans propriétaire.", SickGreen);
        Fill(new Rect(90f, 531f, 272f, 1f), new Color(0.38f, 0.35f, 0.28f));
        DrawText(new Rect(90f, 555f, 270f, 27f), "ÉTAT DU BUREAU", monoStyle);
        DrawStability(new Rect(90f, 598f, 270f, 26f));
        DrawStat(new Rect(90f, 644f, 272f, 29f), "CONCENTRATION", focus.ToString("D2"));
        DrawStat(new Rect(90f, 686f, 272f, 29f), "SÉRIE CORRECTE", currentStreak.ToString("D2"));
        DrawText(
            new Rect(90f, 749f, 270f, 53f),
            "Une anomalie prime sur toute demande de restitution.",
            smallStyle);

        CaseFile file = CurrentCase;
        DrawText(new Rect(444f, 169f, 250f, 24f), file.Id + "  /  FICHE DE TROUVAILLE", monoStyle);
        DrawText(new Rect(444f, 211f, 610f, 42f), file.Name, headingStyle);
        DrawText(new Rect(444f, 261f, 610f, 56f), file.Description, bodyStyle);
        DrawArtifact(new Rect(444f, 338f, 220f, 202f), fileOrder[caseIndex]);
        DrawText(new Rect(692f, 350f, 355f, 23f), "NOTE DE RÉCUPÉRATION", monoStyle);
        DrawText(new Rect(692f, 386f, 352f, 101f), file.RecoveryNote, bodyStyle);
        DrawText(new Rect(692f, 493f, 350f, 24f), "SCORE : " + runScore.ToString("D4"), smallStyle);

        if (!resolved)
        {
            DrawText(new Rect(444f, 562f, 610f, 26f), "VOTRE DÉCISION", monoStyle);
            DrawDispositionButton(new Rect(444f, 604f, 190f, 75f), "RESTITUER", Disposition.Restituer, Brass);
            DrawDispositionButton(new Rect(648f, 604f, 190f, 75f), "ARCHIVER", Disposition.Archiver, SickGreen);
            DrawDispositionButton(new Rect(852f, 604f, 190f, 75f), "SCELLER", Disposition.Sceller, Danger);
            DrawText(new Rect(444f, 708f, 600f, 69f), "Choisissez selon les preuves. Une erreur sur une anomalie coûte deux points de stabilité.", smallStyle);
        }
        else
        {
            Color resultColor = lastAward > 0 ? SickGreen : Danger;
            DrawPanel(new Rect(444f, 565f, 610f, 156f), new Color(resultColor.r, resultColor.g, resultColor.b, 0.16f));
            DrawText(new Rect(466f, 580f, 562f, 31f), resultText, headingStyle);
            DrawText(new Rect(466f, 620f, 562f, 59f), resultDetail, bodyStyle);
            DrawButton(
                new Rect(444f, 741f, 610f, 58f),
                NextFileButtonText(),
                true,
                Rust,
                AdvanceAfterFile);
        }

        DrawText(new Rect(1142f, 169f, 350f, 25f), "PIÈCES À CONVICTION", monoStyle);
        DrawText(
            new Rect(1142f, 201f, 355f, 45f),
            string.Format("{0} points de concentration disponibles.", focus),
            smallStyle);
        DrawEvidenceCard(0, new Rect(1142f, 269f, 364f, 148f));
        DrawEvidenceCard(1, new Rect(1142f, 430f, 364f, 148f));
        DrawEvidenceCard(2, new Rect(1142f, 591f, 364f, 148f));
        DrawText(new Rect(1142f, 772f, 364f, 32f), "Un détail n'est pas toujours une preuve.", smallStyle);
    }

    private void DrawDraftScreen()
    {
        DrawHeader("PAUSE ENTRE LES DOSSIERS  /  RÉVISION DU PROTOCOLE");
        DrawPanel(new Rect(190f, 164f, 1220f, 645f), Panel);
        DrawText(new Rect(242f, 207f, 1100f, 28f), "DEUX DOSSIERS SONT CLOS. LE BUREAU VOUS ACCORDE UN OUTIL.", monoStyle);
        DrawText(new Rect(242f, 254f, 1080f, 48f), "Choisissez un seul ajout au protocole.", titleStyle);

        List<int> options = GetAvailableUpgrades();
        for (int i = 0; i < options.Count; i++)
        {
            int upgradeIndex = options[i];
            float y = 348f + i * 102f;
            Upgrade upgrade = AvailableUpgrades[upgradeIndex];
            DrawPanel(new Rect(242f, y, 1092f, 80f), PanelRaised);
            DrawText(new Rect(270f, y + 12f, 345f, 28f), upgrade.Name, headingStyle);
            DrawText(new Rect(630f, y + 11f, 480f, 55f), upgrade.Description, bodyStyle);
            DrawButton(
                new Rect(1155f, y + 11f, 150f, 56f),
                "ADOPTER",
                true,
                Rust,
                delegate { SelectUpgrade(upgradeIndex); });
        }
    }

    private void DrawIntermission()
    {
        DrawHeader("SERVICE STANDARD TERMINÉ  /  DÉCISION DE GARDE");
        DrawPanel(new Rect(170f, 150f, 1260f, 682f), Panel);
        DrawText(new Rect(228f, 206f, 1100f, 30f), "QUATRE DOSSIERS CLASSÉS", monoStyle);
        DrawText(new Rect(228f, 259f, 1050f, 54f), "Le jour approche. Le registre est prêt à être scellé.", titleStyle);
        DrawText(
            new Rect(228f, 337f, 1040f, 93f),
            string.Format(
                "Vous avez conservé {0} dossier(s), gagné {1} points et maintenu {2} point(s) de stabilité.",
                correctFiles,
                runScore,
                stability),
            bodyStyle);
        Fill(new Rect(228f, 456f, 1118f, 1f), new Color(0.4f, 0.37f, 0.29f));
        DrawText(new Rect(228f, 486f, 1000f, 28f), "FERMER LE SERVICE", monoStyle);
        DrawText(new Rect(228f, 522f, 1020f, 54f), "Le score est acquis. Rentrez avec vos dossiers intacts.", bodyStyle);
        DrawButton(
            new Rect(228f, 594f, 490f, 72f),
            "SCELLER LE REGISTRE",
            true,
            SickGreen,
            FinishShift);
        DrawText(new Rect(228f, 698f, 1000f, 25f), "OU RISQUER UNE ANNEXE INTERDITE", monoStyle);
        DrawText(
            new Rect(228f, 733f, 1020f, 54f),
            "Deux objets supplémentaires. Un échec met fin au service et réduit le score final.",
            bodyStyle);
        DrawButton(
            new Rect(892f, 594f, 454f, 72f),
            "OUVRIR L'ANNEXE",
            stability > 0,
            Rust,
            OpenAnnex);
        DrawText(new Rect(892f, 685f, 454f, 64f), "La décision vous appartient. Aucun bonus ne compense une erreur de confinement.", smallStyle);
    }

    private void DrawReport()
    {
        DrawHeader("RAPPORT DE FIN DE SERVICE");
        DrawPanel(new Rect(200f, 150f, 1200f, 682f), Panel);
        bool cleanFinish = completedFiles >= StandardFiles && stability > 0;
        string headline = cleanFinish ? "LE REGISTRE EST SCELLÉ." : "LE BUREAU A CESSÉ DE RÉPONDRE.";
        DrawText(new Rect(255f, 205f, 1060f, 64f), headline, titleStyle);
        DrawText(
            new Rect(255f, 283f, 1060f, 52f),
            cleanFinish
                ? "Les portes tiennent. Pour l'instant."
                : "Une décision a laissé entrer quelque chose qui n'avait pas de nom.",
            bodyStyle);

        DrawPanel(new Rect(255f, 373f, 500f, 261f), PanelRaised);
        DrawPanel(new Rect(790f, 373f, 555f, 261f), PanelRaised);
        DrawStat(new Rect(290f, 412f, 425f, 34f), "SCORE", runScore.ToString("D4"));
        DrawStat(new Rect(290f, 466f, 425f, 34f), "DOSSIERS JUSTES", correctFiles.ToString("D2") + " / " + completedFiles.ToString("D2"));
        DrawStat(new Rect(290f, 520f, 425f, 34f), "MEILLEURE SÉRIE", bestStreak.ToString("D2"));
        DrawStat(new Rect(290f, 574f, 425f, 34f), "STABILITÉ RESTANTE", stability.ToString("D2"));
        DrawText(new Rect(828f, 412f, 480f, 28f), "NOTE D'ARCHIVE", monoStyle);
        DrawText(
            new Rect(828f, 459f, 470f, 107f),
            string.IsNullOrEmpty(lastMistake)
                ? "Aucune erreur consignée. Le bureau n'oublie pourtant rien."
                : lastMistake,
            bodyStyle);
        DrawText(
            new Rect(828f, 581f, 470f, 39f),
            string.Format("+{0} dossier(s) conservé(s) au registre permanent.", correctFiles),
            smallStyle);

        DrawButton(
            new Rect(255f, 687f, 500f, 69f),
            "RETOURNER AU BUREAU",
            true,
            Rust,
            ReturnToTitle);
        DrawButton(
            new Rect(790f, 687f, 555f, 69f),
            "RECOMMENCER UN SERVICE",
            true,
            PanelRaised,
            BeginShift);
    }

    private void DrawPauseOverlay()
    {
        Fill(new Rect(0f, 0f, CanvasWidth, CanvasHeight), new Color(0f, 0f, 0f, 0.72f));
        DrawPanel(new Rect(480f, 306f, 640f, 260f), PanelRaised);
        DrawText(new Rect(530f, 348f, 540f, 50f), "DOSSIER EN SUSPENS", headingStyle);
        DrawText(new Rect(530f, 410f, 540f, 36f), "Le registre attendra.", bodyStyle);
        DrawButton(new Rect(530f, 477f, 540f, 55f), "REPRENDRE  /  ÉCHAP", true, Rust, delegate { paused = false; });
    }

    private void DrawHeader(string rightLabel)
    {
        DrawText(new Rect(66f, 43f, 500f, 29f), "Q.P.L.  /  BUREAU DES OBJETS IMPOSSIBLES", monoStyle);
        DrawText(new Rect(1035f, 43f, 500f, 29f), rightLabel, smallStyle);
        Fill(new Rect(66f, 91f, 1468f, 1f), new Color(0.37f, 0.34f, 0.27f));
    }

    private void DrawRuleRow(float x, float y, string number, string title, string rule, Color color)
    {
        DrawText(new Rect(x, y, 36f, 23f), number, smallStyle);
        DrawText(new Rect(x + 43f, y - 2f, 220f, 26f), title, CreateStyle(17, color, FontStyle.Bold, TextAnchor.MiddleLeft));
        DrawText(new Rect(x + 43f, y + 30f, 225f, 51f), rule, smallStyle);
    }

    private void DrawEvidenceCard(int index, Rect rect)
    {
        Evidence evidence = CurrentCase.GetEvidence(index);
        bool isRevealed = revealedEvidence[index];
        DrawPanel(rect, isRevealed ? new Color(0.17f, 0.18f, 0.15f) : PanelRaised);
        DrawText(new Rect(rect.x + 17f, rect.y + 13f, rect.width - 34f, 25f), evidence.Title, monoStyle);

        if (isRevealed)
        {
            DrawText(new Rect(rect.x + 17f, rect.y + 49f, rect.width - 34f, 77f), evidence.Observation, bodyStyle);
            return;
        }

        DrawText(new Rect(rect.x + 17f, rect.y + 50f, rect.width - 34f, 22f), "Contenu sous enveloppe.", smallStyle);
        string action = HasUpgrade("loupe") && freeInspectionsUsed == 0 ? "EXAMINER  /  GRATUIT" : "EXAMINER  /  1 CONCENTRATION";
        DrawButton(
            new Rect(rect.x + 17f, rect.y + 83f, rect.width - 34f, 46f),
            action,
            focus > 0 || (HasUpgrade("loupe") && freeInspectionsUsed == 0),
            Rust,
            delegate { RevealEvidence(index); });
    }

    private void DrawDispositionButton(Rect rect, string label, Disposition disposition, Color color)
    {
        DrawButton(rect, label, true, color, delegate { ResolveCase(disposition); });
    }

    private void DrawStability(Rect rect)
    {
        for (int i = 0; i < 3; i++)
        {
            Color color = i < stability ? (i == 0 ? Danger : Brass) : new Color(0.20f, 0.20f, 0.17f);
            Fill(new Rect(rect.x + i * 47f, rect.y, 36f, rect.height), color);
        }
    }

    private void DrawStat(Rect rect, string label, string value)
    {
        DrawText(new Rect(rect.x, rect.y, rect.width * 0.67f, rect.height), label, smallStyle);
        DrawText(new Rect(rect.x + rect.width * 0.68f, rect.y, rect.width * 0.32f, rect.height), value, monoStyle);
    }

    private void DrawArtifact(Rect rect, int seed)
    {
        DrawPanel(rect, new Color(0.055f, 0.067f, 0.061f));
        Fill(new Rect(rect.x + 12f, rect.y + rect.height - 25f, rect.width - 24f, 1f), new Color(0.40f, 0.35f, 0.25f));

        Color objectColor = seed % 2 == 0 ? Brass : new Color(0.48f, 0.55f, 0.38f);
        float cx = rect.x + rect.width * 0.5f;
        float cy = rect.y + rect.height * 0.49f;

        switch (seed % 6)
        {
            case 0:
                DrawDisc(cx, cy - 14f, 34f, objectColor);
                Fill(new Rect(cx - 29f, cy + 6f, 58f, 43f), objectColor);
                Fill(new Rect(cx - 20f, cy + 49f, 40f, 8f), objectColor);
                Fill(new Rect(cx - 44f, cy + 58f, 88f, 6f), objectColor);
                break;
            case 1:
                Fill(new Rect(cx - 12f, cy - 49f, 24f, 93f), objectColor);
                DrawDisc(cx, cy - 44f, 26f, objectColor);
                DrawDisc(cx, cy - 44f, 12f, new Color(0.055f, 0.067f, 0.061f));
                Fill(new Rect(cx + 9f, cy + 26f, 37f, 13f), objectColor);
                Fill(new Rect(cx + 33f, cy + 29f, 11f, 29f), objectColor);
                Fill(new Rect(cx - 40f, cy + 61f, 80f, 5f), new Color(0.38f, 0.26f, 0.14f));
                break;
            case 2:
                Fill(new Rect(cx - 39f, cy - 27f, 78f, 71f), objectColor);
                Fill(new Rect(cx - 26f, cy - 38f, 52f, 14f), Brass);
                Fill(new Rect(cx - 8f, cy - 50f, 16f, 12f), Brass);
                Fill(new Rect(cx - 47f, cy + 43f, 94f, 8f), new Color(0.38f, 0.26f, 0.14f));
                break;
            case 3:
                Fill(new Rect(cx - 48f, cy - 41f, 96f, 83f), new Color(0.23f, 0.25f, 0.22f));
                Fill(new Rect(cx - 35f, cy - 28f, 70f, 57f), objectColor);
                DrawDisc(cx - 17f, cy - 7f, 5f, Ink);
                DrawDisc(cx + 17f, cy - 7f, 5f, Ink);
                Fill(new Rect(cx - 20f, cy + 12f, 40f, 4f), Ink);
                Fill(new Rect(cx - 55f, cy + 51f, 110f, 5f), Brass);
                break;
            case 4:
                DrawDisc(cx, cy, 47f, objectColor);
                DrawDisc(cx, cy, 33f, new Color(0.12f, 0.13f, 0.11f));
                DrawDisc(cx, cy, 6f, Brass);
                Fill(new Rect(cx - 3f, cy - 40f, 6f, 79f), objectColor);
                Fill(new Rect(cx - 33f, cy - 3f, 66f, 6f), objectColor);
                break;
            default:
                Fill(new Rect(cx - 31f, cy - 39f, 62f, 79f), objectColor);
                Fill(new Rect(cx - 42f, cy - 48f, 84f, 13f), Brass);
                Fill(new Rect(cx - 9f, cy - 64f, 18f, 16f), Brass);
                DrawDisc(cx, cy - 4f, 13f, new Color(0.13f, 0.14f, 0.12f));
                Fill(new Rect(cx - 48f, cy + 45f, 96f, 6f), new Color(0.38f, 0.26f, 0.14f));
                break;
        }
    }

    private void DrawDisc(float centerX, float centerY, float radius, Color color)
    {
        for (int row = -Mathf.CeilToInt(radius); row <= Mathf.CeilToInt(radius); row += 3)
        {
            float width = Mathf.Sqrt(Mathf.Max(0f, radius * radius - row * row)) * 2f;
            Fill(new Rect(centerX - width * 0.5f, centerY + row, width, 3f), color);
        }
    }

    private void DrawPanel(Rect rect, Color color)
    {
        Fill(rect, color);
        Fill(new Rect(rect.x, rect.y, rect.width, 1f), new Color(0.47f, 0.40f, 0.28f, 0.75f));
        Fill(new Rect(rect.x, rect.y, 1f, rect.height), new Color(0.47f, 0.40f, 0.28f, 0.5f));
        Fill(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0f, 0f, 0f, 0.45f));
        Fill(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), new Color(0f, 0f, 0f, 0.45f));
    }

    private void Fill(Rect rect, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, whiteTexture);
        GUI.color = oldColor;
    }

    private void DrawText(Rect rect, string text, GUIStyle style)
    {
        GUI.Label(rect, text, style);
    }

    private void DrawButton(Rect rect, string label, bool enabled, Color color, Action onClick)
    {
        Event currentEvent = Event.current;
        bool hover = enabled && currentEvent.type == EventType.Repaint && rect.Contains(currentEvent.mousePosition);
        Color baseColor = enabled ? color : new Color(0.24f, 0.24f, 0.21f);
        if (hover)
            baseColor = Color.Lerp(baseColor, Paper, 0.18f);
        Fill(rect, baseColor);
        Fill(new Rect(rect.x, rect.y, rect.width, 2f), new Color(0f, 0f, 0f, 0.28f));
        DrawText(rect, label, buttonTextStyle);

        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0f);
        bool clicked = GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
        GUI.color = oldColor;
        if (clicked && enabled && onClick != null)
            onClick();
    }

    private void BeginShift()
    {
        selectedDoctrine = Mathf.Clamp(selectedDoctrine, 0, UnlockedDoctrineCount() - 1);
        fileOrder.Clear();
        for (int i = 0; i < CaseCatalog.Length; i++)
            fileOrder.Add(i);

        for (int i = fileOrder.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            int value = fileOrder[i];
            fileOrder[i] = fileOrder[swapIndex];
            fileOrder[swapIndex] = value;
        }

        chosenUpgrades.Clear();
        caseIndex = 0;
        correctFiles = 0;
        completedFiles = 0;
        currentStreak = 0;
        bestStreak = 0;
        runScore = 0;
        stability = 3;
        usedSafetyRabat = false;
        lastMistake = string.Empty;
        screen = ScreenState.File;
        LoadCurrentFile();
    }

    private void LoadCurrentFile()
    {
        Array.Clear(revealedEvidence, 0, revealedEvidence.Length);
        focus = BaseFocus + (HasUpgrade("buvard") ? 1 : 0);
        freeInspectionsUsed = 0;
        resolved = false;
        paused = false;
        lastAward = 0;
        resultText = string.Empty;
        resultDetail = string.Empty;
    }

    private void RevealEvidence(int index)
    {
        if (paused || resolved || index < 0 || index >= revealedEvidence.Length || revealedEvidence[index])
            return;

        bool free = HasUpgrade("loupe") && freeInspectionsUsed == 0;
        if (focus <= 0 && !free)
            return;

        revealedEvidence[index] = true;
        if (free)
            freeInspectionsUsed++;
        else
            focus--;
    }

    private void ResolveCase(Disposition choice)
    {
        if (paused || resolved)
            return;

        CaseFile file = CurrentCase;
        completedFiles++;
        bool correct = choice == file.CorrectDisposition;
        resolved = true;

        if (correct)
        {
            correctFiles++;
            currentStreak++;
            bestStreak = Mathf.Max(bestStreak, currentStreak);
            int doctrineBonus = CurrentDoctrine().BonusDisposition == choice ? 60 : 0;
            int upgradeBonus = choice == Disposition.Sceller && file.IsAnomalous && HasUpgrade("sceau") ? 70 : 0;
            lastAward = 100 + focus * 20 + currentStreak * 15 + doctrineBonus + upgradeBonus;
            runScore += lastAward;
            PlayerPrefs.SetInt(ArchiveKey, ArchiveCount + 1);
            PlayerPrefs.Save();

            resultText = "DÉCISION CONFORME  /  +" + lastAward + " POINTS";
            resultDetail = ExplanationFor(file);
        }
        else
        {
            currentStreak = 0;
            int penalty = file.IsAnomalous && choice != Disposition.Sceller ? 2 : 1;
            bool failedAnnex = caseIndex >= StandardFiles;
            if (failedAnnex)
            {
                int scoreLoss = Mathf.Max(100, runScore / 4);
                runScore = Mathf.Max(0, runScore - scoreLoss);
                stability = 0;
                resultText = "ANNEXE ROMPUE  /  -" + scoreLoss + " POINTS";
            }
            else if (HasUpgrade("rabat") && !usedSafetyRabat)
            {
                usedSafetyRabat = true;
                penalty = 0;
                stability = Mathf.Max(0, stability - penalty);
                resultText = "ERREUR ABSORBÉE  /  STABILITÉ PRÉSERVÉE";
            }
            else
            {
                stability = Mathf.Max(0, stability - penalty);
                resultText = "DÉCISION REFUSÉE  /  -" + penalty + " STABILITÉ";
            }

            lastMistake = string.Format(
                "{0} : {1} était la décision requise. {2}",
                file.Id,
                DispositionLabel(file.CorrectDisposition),
                ExplanationFor(file));
            resultDetail = ExplanationFor(file);
        }

        if (stability == 0)
        {
            resultDetail += "\nLe service s'arrête : la stabilité du bureau est épuisée.";
        }
    }

    private string ExplanationFor(CaseFile file)
    {
        if (file.IsAnomalous)
            return "L'anomalie confirmée prime sur toute réclamation : l'objet devait être scellé.";
        if (file.HasVerifiedClaim)
            return "Aucune anomalie n'a été confirmée et le sceau du propriétaire est vérifié : l'objet devait être restitué.";
        return "Aucune anomalie ni réclamation vérifiée : l'objet devait rejoindre les archives.";
    }

    private void AdvanceAfterFile()
    {
        if (stability <= 0)
        {
            FinishShift();
            return;
        }

        if (completedFiles >= TotalFiles)
        {
            FinishShift();
            return;
        }

        if (completedFiles % 2 == 0)
        {
            screen = ScreenState.Draft;
            return;
        }

        caseIndex++;
        LoadCurrentFile();
    }

    private void SelectUpgrade(int upgradeIndex)
    {
        if (screen != ScreenState.Draft ||
            upgradeIndex < 0 ||
            upgradeIndex >= AvailableUpgrades.Length ||
            chosenUpgrades.Contains(upgradeIndex))
            return;

        chosenUpgrades.Add(upgradeIndex);

        if (completedFiles == StandardFiles)
        {
            screen = ScreenState.Intermission;
            return;
        }

        caseIndex++;
        LoadCurrentFile();
        screen = ScreenState.File;
    }

    private void OpenAnnex()
    {
        if (stability <= 0)
            return;
        caseIndex = StandardFiles;
        LoadCurrentFile();
        screen = ScreenState.File;
    }

    private void FinishShift()
    {
        if (runScore > PlayerPrefs.GetInt(BestScoreKey, 0))
            PlayerPrefs.SetInt(BestScoreKey, runScore);
        PlayerPrefs.SetInt(RunsKey, PlayerPrefs.GetInt(RunsKey, 0) + 1);
        PlayerPrefs.Save();
        screen = ScreenState.Report;
        paused = false;
    }

    private void ReturnToTitle()
    {
        screen = ScreenState.Title;
        paused = false;
    }

    private void CycleDoctrine()
    {
        int count = UnlockedDoctrineCount();
        if (count > 1)
            selectedDoctrine = (selectedDoctrine + 1) % count;
    }

    private Doctrine CurrentDoctrine()
    {
        int index = Mathf.Clamp(selectedDoctrine, 0, UnlockedDoctrineCount() - 1);
        return Doctrines[index];
    }

    private int UnlockedDoctrineCount()
    {
        int count = 0;
        for (int i = 0; i < Doctrines.Length; i++)
        {
            if (ArchiveCount >= Doctrines[i].UnlockAt)
                count++;
        }
        return Mathf.Max(1, count);
    }

    private int NextDoctrineUnlock()
    {
        for (int i = 0; i < Doctrines.Length; i++)
        {
            if (ArchiveCount < Doctrines[i].UnlockAt)
                return Doctrines[i].UnlockAt;
        }
        return -1;
    }

    private bool HasUpgrade(string id)
    {
        for (int i = 0; i < chosenUpgrades.Count; i++)
        {
            if (AvailableUpgrades[chosenUpgrades[i]].Id == id)
                return true;
        }
        return false;
    }

    private List<int> GetAvailableUpgrades()
    {
        var available = new List<int>();
        for (int i = 0; i < AvailableUpgrades.Length; i++)
        {
            if (!chosenUpgrades.Contains(i))
                available.Add(i);
        }
        return available;
    }

    private string NextFileButtonText()
    {
        if (stability <= 0 || completedFiles >= TotalFiles)
            return "TERMINER LE SERVICE";
        if (completedFiles == StandardFiles)
            return "PASSER À LA DÉCISION DE GARDE";
        if (completedFiles % 2 == 0)
            return "RÉVISER LE PROTOCOLE";
        return "OUVRIR LE DOSSIER SUIVANT";
    }

    private static string DispositionLabel(Disposition disposition)
    {
        switch (disposition)
        {
            case Disposition.Restituer:
                return "RESTITUER";
            case Disposition.Archiver:
                return "ARCHIVER";
            default:
                return "SCELLER";
        }
    }
}
