var currentPlayerColor;
var gameId;
var currentTurn;

/* Unicode chess piece characters */
var pieceMapping = {
    "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
    "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
};

var spinner;

function PostMove(start, end, promote) {
    if(start == end)
        return;

    spinner = new Spinner({
        lines: 12, // The number of lines to draw
        length: 7, // The length of each line
        width: 5, // The line thickness
        radius: 10, // The radius of the inner circle
        color: '#B57271', // #rbg or #rrggbb
        speed: 1, // Rounds per second
        trail: 100, // Afterglow percentage
        shadow: false // Whether to render a shadow
    }).spin($("#spinny")[0]);

    $.post("/Board/PlayMove", {
        "id": gameId,
        "Start": start, 
        "End": end, 
        "Promote": promote, 
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val() }
    ).done(ProcessServerResponse);
}

function Resign() {
    if(!confirm("Are you sure you want to resign?")) {
        return;
    }

    $.post("/Board/Resign", {
        "id": gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(ProcessServerResponse);
}

function TellServerGameIsTimedOut(message) {
    $.post("/Board/TimedOut", {
        "id": gameId,
        "message": message,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(ProcessServerResponse);
}

function EndGame() {
    LockBoard();
    if (isTimedGame) {
        clearInterval(myClock.timerId);
        $("div#readybutton").hide();
    }
    $("#turnindicator").text("GAME OVER");
    $("#resign").hide();
}

function UpdateUi(fen) {
    chessBoard.position(fen);

    var turn = fen.split(" ")[1];
    if ($("#turnindicator").text() != "GAME OVER") {
        currentTurn = turn;
        $("#turnindicator").text((turn == "b" ? "Black" : "White") + " to play");
    }

    if ($("#spinny")[0].hasChildNodes()) {
        spinner.stop();
    }
}

function ProcessServerResponse(data) {
    UpdateUi(data.fen);
    $("#messages").text(data.message);
   
    if (data.status == "AUTH") {
        return; /* Not allowed to play on this board */
    }

    if (data.status == "RESIGN" || data.status == "TIME") {
        EndGame();
        return;
    }

    if (data.status != "OK")
        return;

    if (isTimedGame) {
        myClock.SetLastSyncTime(myClock);
        myClock.SyncClockWithServer(myClock);
    }

    $("#lastmove").text(data.movefrom +" -> " + data.moveto);
    $("#fen").text(data.fen);
    UpdateTakenPieces(data.fen);
    
    //crappy
    if(data.message.match(/(draw)|(mate)/i) != null) {
        EndGame();
        return;
    }

    var board = $('#board');

    board.find('.square-55d63').removeClass('highlight-white');
    board.find('.square-' + data.movefrom.toLowerCase()).addClass('highlight-white');
    board.find('.square-' + data.moveto.toLowerCase()).addClass('highlight-white');
}

function LockBoard() {
    chessBoard.draggable = false;
}

function UnlockBoard() {
    chessBoard.draggable = true;
}
   
function UpdateTakenPieces(fen) {
    var initialFen = fen.split(" ")[0];
    var blackArmy = "rrnnbbqpppppppp";
    var whiteArmy = "RRNNBBQPPPPPPPP";
        
    for(var i=0;i<initialFen.length;i++) {
        var c = initialFen[i];
        blackArmy = blackArmy.replace(c, "");
        whiteArmy = whiteArmy.replace(c, "");
    }

    $("#blacktaken").text(blackArmy.split("").map(function(x) { return pieceMapping[x]; }).join(""));
    $("#whitetaken").text(whiteArmy.split("").map(function(x) { return pieceMapping[x]; }).join(""));
}