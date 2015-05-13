var currentPlayerColor;
var gameId;
var currentTurn;
var boardLocked = false;
var chessBoard;
var isTimedGame = false;

/* Unicode chess piece characters */
var pieceMapping = {
    "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
    "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
};

var spinner = CreateSpinner();

function ParentOfSpinny()
{
    if (chessBoard.orientation() == "black") {
        return $('#board').find(".square-e4");
    }
    else if (chessBoard.orientation() == "white") {
        return $('#board').find(".square-d5");
    }
}

function DoMove(source, target, piece) {
    if ((target[1] == '8' && piece == 'wP') || (target[1] == '1' && piece == 'bP')) {
        $("input#Start").val(source.toUpperCase());
        $("input#End").val(target.toUpperCase());
        $("#submitmove form").show();
        $("#Promote").val("Queen");
        return '';
    }

    PostMove(source.toUpperCase(), target.toUpperCase(), $("#Promote option:selected").text());
    $("#submitmove form").hide();
    $("#Promote").val([]);
}

function onDragStart(source, piece, position, orientation) {
    if ((orientation === 'white' && piece.search(/^w/) === -1) ||
        (orientation === 'black' && piece.search(/^b/) === -1) ||
        !$("form").is(":hidden") || piece.search(currentPlayerColor) == -1 || currentTurn != currentPlayerColor || boardLocked) {
        return false;
    }
}

function PostMove(start, end, promote) {
    if(start == end)
        return;

    spinner.spin(ParentOfSpinny()[0]);

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

function ClaimDraw() {
    if(!confirm("Are you sure you want to claim a draw?")) {
        return;
    }

    $.post("/Board/ClaimDraw", {
        "id": gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(ProcessServerResponse);
}

function TellServerGameIsTimedOut(message, timedOutColor) {
    $.post("/Board/TimedOut", {
        "id": gameId,
        "message": message,
        "timedoutcolor" : timedOutColor,
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
    $("#resignbutton").hide();
    $("#drawbutton").hide();
}

function UpdateUi(fen) {
    chessBoard.position(fen);

    var splitFen = fen.split(" ");
    var turn = splitFen[1];

    if ($("#turnindicator").text() != "GAME OVER") {
        currentTurn = turn;
        $("#turnindicator").text((turn == "b" ? "Black" : "White") + " to play");
    }

    if (ParentOfSpinny()[0].hasChildNodes()) {
        spinner.stop();
    }
}

function ProcessServerResponse(data) {
    UpdateUi(data.fen);

    if (data.message) {
        $("#messages").parent().css('visibility', 'visible');
        $("#messages").text(data.message);
    } else {
        $("#messages").parent().css('visibility', 'hidden');
        $("#messages").html('&nbsp;');
    }
   
    if (data.mayClaimDraw) {
        $("#drawbutton").show();
    }

    if (data.status == "AUTH") {
        return; /* Not allowed to play on this board */
    }

    if (data.status == "RESIGN" || data.status == "TIME" || data.status == "DRAW") {
        EndGame();
        return;
    }

    if (data.status != "OK")
        return;

    if (isTimedGame) {
        myClock.SetLastSyncTime(myClock);
        myClock.SyncClockWithServer(myClock);
    }

    $("#lastmove").text(data.lastmove);
    $("#fen").text(data.fen);
    UpdateTakenPieces(data.fen);
    
    var board = $('#board');

    board.find('.square-55d63').removeClass('highlight-white');
    board.find('.square-' + data.movefrom.toLowerCase()).addClass('highlight-white');
    board.find('.square-' + data.moveto.toLowerCase()).addClass('highlight-white');

    //crappy
    if(data.message.match(/(draw)|(mate)/i) != null) {
        EndGame();
        return;
    }
}

function LockBoard() {
    if (boardLocked)
        return;
    boardLocked = true;
    ParentOfSpinny().prepend('<div class="lockIcon"></div>');
}

function UnlockBoard() {
    if (!boardLocked)
        return;
    boardLocked = false;
    $(".lockIcon").remove();
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

    var regexBlack = /p+/;
    var answer = regexBlack.exec(blackArmy);
    if (answer != null && answer[0].length > 1) {
        blackArmy = blackArmy.replace(answer, 'p(' + answer[0].length + ')');
    }
    var regexWhite = /P+/;
    answer = regexWhite.exec(whiteArmy);
    if (answer != null && answer[0].length > 1) {
        whiteArmy = whiteArmy.replace(answer, 'P(' + answer[0].length + ')');
    }

    var whitePawnsTaken = "";
    var blackPawnsTaken = "";

    var partialWhitePieces = whiteArmy.split('P', 2);
    var partialBlackPieces = blackArmy.split('p', 2);
    var whitePieces = partialWhitePieces[0];
    var blackPieces = partialBlackPieces[0];
    if (partialWhitePieces.length > 1) {
        whitePawnsTaken = pieceMapping['P'] + "<span style=\"font-size : medium\">" + partialWhitePieces[1] + "</span>";
    }
    if (partialBlackPieces.length > 1) {
        blackPawnsTaken = pieceMapping['p'] + "<span style=\"font-size : medium\">" + partialBlackPieces[1] + "</span>";
    }

    $("#blacktaken").html(blackPieces.split("").map(function (x) { return pieceMapping[x]; }).join("&#8203;") + blackPawnsTaken);
    $("#whitetaken").html(whitePieces.split("").map(function (x) { return pieceMapping[x]; }).join("&#8203;") + whitePawnsTaken);
}

function DocumentReady() {
    var cfg = { pieceTheme: '/Images/{piece}.png', showNotation: false, draggable: true, onDrop: DoMove, onDragStart: onDragStart };
    chessBoard = new ChessBoard('board', cfg);

    if (currentPlayerColor == 'b') {
        chessBoard.flip();
    }

    //SignalR stuff
    var updater = $.connection.updateServer;
    // Define a client-side message which the server can call
    updater.client.addMessage = function (message) {
        ProcessServerResponse(message);
    };

    // Define a callback for when the connection is established. We join the client group corresponding to our game ID.
    $.connection.hub.start(function () {
        $.connection.hub.proxies.updateserver.server.join(gameId);
    });

    // Hide the promotion UI and set its selection to nothing
    $("#submitmove form").hide();
    $("#Promote").val([]);

    // Hide the claim the draw button
    $("#drawbutton").hide();

    $("#submitmove form").submit(function () {
        PostMove($("input#Start").val(), $("input#End").val(), $("#Promote option:selected").text());
        $("#submitmove form").hide();
        $("#Promote").val([]);
        return false;
    });
}