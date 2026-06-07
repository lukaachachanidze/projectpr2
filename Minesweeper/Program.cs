using System;
using Gtk;

class Cell
{
    public bool Mine;
    public bool Revealed;
    public bool Flagged;
    public int MinesAround;
}

class Board
{
    public int Rows;
    public int Cols;
    public int MineCount;

    public Cell[,] Cells;

    Random random = new Random();

    public Board(int rows, int cols, int mineCount)
    {
        Rows = rows;
        Cols = cols;
        MineCount = mineCount;

        Cells = new Cell[Rows, Cols];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Cells[r, c] = new Cell();
            }
        }

        PlaceMines();
        CountAllNumbers();
    }

    void PlaceMines()
    {
        int placed = 0;

        while (placed < MineCount)
        {
            int r = random.Next(Rows);
            int c = random.Next(Cols);

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

    public bool IsWin()
    {
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
    const int Rows = 8;
    const int Cols = 8;
    const int Mines = 10;
    const int ButtonSize = 40;

    Board board;
    Button[,] buttons;
    Grid grid;

    bool gameOver = false;

    public GameWindow() : base("Minesweeper")
    {
        SetDefaultSize(Cols * ButtonSize, Rows * ButtonSize);

        DeleteEvent += (sender, args) =>
        {
            Gtk.Application.Quit();
        };

        board = new Board(Rows, Cols, Mines);
        buttons = new Button[Rows, Cols];

        grid = new Grid();
        Add(grid);

        CreateButtons();

        ShowAll();
    }

    void CreateButtons()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
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

        if (cell.Mine)
        {
            gameOver = true;
            ShowMines();
            ShowMessage("Game over!");
            return;
        }

        board.Reveal(row, col);
        UpdateButtons();

        if (board.IsWin())
        {
            gameOver = true;
            ShowMessage("You won!");
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
    }

    void UpdateButtons()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Cell cell = board.Cells[r, c];
                Button button = buttons[r, c];

                if (cell.Flagged)
                {
                    button.Label = "F";
                }
                else if (!cell.Revealed)
                {
                    button.Label = "#";
                }
                else if (cell.MinesAround == 0)
                {
                    button.Label = ".";
                    button.Sensitive = false;
                }
                else
                {
                    button.Label = cell.MinesAround.ToString();
                    button.Sensitive = false;
                }
            }
        }
    }

    void ShowMines()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (board.Cells[r, c].Mine)
                {
                    buttons[r, c].Label = "*";
                }
            }
        }
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