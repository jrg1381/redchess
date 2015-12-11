function BoardViewer(positions, board, gameId) {
    this.positions = positions;
    this.board = board;
    this.currentMove = 0;
    this.gameId = gameId;

    var that = this;

    function clickText() {
        $("#m" + that.currentMove).removeClass("highlightText");
        $(this).addClass("highlightText");
        that.currentMove = parseInt(this.id.substr(1));
        // Remove the boldness on the variant lines
        $("table#analysisMoves td").css("font-weight", "normal");
        that.updateBoard(that.currentMove);
    }

    function populateMovesBox() {
        var lastMove = positions.Moves.length;
        var moveNumber = 1;
        $("#moves").empty();

        for (var i = 1; i < lastMove; i += 2) {
            var originalI1 = i;
            var originalI2 = i + 1;

            var text = "<tr><td class=\"movenumber\">" + moveNumber++ + ".</td><td id=\"m" + originalI1 + "\">" + positions.Moves[originalI1].Move + "</td> ";
            if (originalI2 < lastMove) {
                text += "<td id=\"m" + originalI2 + "\">" + positions.Moves[originalI2].Move + "</td>";
            }
            text += "</tr>";

            $("#moves").append(text);

            $("#m" + originalI1).on("click", clickText);
            $("#m" + originalI2).on("click", clickText);
        }
    }

    function configureActionButtons() {
        var lastMove = positions.Moves.length;

        // Remove handlers, as this function can be called twice if two BoardViewers 
        // are constructed and they point at the same underlying UI elements.
        $("#goForward").off();
        $("#goBack").off();
        $("#goStart").off();
        $("#goFlip").off();
        $("#goEnd").off();

        $("span.button").mouseover(function () {
            $(this).parent().fadeTo(40, 1.0);
        }).mouseout(function () {
            $(this).parent().fadeTo(40, 0.7);
        });

        $("#goForward").on("click", function () {
            if (that.currentMove === lastMove - 1) return;
            that.updateBoard(that.currentMove + 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove++;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goBack").on("click", function () {
            if (that.currentMove === 0) return;
            that.updateBoard(that.currentMove - 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove--;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goStart").on("click", function () {
            that.updateBoard(0);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove = 0;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goEnd").on("click", function () {
            that.updateBoard(lastMove - 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove = lastMove - 1;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goFlip").on("click", function () {
            that.board.flip();
        });

        $("#playFromHere").on("click", function () {
            window.location = "/History/PlayFromHere?move=" + that.currentMove + "&gameId=" + that.gameId;
        });
    };

    populateMovesBox();
    configureActionButtons();
    board.position(this.positions.Moves[0].Fen);
    this.updateBoard(0);
}

BoardViewer.prototype.positions = null;
BoardViewer.prototype.callbacks = [];

BoardViewer.prototype.getSpinner = function() {
    if (this.spinner == null) {
        this.spinner = CreateSpinner();
    }
    return this.spinner;
};

BoardViewer.prototype.fakeClick = function(moveNumber) {
    $("#m" + moveNumber).click();
}

BoardViewer.prototype.onMoveSelected = function (c) {
    if (!$.isFunction(c)) {
        console.error("Attempt to add callback object which wasn't a function.");
        return;
    }
    this.callbacks.push(c);
}

BoardViewer.prototype.fireCallbacks = function(moveNumber) {
    for (var i = 0; i < this.callbacks.length; i++) {
        this.callbacks[i](moveNumber + 1);
    }
}

BoardViewer.prototype.startSpinning = function() {
    this.getSpinner().spin($("#spinner-location")[0]);
};

BoardViewer.prototype.stopSpinning = function() {
    this.getSpinner().stop();
};

BoardViewer.prototype.updateBoard = function(newMove) {
    var lastMove = this.positions.Moves.length;

    $("span#goBack").show();
    $("span#goForward").show();

    if (newMove === 0) {
        $("span#goBack").hide();
    }

    if (newMove === lastMove - 1) {
        $("span#goForward").hide();
    }

    this.fireCallbacks(newMove);
    this.board.position(this.positions.Moves[newMove].Fen);
};

BoardViewer.prototype.setFen = function(fen) {
    this.board.position(fen);
};

