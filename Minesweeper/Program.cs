using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

class Cell
{
    public bool Mine { get; set; }
    public bool Revealed { get; set; }
    public bool Flagged { get; set; }
    public int MinesAround { get; set; }
}

class Difficulty
{
    public string Name { get; private set; }
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public int Mines { get; private set; }

    public Difficulty(string name, int rows, int cols, int mines)
    {
        Name = name;
        Rows = rows;
        Cols = cols;
        Mines = mines;
    }
}

class ScoreRecord
{
    public int Wins;
    public int Losses;
    public int BestTime;
}

class Scoreboard
{
    const string FileName = "scores.txt";

    Dictionary<string, ScoreRecord> records = new Dictionary<string, ScoreRecord>();

    public Scoreboard()
    {
        Load();
    }

    public ScoreRecord GetRecord(string difficulty)
    {
        if (!records.ContainsKey(difficulty))
        {
            records[difficulty] = new ScoreRecord();
        }

        return records[difficulty];
    }

    public void AddWin(string difficulty, int time)
    {
        ScoreRecord record = GetRecord(difficulty);

        record.Wins++;

        // best time is changed only if this is the first win
        // or if the player finished faster than before.
        if (record.BestTime == 0 || time < record.BestTime)
        {
            record.BestTime = time;
        }

        Save();
    }

    public void AddLoss(string difficulty)
    {
        ScoreRecord record = GetRecord(difficulty);

        record.Losses++;

        Save();
    }

    void Load()
    {
        // if the game is opened for the first time
        // score file does not exist yet.
        if (!File.Exists(FileName))
        {
            return;
        }

        foreach (string line in File.ReadAllLines(FileName))
        {
            string[] parts = line.Split(';');

            // format in file
            // difficulty;wins;losses;bestTime
            if (parts.Length != 4)
            {
                continue;
            }

            int wins;
            int losses;
            int bestTime;

            if (int.TryParse(parts[1], out wins) &&
                int.TryParse(parts[2], out losses) &&
                int.TryParse(parts[3], out bestTime))
            {
                ScoreRecord record = new ScoreRecord();

                record.Wins = wins;
                record.Losses = losses;
                record.BestTime = bestTime;

                records[parts[0]] = record;
            }
        }
    }

    void Save()
    {
        // scores are saved in text file
        // so they stay after closing the game.
        using (StreamWriter writer = new StreamWriter(FileName))
        {
            foreach (string difficulty in records.Keys)
            {
                ScoreRecord record = records[difficulty];

                writer.WriteLine(
                    difficulty + ";" +
                    record.Wins + ";" +
                    record.Losses + ";" +
                    record.BestTime
                );
            }
        }
    }
}

class Board
{
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public int MineCount { get; private set; }
    public bool MinesGenerated { get; private set; }

    public Cell[,] Cells { get; private set; }

    Random random = new Random();

    public Board(int rows, int cols, int mineCount)
    {
        Rows = rows;
        Cols = cols;
        MineCount = mineCount;

        if (MineCount >= Rows * Cols)
        {
            throw new ArgumentException("Too many mines for this board.");
        }

        Cells = new Cell[Rows, Cols];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Cells[r, c] = new Cell();
            }
        }

        // i dont place mines in the constructor
        // because first click should be safe.
    }

    public void GenerateMinesAfterFirstMove(int safeRow, int safeCol)
    {
        if (MinesGenerated)
        {
            return;
        }

        PlaceMines(safeRow, safeCol);
        CountAllNumbers();

        MinesGenerated = true;
    }

    void PlaceMines(int safeRow, int safeCol)
    {
        int placed = 0;

        while (placed < MineCount)
        {
            int r = random.Next(Rows);
            int c = random.Next(Cols);

            // mines are generated after first click
            // so the first clicked cell is never mine.
            if (r == safeRow && c == safeCol)
            {
                continue;
            }

            if (!Cells[r, c].Mine)
            {
                Cells[r, c].Mine = true;
                placed++;
            }
        }
    }

    void CountAllNumbers()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (!Cells[r, c].Mine)
                {
                    Cells[r, c].MinesAround = CountMinesAround(r, c);
                }
            }
        }
    }

    int CountMinesAround(int row, int col)
    {
        int count = 0;

        // we check the 3 by 3 square around this cell.
        // IsInside is needed for cells on the edge of the board.
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (IsInside(r, c) && Cells[r, c].Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public int CountFlagsAround(int row, int col)
    {
        int count = 0;

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (IsInside(r, c) && Cells[r, c].Flagged)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public int CountFlags()
    {
        int count = 0;

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Cells[r, c].Flagged)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool IsInside(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Cols;
    }

    public void Reveal(int row, int col)
    {
        if (!IsInside(row, col))
        {
            return;
        }

        Cell cell = Cells[row, col];

        if (cell.Revealed || cell.Flagged)
        {
            return;
        }

        cell.Revealed = true;

        // empty cells reveal their neighbours recursively
        // like in normal Minesweeper.
        if (!cell.Mine && cell.MinesAround == 0)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r != row || c != col)
                    {
                        Reveal(r, c);
                    }
                }
            }
        }
    }

    public void RevealAllMines()
    {
        // when the player loses i reveal all mines
        // so the player can see where they were.
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Cells[r, c].Mine)
                {
                    Cells[r, c].Revealed = true;
                }
            }
        }
    }

    public bool IsWin()
    {
        // player wins when all non-mine cells are revealed.
        // mines do not have to be flagged.
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (!Cells[r, c].Mine && !Cells[r, c].Revealed)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

class GameWindow : Window
{
    const int ButtonSize = 40;

    Difficulty[] difficulties =
    {
        new Difficulty("Easy", 8, 8, 10),
        new Difficulty("Medium", 12, 12, 25),
        new Difficulty("Hard", 16, 16, 45)
    };

    Board board;
    Button[,] buttons;
    Grid grid;

    ComboBoxText difficultyBox;
    Label timeLabel;
    Label minesLabel;
    Label scoreLabel;
    Label infoLabel;

    Scoreboard scoreboard = new Scoreboard();

    bool gameOver = false;
    bool timerRunning = false;

    int elapsedSeconds = 0;
    uint timerId = 0;

    public GameWindow() : base("Minesweeper")
    {
        DeleteEvent += (sender, args) =>
        {
            StopTimer();
            Gtk.Application.Quit();
        };

        Gtk.Box mainBox = new Gtk.Box(Orientation.Vertical, 6);
        Add(mainBox);

        Gtk.Box topBox = new Gtk.Box(Orientation.Horizontal, 6);
        mainBox.PackStart(topBox, false, false, 5);

        difficultyBox = new ComboBoxText();

        for (int i = 0; i < difficulties.Length; i++)
        {
            difficultyBox.AppendText(difficulties[i].Name);
        }

        difficultyBox.Active = 0;

        difficultyBox.Changed += (sender, args) =>
        {
            NewGame();
        };

        Button restartButton = new Button("Restart");

        restartButton.Clicked += (sender, args) =>
        {
            NewGame();
        };

        Button scoreButton = new Button("Scores");

        scoreButton.Clicked += (sender, args) =>
        {
            ShowScoreboard();
        };

        timeLabel = new Label();
        minesLabel = new Label();
        scoreLabel = new Label();

        topBox.PackStart(new Label("Difficulty:"), false, false, 0);
        topBox.PackStart(difficultyBox, false, false, 0);
        topBox.PackStart(restartButton, false, false, 0);
        topBox.PackStart(scoreButton, false, false, 0);
        topBox.PackStart(timeLabel, false, false, 10);
        topBox.PackStart(minesLabel, false, false, 10);
        topBox.PackStart(scoreLabel, false, false, 10);

        grid = new Grid();
        grid.RowSpacing = 1;
        grid.ColumnSpacing = 1;

        mainBox.PackStart(grid, true, true, 5);

        infoLabel = new Label("Left click opens a cell. Right click places a flag.");
        mainBox.PackStart(infoLabel, false, false, 5);

        NewGame();

        ShowAll();
    }

    Difficulty CurrentDifficulty()
    {
        int index = difficultyBox.Active;

        if (index < 0)
        {
            index = 0;
        }

        return difficulties[index];
    }

    void NewGame()
    {
        StopTimer();

        gameOver = false;
        elapsedSeconds = 0;

        Difficulty difficulty = CurrentDifficulty();

        board = new Board(difficulty.Rows, difficulty.Cols, difficulty.Mines);
        buttons = new Button[difficulty.Rows, difficulty.Cols];

        ClearGrid();
        CreateButtons();

        infoLabel.Text = "Left click opens a cell. Right click places a flag.";

        UpdateButtons();
        UpdateTopLabels();

        SetDefaultSize(
            Math.Max(difficulty.Cols * ButtonSize, 650),
            difficulty.Rows * ButtonSize + 100
        );

        ShowAll();
    }

    void ClearGrid()
    {
        // on restart or difficulty change,
        // i remove old buttons before creating new ones.
        foreach (Widget child in grid.Children)
        {
            grid.Remove(child);
        }
    }

    void CreateButtons()
    {
        for (int r = 0; r < board.Rows; r++)
        {
            for (int c = 0; c < board.Cols; c++)
            {
                Button button = CreateButton(r, c);

                buttons[r, c] = button;
                grid.Attach(button, c, r, 1, 1);
            }
        }
    }

    Button CreateButton(int row, int col)
    {
        Button button = new Button("#");
        button.SetSizeRequest(ButtonSize, ButtonSize);

        button.Clicked += (sender, args) =>
        {
            LeftClick(row, col);
        };

        button.ButtonPressEvent += (sender, args) =>
        {
            // in GTK mouse button 3 means right click.
            // so i use right click for placing and removing flags.
            if (args.Event.Button == 3)
            {
                RightClick(row, col);
                args.RetVal = true;
            }
        };

        return button;
    }

    void LeftClick(int row, int col)
    {
        if (gameOver)
        {
            return;
        }

        Cell cell = board.Cells[row, col];

        if (cell.Flagged)
        {
            return;
        }

        // if player clicks an already revealed number,
        // i try the flag shortcut.
        if (cell.Revealed)
        {
            OpenNeighboursIfFlagsMatch(row, col);
            return;
        }

        OpenHiddenCell(row, col);

        if (!gameOver)
        {
            CheckWin();
        }
    }

    void RightClick(int row, int col)
    {
        if (gameOver)
        {
            return;
        }

        Cell cell = board.Cells[row, col];

        if (cell.Revealed)
        {
            return;
        }

        cell.Flagged = !cell.Flagged;

        UpdateButtons();
        UpdateTopLabels();
    }

    void OpenHiddenCell(int row, int col)
    {
        if (!board.IsInside(row, col))
        {
            return;
        }

        Cell cell = board.Cells[row, col];

        if (cell.Revealed || cell.Flagged)
        {
            return;
        }
        
        // the board creates mines only after the first move.
        // that is why the first opened cell canot be mine.
        if (!board.MinesGenerated)
        {
            board.GenerateMinesAfterFirstMove(row, col);
            StartTimer();
        }

        if (cell.Mine)
        {
            cell.Revealed = true;
            LoseGame();
            return;
        }

        board.Reveal(row, col);

        UpdateButtons();
        UpdateTopLabels();
    }

    void OpenNeighboursIfFlagsMatch(int row, int col)
    {
        Cell cell = board.Cells[row, col];

        if (!cell.Revealed || cell.MinesAround == 0)
        {
            return;
        }

        int flagsAround = board.CountFlagsAround(row, col);

        // if the number of flags around this cell is correct
        // we open the other neighbours automaticlly.
        // if flags are wrong this can still open mine.
        if (flagsAround != cell.MinesAround)
        {
            return;
        }

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (!board.IsInside(r, c))
                {
                    continue;
                }

                if (r == row && c == col)
                {
                    continue;
                }

                Cell neighbour = board.Cells[r, c];

                if (!neighbour.Revealed && !neighbour.Flagged)
                {
                    OpenHiddenCell(r, c);

                    if (gameOver)
                    {
                        return;
                    }
                }
            }
        }

        CheckWin();
    }

    void CheckWin()
    {
        if (board.IsWin())
        {
            gameOver = true;
            StopTimer();

            scoreboard.AddWin(CurrentDifficulty().Name, elapsedSeconds);

            infoLabel.Text = "You won!";
            UpdateButtons();
            UpdateTopLabels();

            ShowMessage("You won!");
        }
    }

    void LoseGame()
    {
        gameOver = true;
        StopTimer();

        board.RevealAllMines();

        scoreboard.AddLoss(CurrentDifficulty().Name);

        infoLabel.Text = "Game over!";
        UpdateButtons();
        UpdateTopLabels();

        ShowMessage("Game over!");
    }

    void StartTimer()
    {
        if (timerRunning)
        {
            return;
        }

        timerRunning = true;

        // this runs once every second
        // returning true keeps the timer running
        // returning false stops it
        
        timerId = GLib.Timeout.Add(1000, () =>
        {
            if (gameOver)
            {
                timerRunning = false;
                timerId = 0;
                return false;
            }

            elapsedSeconds++;
            UpdateTopLabels();

            return true;
        });
    }

    void StopTimer()
    {
        if (timerId != 0)
        {
            GLib.Source.Remove(timerId);
            timerId = 0;
        }

        timerRunning = false;
    }

    void UpdateButtons()
    {
        // this updates the GTK buttons from current board state
        // Board class stores the logic and this method shows it.
        for (int r = 0; r < board.Rows; r++)
        {
            for (int c = 0; c < board.Cols; c++)
            {
                Cell cell = board.Cells[r, c];
                Button button = buttons[r, c];

                if (cell.Revealed)
                {
                    if (cell.Mine)
                    {
                        button.Label = "*";
                    }
                    else if (cell.MinesAround == 0)
                    {
                        button.Label = ".";
                    }
                    else
                    {
                        button.Label = cell.MinesAround.ToString();
                    }
                }
                else if (cell.Flagged)
                {
                    button.Label = "F";
                }
                else
                {
                    button.Label = "#";
                }

                button.Sensitive = !gameOver;
            }
        }
    }

    void UpdateTopLabels()
    {
        Difficulty difficulty = CurrentDifficulty();
        ScoreRecord record = scoreboard.GetRecord(difficulty.Name);

        int minesLeft = difficulty.Mines - board.CountFlags();

        timeLabel.Text = "Time: " + FormatTime(elapsedSeconds);
        minesLabel.Text = "Mines left: " + minesLeft;

        string bestText = "-";

        if (record.BestTime > 0)
        {
            bestText = FormatTime(record.BestTime);
        }

        scoreLabel.Text =
            "Score: " +
            record.Wins + "W / " +
            record.Losses + "L / best " +
            bestText;
    }

    string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int rest = seconds % 60;

        return minutes + ":" + rest.ToString("00");
    }

    void ShowScoreboard()
    {
        string text = "";

        for (int i = 0; i < difficulties.Length; i++)
        {
            Difficulty difficulty = difficulties[i];
            ScoreRecord record = scoreboard.GetRecord(difficulty.Name);

            string bestText = "-";

            if (record.BestTime > 0)
            {
                bestText = FormatTime(record.BestTime);
            }

            text += difficulty.Name + ": ";
            text += record.Wins + " wins, ";
            text += record.Losses + " losses, ";
            text += "best time " + bestText + "\n";
        }

        ShowMessage(text);
    }

    void ShowMessage(string text)
    {
        MessageDialog dialog = new MessageDialog(
            this,
            DialogFlags.Modal,
            MessageType.Info,
            ButtonsType.Ok,
            text
        );

        dialog.Run();
        dialog.Destroy();
    }
}

class Program
{
    static void Main()
    {
        Gtk.Application.Init();
        new GameWindow();
        Gtk.Application.Run();
    }
}