﻿function onDocumentReady() {
    var spinny = getSpinController();
    spinny.startLogoSpinner();

    $.getJSON("/api/Elo")
        .always(function () {
            spinny.stopLogoSpinner();

        })
        .done(function (data) {
            drawEloTable(data);
            drawWinLossTable(data.WinLoss);
            drawWinLossPointsTable(data.WinLoss);
            drawGraph(data);
            updateLastUpdated(data.LastUpdated);
        });
};

function updateLastUpdated(data) {
    $('time#last-updated').attr("datetime", data);
    $('time#last-updated').text(data);
    $("time.timeago").timeago();
}

function drawWinLossTable(data) {
    for (var property in data) {
        if (data.hasOwnProperty(property)) {
            var row = $("<tr></tr>");
            row.append($("<td>" + data[property].White + "</td>"));
            row.append($("<td>" + data[property].Black + "</td>"));
            row.append($("<td>" + data[property].Winner + "</td>"));
            row.append($("<td>" + data[property].Count + "</td>"));
            $("#winloss-table>tbody").append(row);
        }
    }
}

function drawWinLossPointsTable(data) {
    var allUsers = new Object();

    for (var property in data) {
        if (data.hasOwnProperty(property)) {
            var row = data[property];

            if (!allUsers[row.White]) {
                allUsers[row.White] = new Object();
                allUsers[row.White]["Wins"] = 0;
                allUsers[row.White]["Losses"] = 0;
                allUsers[row.White]["Draws"] = 0;
            }

            if (!allUsers[row.Black]) {
                allUsers[row.Black] = new Object();
                allUsers[row.Black]["Wins"] = 0;
                allUsers[row.Black]["Losses"] = 0;
                allUsers[row.Black]["Draws"] = 0;
            }

            if (row.Winner == null) {
                allUsers[row.White]["Draws"] += row.Count;
                allUsers[row.Black]["Draws"] += row.Count;
            } else {
                var loser = row.Winner === row.White ? row.Black : row.White;
                allUsers[row.Winner]["Wins"] += row.Count;
                allUsers[loser]["Losses"] += row.Count;
            }
        }
    }

    for (var u in allUsers) {
        if (allUsers.hasOwnProperty(u)) {
            var q = allUsers[u];
            var row = $("<tr></tr>");
            row.append($("<td>" + u + "</td>"));
            row.append($("<td>" + q["Wins"] + "</td>"));
            row.append($("<td>" + q["Losses"] + "</td>"));
            row.append($("<td>" + q["Draws"] + "</td>"));
            row.append($("<td>" + ((q["Draws"] * 0.5) + q["Wins"]) + "</td>"));
            $("#winloss-points-table>tbody").append(row);
        }
    }
}

function drawEloTable(data) {
    var eloData = data.EloData;
    var color = d3.scale.category10();

    for (var property in eloData) {
        if (eloData.hasOwnProperty(property)) {
            // The server guarantees that the data comes back already sorted
            var latestElo = eloData[property][eloData[property].length - 1].Elo;
            var previousElo = eloData[property][eloData[property].length - 2].Elo;
            var filterFunction = function (x) { return x.UserId == property; };
            var userName = data.Profiles.filter(filterFunction)[0].UserName;
            var row = $("<tr></tr>");
            row.append($('<td><span class="glyphicon glyphicon-user" style="padding-right:0.5em;color :' + color(userName) + '" aria-hidden="true"></span>' + userName + "</td>"));
            row.append($("<td>" + latestElo + "</td>"));
            row.append($("<td>" + (latestElo - previousElo) + "</td>"));
            $("#ratings-table>tbody").append(row);
        }
    }
};

function drawGraph(data) {
    var eloMax = 0, eloMin = Infinity;
    var dateMax = new Date(0), dateMin = new Date();

    for (var property in data.EloData) {
        if (data.EloData.hasOwnProperty(property)) {
            var mappedData = data.EloData[property].map(function (x) { return { Date: new Date(x.Date), Elo: x.Elo } });

            eloMax = Math.max(eloMax, d3.max(mappedData, function (x) { return x.Elo; }));
            eloMin = Math.min(eloMin, d3.min(mappedData, function (x) { return x.Elo; }));
            dateMax = new Date(Math.max(dateMax, d3.max(mappedData, function (x) { return x.Date; })));
            dateMin = new Date(Math.min(dateMin, d3.min(mappedData, function (x) { return x.Date; })));
        }
    }

    var width = $("#svg").width();
    var height = window.screen.availHeight / 1.5;
    var svg = d3.select("#svg").append("svg").attr("width", width).attr("height", height);

    var x = d3.time.scale().domain([dateMin, dateMax]).range([0, width]);
    var y = d3.scale.linear().domain([eloMin - 200, eloMax + 100]).range([height, 0]);
    var xAxis = d3.svg.axis().scale(x).orient("bottom");
    var yAxis = d3.svg.axis().scale(y).orient("right");
    svg.append("g").attr("class", "x-axis").attr("transform", "translate(0," + (height - 32) + ")").call(xAxis);
    svg.append("g").attr("class", "x-axis").call(yAxis);
    var color = d3.scale.category10();

    for (var prop in data.EloData) {
        if (data.EloData.hasOwnProperty(prop)) {
            var mappedData = data.EloData[prop].map(function (x) { return { Date: new Date(x.Date), Elo: x.Elo } });

            var line = d3.svg.line().x(function (d, i) { return x(d.Date); }).y(function (d, i) { return y(d.Elo); });
            svg.append("path").attr("d", line(mappedData)).style("stroke", function (d) { return color(prop); });
        }
    }
}

