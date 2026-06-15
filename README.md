# projectpr2
This is my Minesweeper project written in C# with GTK.

The game works like normal Minesweeper. You reveal cells, avoid mines, and use flags to mark places where you think mines are hidden.

How to play:
Left click reveals a cell.
Right click places or removes a flag.
The symbols mean:
"#"  hidden cell
F  flagged cell
.  empty revealed cell
1-8 number of mines around the cell
"*"  mine

You win when all cells without mines are revealed.
You lose if you reveal a mine.
Difficulties:

There are three difficulty levels:

Easy    8 x 8 board, 10 mines
Medium  12 x 12 board, 25 mines
Hard    16 x 16 board, 45 mines

The difficulty can be changed at the top of the window.

First click:
The first click is always safe.

Flags:
Flags stop the player from accidentally opening a cell.
There is also one extra Minesweeper shortcut. If a revealed number has the correct number of flags around it, clicking that revealed number opens the other neighbouring cells.
For example, if a cell shows 2 and there are exactly two flags around it, clicking the 2 will open the other cells around it.
If the flags are wrong, this can still open a mine.

Timer:
The timer starts after the first move.
It stops when the player wins or loses.

Restart:
The Restart button starts a new game with the current difficulty.

Scoreboard:
The game saves:
wins
losses
best time
The scores are saved separately for each difficulty.
The program saves them in a file called: scores.txt
This file is created automatically after playing.

Controls:
Left click              reveal cell
Right click             place/remove flag
Left click on number    use flag shortcut
Restart button          start new game
Scores button           show scoreboard
Difficulty menu         change difficulty
