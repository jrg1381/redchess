var currentPlayerColor;
var gameId;
var currentTurn;

/* Unicode chess piece characters */
var pieceMapping = {
    "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
    "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
};
    
function PostMove(start, end, promote) {
    if(start == end)
        return;
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

function ProcessServerResponse(data) {
    chessBoard.position(data.fen);
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