var positions;
var board;
var lastMove = 0; // Total number of moves in the game
var currentMove = 0;

var spinner = CreateSpinner();

function clickText() {
    $("#m" + currentMove).removeClass("highlightText");
    $(this).addClass("highlightText");
    currentMove = parseInt(this.id.substr(1));
    updateBoard(currentMove);
}

function ParentOfSpinny() {
    return $('#spinner-location');
}

function updateBoard(newMove) {
    $("span#goBack").show();
    $("span#goForward").show();

    if (newMove == 0) {
        $("span#goBack").hide();
    }

    if (positions[newMove + 1] == null) {
        $("span#goForward").hide();
    }

    board.position(positions[newMove].Fen);
}

function PopulateMovesBox() {
    var moveNumber = 1;
    $('#moves').empty();

    for (i = 1; i < lastMove; i+=2) {
        var originalI1 = i;
        var originalI2 = i + 1;

        var text = "<tr><td class=\"movenumber\">" + moveNumber++ + ".</td><td id=\"m" + originalI1 + "\">" + positions[originalI1].Move + "</td> ";
        if(originalI2 < lastMove) {
            text += "<td id=\"m" + originalI2 + "\">" + positions[originalI2].Move + "</td>";
        }
        text += "</tr>";

        $("#moves").append(text);

        $("#m" + originalI1).on("click", clickText);
        $("#m" + originalI2).on("click", clickText);
    }
}

function ConfigureActionButtons() {
    $("span.button").mouseover(function () {
        $(this).parent().fadeTo(40, 1.0);
    }).mouseout(function () {
        $(this).parent().fadeTo(40, 0.7);
    });

    $("#goForward").on("click", function () {
        if (currentMove == lastMove - 1) return;
        updateBoard(currentMove + 1);
        $("#m" + currentMove).removeClass("highlightText");
        currentMove++;
        $("#m" + currentMove).addClass("highlightText");
    });

    $("#goBack").on("click", function () {
        if (currentMove == 0) return;
        updateBoard(currentMove - 1);
        $("#m" + currentMove).removeClass("highlightText");
        currentMove--;
        $("#m" + currentMove).addClass("highlightText");
    });

    $("#goStart").on("click", function () {
        updateBoard(0);
        $("#m" + currentMove).removeClass("highlightText");
        currentMove = 0;
        $("#m" + currentMove).addClass("highlightText");
    });

    $("#goEnd").on("click", function () {
        updateBoard(lastMove - 1);
        $("#m" + currentMove).removeClass("highlightText");
        currentMove = lastMove - 1;
        $("#m" + currentMove).addClass("highlightText");
    });

    $("#goFlip").on("click", function () {
        board.flip();
    });
};