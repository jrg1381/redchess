function movesWhichAreMateInN(evaluationData) {
    return evaluationData.filter(function (value) {
        return value.scoreType === 1;
    }).map(function (x) {
        return x.move - 1;
    });
};

function showGraph(boardViewer, evaluationData, gameOver, status) { // domain([d3.min(evaluationData), d3.max(evaluationData)])
    var width = 512;
    var height = 64;

    if (evaluationData.length <= 1) {
        console.log("Danger: Not enough evaluation data in JSON");
        return;
    }

    // Because it might come out of the database in any order
    evaluationData.sort(function (a, b) { return (a.move < b.move) ? -1 : (a.move > b.move) ? 1 : 0; });

    // Scale off anything above 2500
    var absoluteMax = Math.min(2500, d3.max(evaluationData, function (p) { return Math.abs(p.score); }));

    var svg = d3.select("#svg").append("svg").attr("width", width).attr("height", height).append("g").data(evaluationData);
    var x = d3.scale.linear().domain([0, evaluationData.length]).range([0, width]);
    var y = d3.scale.linear().domain([-absoluteMax, absoluteMax]).range([height, 0]);
    var xAxis = d3.svg.axis().scale(x).orient("bottom").tickFormat(d3.format("d"));
    var line = d3.svg.line().x(function (d, i) { return x(d.move); }).y(function (d, i) { return y(d.score); });

    svg.append("g").attr("class", "x-axis").attr("transform", "translate(0,32)").call(xAxis);
    svg.append("path").attr("d", line(evaluationData));
    var x0 = x(evaluationData[0].move);
    var y0 = y(evaluationData[0].score);
    svg.append("circle").attr("r", 5).attr("cx", x0).attr("cy", y0).attr("class", "blob");

    // Mark the zones where forced mate is possible
    var movesWhichAreMate = movesWhichAreMateInN(evaluationData);
    // Width of a single move. TODO: optimize contiguous ranges into single rectangles
    var dx = x(evaluationData[1].move) - x(evaluationData[0].move);
    for (var i = 0; i < movesWhichAreMate.length; i++) {
        svg.append("rect")
            .attr("width", dx)
            .attr("height", 64)
            .attr("x", x(evaluationData[movesWhichAreMate[i]].move))
            .attr("class", "mate-zone");
    }

    // Set up clicking
    $("svg").on('click', function (d) {
        var xFromClick = Math.round(x.invert(d.offsetX));
        boardViewer.fakeClick(xFromClick);
    });

    boardViewer.onMoveSelected(function (moveNumber) {
        window.location.hash = moveNumber - 1;

        var svgElement = d3.select("svg");
        svgElement.selectAll("circle").remove();
        svgElement.selectAll("text").remove();

        if (moveNumber > evaluationData.length - 1) {
            if (gameOver) {
                $("#upper-analysis").html("Game over<br\>" + status);
            } else {
                $("#upper-analysis").text("No analysis for this move");
            }

            $("#lower-analysis").text("");
            return;
        }

        var yPosition;
        var xPosition = x(moveNumber);
        if (evaluationData[moveNumber].scoreType === 1) { // corresponds to enum MateInN
            var score = evaluationData[moveNumber].score;
            var whiteWin = Math.sign(score) === 1;
            var mateClass = whiteWin ? "matewhite" : "mateblack";
            yPosition = y0;
            svgElement.append("circle").attr("r", 5).attr("cx", xPosition).attr("cy", yPosition).attr("class", mateClass);
            svgElement.append("text").attr("fill", "red").attr("x", xPosition).attr("y", yPosition - 10).text(Math.abs(score));
            var indicator = whiteWin ? "&#x2654;" : "&#x265a;"; // White king / Black king unicode symbols
            $("#upper-analysis").html(indicator + " Mate in " + Math.abs(score));
        } else {
            yPosition = y(evaluationData[moveNumber].score);
            svgElement.append("circle").attr("r", 5).attr("cx", xPosition).attr("cy", yPosition).attr("class", "blob");
            $("#upper-analysis").text("Score : " + evaluationData[moveNumber].score);
        };

        $("#analysisMoves").remove();

        if ("lines" in evaluationData[moveNumber] && evaluationData[moveNumber].lines.length > 0) {
            $("#lower-analysis").append("<table id=\"analysisMoves\"></table>");
            var analysisMoves = $("#analysisMoves");

            function functionMaker(f, link) {
                return function () {
                    boardViewer.setFen(f);
                    $("table#analysisMoves td").css("font-weight", "normal");
                    link.css("font-weight", "bold");
                };
            };

            var lastMove = evaluationData[moveNumber].lines.length;
            var moveNumberDisplay = evaluationData[moveNumber].lines[0].MoveNumber;

            for (var i = 0; i < lastMove; i += 2) {
                var originalI1 = i;
                var originalI2 = i + 1;
                var evaluationLine2 = "";
                var evaluationLine1 = evaluationData[moveNumber].lines[originalI1];

                var text = "";
                var fullMoveNumber = 0;

                if (moveNumberDisplay % 2 === 0) {
                    fullMoveNumber = moveNumberDisplay / 2;
                } else {
                    fullMoveNumber = (moveNumberDisplay + 1) / 2;
                }

                if (i === 0 && moveNumberDisplay % 2 === 1) {
                    text += "<tr><td class=\"movenumber\">" + fullMoveNumber + ".</td>";
                    text += "<td>...</td>";
                    text += "<td id=\"em" + originalI1 + "\">" + evaluationLine1.Move + "</td>";
                    i--; // Because we've only consumed a single analysis line
                }
                else {
                    text += "<tr><td class=\"movenumber\">" + fullMoveNumber + ".</td>";
                    text += "<td id=\"em" + originalI1 + "\">" + evaluationLine1.Move + "</td>";
                    if (originalI2 < lastMove) {
                        evaluationLine2 = evaluationData[moveNumber].lines[originalI2];
                        text += "<td id=\"em" + originalI2 + "\">" + evaluationLine2.Move + "</td>";
                    }
                }

                analysisMoves.append(text);

                var firstMove = $("td#em" + originalI1);
                firstMove.on("click", functionMaker(evaluationLine1.Fen, firstMove));
                if (originalI2 < lastMove) {
                    var secondMove = $("td#em" + originalI2);
                    secondMove.on("click", functionMaker(evaluationLine2.Fen, secondMove));
                }

                moveNumberDisplay += 2;
            }
        }
    });
}

function actOnMoveInUrlHash(event, boardViewer) {
    if (window.location.hash) {
        var moveToJumpTo = window.location.hash.substring(1);
        var integerMove = parseInt(moveToJumpTo);
        if (integerMove !== NaN) {
            // Pretend that the move has been clicked
            boardViewer.fakeClick(integerMove);
        }
    }
}

function onDocumentReady(gameId) {
    var cfg = { pieceTheme: '/Images/{piece}.png', position: '', showNotation: false };
    var board = new ChessBoard('board', cfg);
    var emptyData = { Moves: [{ Fen: "", Move: "" }], Description: "", IsParticipant: false };
    var boardViewer = new BoardViewer(emptyData, board, gameId);
    boardViewer.startSpinning();

    $.getJSON("/api/Moves/" + gameId)
        .always(function() {
            boardViewer.stopSpinning();
        })
        .done(function(data) {
            var moves = data.Moves.length;
            var analyzedMoves = data.Analysis.length;
            var winner = data.Winner;

            // They're allowed to differ by 2 because 'moves' includes the terminal move which has no corresponding analysis and the first 'move' which is the starting position
            if (moves - analyzedMoves >= 2) {
                $("#analysisTitle").text("Analysis (completed " + analyzedMoves + "/" + moves + ")");
            }

            boardViewer = new BoardViewer(data, board, gameId);

            window.addEventListener('popstate', function(event) {
                actOnMoveInUrlHash(event, boardViewer);
            });

            // We add an extra move 0 to make the graph start at evens, so moveNumber must be incremented
            var evaluationData = data.Analysis.map(function(x) { return { move: x.MoveNumber + 1, score: x.Evaluation, scoreType: x.BoardEvaluationType, lines: x.AnalysisLines }; });

            // Start data at zero. Make sure scoretype is invalid number.
            evaluationData.unshift({ move: 0, score: 0, scoreType: -99 });
            showGraph(boardViewer, evaluationData, data.GameOver, data.Status);

            boardViewer.highlightMoves(movesWhichAreMateInN(evaluationData));
            // Go to the move specified in the URL, if there is one
            actOnMoveInUrlHash(null, boardViewer);

            var description = data.Description;

            if (winner === "w") {
                description = "<b>" + description.replace(" vs ", "</b> vs ");
            } else if (winner === "b") {
                description = description.replace(" vs ", " vs <b>") + "</b>";
            }

            $('#title').html(description);
            $('span#desc').html(description);
            $("#pgn-button").click(function() {
                location.href = "/History/Pgn/" + gameId;
            });

            $('#fen-button')
            .popover({
                    content: function() {
                        return boardViewer.positions.Moves[boardViewer.currentMove].Fen;
                    }
                });

        }).fail(function(jqXHR, textStatus, errorThrown) {
            $('span#desc').text(jqXHR.status + " " + errorThrown);
            if (jqXHR.responsejSON !== undefined) {
                $("table#moves").append("<tr><td>" + jqXHR.responseJSON.message + "</td></tr>");
                $("table#moves").append("<tr><td>" + jqXHR.responseJSON.messageDetail + "</td></tr>");
            }
            $("#pgn-button").hide();
        });
};